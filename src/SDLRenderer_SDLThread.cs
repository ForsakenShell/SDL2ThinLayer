/*
 * SDLRenderer_SDLThread.cs
 *
 * Everything thread related to the class is handled here.  Don't mess with this part of the class
 * unless you fully understand the magic of multi-threading.
 *
 * User: 1000101
 * Date: 28/01/2018
 * Time: 3:00 AM
 * 
 */
using System;
using System.Diagnostics;
using System.Threading;

using SDL2;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        #region Internal:  Thread state enum
        
        enum SDLThreadState
        {
            Inactive = 0,
            Starting = 1,
            Running = 2,
            Stopping = 3,
            Paused = 4,
            Error = -1
        }
        
        #endregion
        
        #region Internal:  Thread level state machine variables
        
        Thread _sdlThread;
        SDLThreadState _threadState;
        bool _exitRequested;
        bool _pauseThread;
        bool _windowResetRequired;
        
        Stopwatch _threadTimer;
        long _drawTicks;
        long _eventTicks;
        long _baseFrameDelay;
        
        ImageTypes _windowSaveFormat;
        string _windowSaveFilename;
        bool _windowSaveRequested;
        bool _windowSaved;
            
        #endregion
        
        #region Internal:  Performance feedback variables
        
        int _actualFPS;
        int _potentialFPS; // Take this with a grain of salt
        long _averageFrameTime;
        
        #endregion
        
        #region Public API:  SDLRenderer Thread Update frequency
        
        public int DrawsPerSecond
        {
            get
            {
                return _drawsPS;
            }
            set
            {
                if( value <= 0.0d ) return;
                if( value > MAX_UPDATES_PER_SECOND ) return;
                _drawsPS = value;
                INTERNAL_UpdateState_ThreadIntervals();
            }
        }
        
        public int EventsPerSecond
        {
            get
            {
                return _eventsPS;
            }
            set
            {
                if( value <= 0.0d ) return;
                if( value > MAX_UPDATES_PER_SECOND ) return;
                _eventsPS = value;
                INTERNAL_UpdateState_ThreadIntervals();
            }
        }
        
        #endregion
        
        #region Public API:  Performance Feedback Properties
        
        public int ActualFPS
        {
            get
            {
                return _actualFPS;
            }
        }
        
        public int PotentialFPS
        {
            get
            {
                return _potentialFPS;
            }
        }
        
        public long AverageFrameTimeTicks
        {
            get
            {
                return _averageFrameTime;
            }
        }
        
        public long AverageFrameTimeMS
        {
            get
            {
                return ( _averageFrameTime / TimeSpan.TicksPerMillisecond );
            }
        }
        
        #endregion
        
        #region Internal:  Thread State
        
        // These are the only properties/functions that should be called outside of the SDLThread
        
        bool INTERNAL_SDLThread_Active
        {
            get
            {
                return _threadState > SDLThreadState.Inactive;
            }
        }
        
        bool INTERNAL_SDLThread_Starting
        {
            get
            {
                return _threadState == SDLThreadState.Starting;
            }
        }
        
        bool INTERNAL_SDLThread_Running
        {
            get
            {
                return _threadState == SDLThreadState.Running;
            }
        }
        
        bool INTERNAL_SDLThread_Paused
        {
            get
            {
                return _threadState == SDLThreadState.Paused;
            }
        }
        
        bool INTERNAL_SDLThread_Stopping
        {
            get
            {
                return _threadState == SDLThreadState.Stopping;
            }
        }
        
        #endregion
        
        #region Public API:  SDLThread Pause/Resume
        
        /// <summary>
        /// Tells the SDLThread to pause and will not return until it does or it can't.
        ///
        /// DO NOT CALL THIS FROM THE SDLThread!
        /// </summary>
        public bool SDLThread_Pause()
        {
            if( !INTERNAL_SDLThread_Running ) return false;
            
            _pauseThread = true;
            while( !INTERNAL_SDLThread_Paused )
                Thread.Sleep( 0 );
            
            return true;
        }
        
        /// <summary>
        /// Tells the SDLThread to resume and will not return until it does or it can't.
        ///
        /// DO NOT CALL THIS FROM THE SDLThread!
        /// </summary>
        public bool SDLThread_Resume()
        {
            if( !INTERNAL_SDLThread_Paused ) return false;
            
            _pauseThread = false;
            while( !INTERNAL_SDLThread_Running )
                Thread.Sleep( 0 );
            
            return true;
        }
        
        #endregion
        
        #region Internal:  WaitFor[TypeValue]
        
        /// <summary>
        /// Tells the current thread to wait for the SDLThread to set a specific internal variable to a specific value.
        ///
        /// DO NOT CALL THIS FROM THE SDLThread!
        /// </summary>
        void INTERNAL_SDLThread_WaitForBool( ref bool toWaitFor, bool initialValue, bool waitValue, int releaseMS )
        {
            toWaitFor = initialValue;
            while( toWaitFor != waitValue )
                Thread.Sleep( releaseMS );
        }
        
        #endregion
        
        #region Internal:  SDLRenderer Thread Main Methods
        
        // All code here should be running in it's own thread created in INTERNAL_Init_SDLThread()
        // and should never be called outside of the thread itself.
        
        void INTERNAL_SDLThread_Main()
        {
            //Console.Write( "INTERNAL_SDLThread_Main()\n" );
            
            _threadState = SDLThreadState.Starting;
            
            // Request some User Event IDs
            _sdlUEID_Invoke_NoParams = SDL.SDL_RegisterEvents( 1 );
            if( _sdlUEID_Invoke_NoParams == 0xFFFFFFFF )
            {
                INTERNAL_SDLThread_Cleanup( SDLThreadState.Error );
                return;
            }
            _sdlUEID_BeginInvoke_NoParams = SDL.SDL_RegisterEvents( 1 );
            if( _sdlUEID_BeginInvoke_NoParams == 0xFFFFFFFF )
            {
                INTERNAL_SDLThread_Cleanup( SDLThreadState.Error );
                return;
            }
            
            // Create the SDL window and renderer
            var wrStartedOk = INTERNAL_SDLThread_InitWindowAndRenderer();
            if( !wrStartedOk )
            {
                INTERNAL_SDLThread_Cleanup( SDLThreadState.Error );
                return;
            }
            
            // Translate the state machine into something meaningful to SDL
            INTERNAL_UpdateState_FunctionPointers();
            INTERNAL_UpdateState_CursorVisibility();
            INTERNAL_UpdateState_ThreadIntervals();
            
            // Now do the main thread loop
            INTERNAL_SDLThread_MainLoop();
            
            // Clean up after the thread
            INTERNAL_SDLThread_Cleanup( SDLThreadState.Inactive );
            
        }
        
        void INTERNAL_SDLThread_MainLoop()
        {
            //Console.Write( "INTERNAL_SDLThread_MainLoop()\n" );
            
            TimeSpan loopTime = TimeSpan.FromTicks( 0 );
            long loopStartTick = 0;
            long loopDelayTicks = 0;
            long loopEndTick = 0;
            TimeSpan loopSleepTime;
            int loopSleepMS = 0;
            long lastDrawTick = 0;
            long lastEventTick = 0;
            long drawTickDelta = 0;
            long eventTickDelta = 0;
            
            long lastFPSCount = 0;
            long fpsTicks = 0;
            long frameStart = 0;
            long frameEnd = 0;
            long frameTime = 0;
            
            // Thread timing
            _threadTimer = new Stopwatch();
            
            // Mark the thread as running
            _threadState = SDLThreadState.Running;
            _threadTimer.Start();
            
            // Loop until we exit
            while( !_exitRequested )
            {
                
                if( _pauseThread )
                {
                    // Pause the thread for 1ms - Not really though, but 0 will make us
                    // return as soon as we can and use a bunch of CPU which we don't want.
                    // Using the value of 1 means it will release the CPU for the real
                    // minimal threshold (typically 15ms on Windows).
                    _threadState = SDLThreadState.Paused;
                    
                    while( _pauseThread )
                        Thread.Sleep( 1 );
                    
                    _threadState = SDLThreadState.Running;
                }
                
                if( _windowSaveRequested )
                    INTERNAL_SDLThread_SaveWindowToFile();
                
                if( _windowResetRequired )
                    _exitRequested = !INTERNAL_SDLThread_ResetWindowAndRenderer();
                
                if( !_exitRequested )
                {
                    loopStartTick = _threadTimer.Elapsed.Ticks;
                    loopDelayTicks += loopTime.Ticks;
                    
                    if( loopDelayTicks >= _baseFrameDelay )
                    {
                        loopDelayTicks -= _baseFrameDelay;
                        drawTickDelta = loopStartTick - lastDrawTick;
                        eventTickDelta = loopStartTick - lastEventTick;
                        
                        if( drawTickDelta >= _drawTicks )
                        {
                            // Time to render the scene
                            lastFPSCount++;
                            drawTickDelta -= _drawTicks;
                            lastDrawTick = loopStartTick;
                            frameStart = _threadTimer.Elapsed.Ticks;
                            INTERNAL_SDLThread_RenderScene();
                            frameEnd = _threadTimer.Elapsed.Ticks;
                            frameTime += ( frameEnd - frameStart );
                        }
                        
                        if( eventTickDelta >= _eventTicks )
                        {
                            // Time to check and handle events
                            eventTickDelta -= _eventTicks;
                            lastEventTick = loopStartTick;
                            INTERNAL_SDLThread_EventDispatcher();
                        }
                    }
                }
                
                if( !_exitRequested )
                {
                    // Sleep until the next expected update
                    loopSleepTime = TimeSpan.FromTicks( _baseFrameDelay - loopDelayTicks );
                    loopSleepMS = (int)loopSleepTime.TotalMilliseconds;
                    if( loopSleepMS >= 0 )
                        Thread.Sleep( loopSleepMS );
                    
                    // End of loop, get time elapsed for this loop
                    loopEndTick = _threadTimer.Elapsed.Ticks;
                    loopTime = TimeSpan.FromTicks( loopEndTick - loopStartTick );
                    
                    // Performance feedback
                    fpsTicks += loopTime.Ticks;
                    if( ( fpsTicks >= TimeSpan.TicksPerSecond )&&( lastFPSCount > 0 ) )
                    {
                        _actualFPS = (int)( ( lastFPSCount * TimeSpan.TicksPerSecond ) / fpsTicks );
                        _potentialFPS = (int)( ( lastFPSCount * TimeSpan.TicksPerSecond ) / frameTime );
                        _averageFrameTime = frameTime / lastFPSCount;
                        lastFPSCount = 0;
                        fpsTicks = 0;
                        frameTime = 0;
                    }
                }
            }
            
            // Mark the thread as no longer running
            _threadTimer.Stop();
            _threadState = SDLThreadState.Stopping;
            _actualFPS = 0;
            _potentialFPS = 0;
            _averageFrameTime = 0;
        }
        
        void INTERNAL_SDLThread_RenderScene()
        {
            //Console.Write( "INTERNAL_SDLThread_RenderScene()\n" );
            
            #if DEBUG
            if( !IsReady ) return;
            #endif
            
            DelFunc_ClearScene();
            
            if( DrawScene != null )
                DrawScene( this );
            
            SDL.SDL_RenderPresent( _sdlRenderer );
        }
        
        #endregion
        
        #region Internal:  SDLThread Save SDL_Window to file (ie: png, etc)
        
        void INTERNAL_SDLThread_SaveWindowToFile()
        {
            _windowResetRequired = true;
            
            var sdlSurface = SDL.SDL_GetWindowSurface( _sdlWindow );
            var mustLock = SDL.SDL_MUSTLOCK( sdlSurface );
            
            _windowSaved = INTERNAL_Save_SDLSurface( sdlSurface, _windowSaveFormat, _windowSaveFilename );
            
            _windowSaveRequested = false;
        }
        
        #endregion
        
        #region Internal:  SDLThread Init/Denit/Reset
        
        void INTERNAL_SDLThread_Cleanup( SDLThreadState newState )
        {
            //Console.Write( "INTERNAL_SDLThread_Cleanup()\n" );
            
            // Dispose of the renderer, window, etc
            INTERNAL_SDLThread_ReleaseWindowAndRenderer();
            
            _sdlThread = null;
            _threadState = newState;
        }
        
        void INTERNAL_SDLThread_ReleaseWindowAndRenderer()
        {
            //Console.Write( "INTERNAL_SDLThread_ReleaseWindowAndRenderer()\n" );
            
            if( _sdlRenderer != IntPtr.Zero )
                SDL.SDL_DestroyRenderer( _sdlRenderer );
            if( _sdlWindow != IntPtr.Zero )
                SDL.SDL_DestroyWindow( _sdlWindow );
            
            _sdlRenderer = IntPtr.Zero;
            _sdlWindow = IntPtr.Zero;
        }
        
        bool INTERNAL_SDLThread_ResetWindowAndRenderer()
        {
            //Console.Write( "INTERNAL_SDLThread_ResetWindowAndRenderer()\n" );
            
            // Get the old state values
            var obm = this.BlendMode;
            var osc = this.ShowCursor;
            
            // Free the existing SDL_Window and SDL_Renderer
            INTERNAL_SDLThread_ReleaseWindowAndRenderer();
            
            // Aquire new a SDL_Window and SDL_Renderer
            var ret = INTERNAL_SDLThread_InitWindowAndRenderer();
            if( ret )
            {
                // Set the old state values
                this.BlendMode = obm;
                this.ShowCursor = osc;
            }
            
            return ret;
        }
        
        bool INTERNAL_SDLThread_InitWindowAndRenderer()
        {
            //Console.Write( "INTERNAL_SDLThread_InitWindowAndRenderer()\n" );
            
            // Create the SDL window
            _sdlWindow = SDL.SDL_CreateWindow(
                _windowTitle,
                SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED,
                _windowSize.Width, _windowSize.Height,
                !_anchored ?
                SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN :
                SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS |
                SDL.SDL_WindowFlags.SDL_WINDOW_SKIP_TASKBAR |
                SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN
            );
            if( _sdlWindow == IntPtr.Zero )
                return false;
                //throw new Exception( string.Format( "Unable to create SDL_Window!\n\n{0}", SDL.SDL_GetError() ) );
            
            // Create the underlying renderer
            _sdlRenderer = SDL.SDL_CreateRenderer(
                _sdlWindow,
                -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC
            );
            if( _sdlRenderer == IntPtr.Zero )
                return false;
                //throw new Exception( string.Format( "Unable to create SDL_Renderer!\n\n{0}", SDL.SDL_GetError() ) );
            
            if( SDL.SDL_GetRendererInfo( _sdlRenderer, out _sdlRenderInfo ) != 0 )
                return false;
                //throw new Exception( string.Format( "Unable to obtain SDL_RendererInfo!\n\n{0}", SDL.SDL_GetError() ) );
            
            _sdlWindow_PixelFormat = SDL.SDL_GetWindowPixelFormat( _sdlWindow );
            if( _sdlWindow_PixelFormat == SDL.SDL_PIXELFORMAT_UNKNOWN )
                return false;
                //throw new Exception( string.Format( "Unable to obtain SDL_Window pixel format!\n\n{0}", SDL.SDL_GetError() ) );
            
            if( SDL.SDL_PixelFormatEnumToMasks(
                _sdlWindow_PixelFormat,
                out _sdlWindow_bpp,
                out _sdlWindow_Rmask,
                out _sdlWindow_Gmask,
                out _sdlWindow_Bmask,
                out _sdlWindow_Amask ) == SDL.SDL_bool.SDL_FALSE )
                return false;
                //throw new Exception( string.Format( "Unable to obtain SDL_Window pixel format bitmasks!\n\n{0}", SDL.SDL_GetError() ) );
            
            // Get the Win32 HWND from the SDL window
            var sysWMinfo = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetWindowWMInfo( _sdlWindow, ref sysWMinfo );
            var sdlWindowHandle = sysWMinfo.info.win.window;
            
            if( _anchored )
            {
                // Time to anchor the window to the control...
                
                // Tell SDL we don't want a border...
                SDL.SDL_SetWindowBordered( _sdlWindow, SDL.SDL_bool.SDL_FALSE );
                
                // ...Aero doesn't always listen to SDL so force it through the Windows API
                var winStyle = (WinAPI.WindowStyleFlags)WinAPI.GetWindowLongPtr( sdlWindowHandle, WinAPI.WindowLongIndex.GWL_STYLE );
                winStyle &= ~WinAPI.WindowStyleFlags.WS_BORDER;
                winStyle &= ~WinAPI.WindowStyleFlags.WS_SIZEBOX;
                winStyle &= ~WinAPI.WindowStyleFlags.WS_DLGFRAME;
                WinAPI.SetWindowLongPtr( sdlWindowHandle, WinAPI.WindowLongIndex.GWL_STYLE, (uint)winStyle );
               
                // Move the SDL window to 0, 0
                WinAPI.SetWindowPos(
                    sdlWindowHandle,
                    _mainFormHandle,
                    0, 0,
                    0, 0,
                    WinAPI.WindowSWPFlags.SWP_NOSIZE | WinAPI.WindowSWPFlags.SWP_SHOWWINDOW
                );
                
                // Anchor the SDL_Window to the control
                WinAPI.SetParent( sdlWindowHandle, _targetControlHandle );
                
            }
            else
            {
                // Make the SDL_Window look like a tool window
                var winStyle = (WinAPI.WindowStyleFlags)WinAPI.GetWindowLongPtr( sdlWindowHandle, WinAPI.WindowLongIndex.GWL_EXSTYLE );
                winStyle |= WinAPI.WindowStyleFlags.WS_EX_TOOLWINDOW;
                WinAPI.SetWindowLongPtr( sdlWindowHandle, WinAPI.WindowLongIndex.GWL_EXSTYLE, (uint)winStyle );
                
            }
            
            // ShowWindow to force all the changes and present the SDL_Window
            WinAPI.ShowWindow( sdlWindowHandle, WinAPI.ShowWindowFlags.SW_SHOWNORMAL );
            
            // SDL_Window and SDL_Renderer are ready for use
            _windowResetRequired = false;
            return true;
        }
        
        #endregion
        
    }
    
}
