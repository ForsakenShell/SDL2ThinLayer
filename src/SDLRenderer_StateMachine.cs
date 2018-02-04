/*
 * SDLRenderer_StateMachine.cs
 *
 * This houses most of the SDLRenderers state machine and resources as well as access to the parts of the
 * state machine that can be changed after the SDLRenderer has been created.
 *
 * User: 1000101
 * Date: 28/01/2018
 * Time: 3:09 AM
 * 
 */
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using System.Drawing;
using System.Windows.Forms;
using SDL2;

using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        #region Internal:  Windows.Forms control objects
        
        bool _anchored;
        Control _parent;
        IntPtr _parentHandle;
        Size _windowSize;
        
        // Anchored references
        Form _mainForm;
        Control _targetControl;
        IntPtr _mainFormHandle;
        IntPtr _targetControlHandle;
        
        // Unanchored references
        string _windowTitle;
        
        #endregion
        
        #region Internal:  SDL control objects
        
        IntPtr _sdlWindow;
        uint _sdlWindow_PixelFormat;
        int _sdlWindow_bpp;
        uint _sdlWindow_Rmask;
        uint _sdlWindow_Gmask;
        uint _sdlWindow_Bmask;
        uint _sdlWindow_Amask;
        
        IntPtr _sdlRenderer;
        SDL.SDL_RendererInfo _sdlRenderInfo;
        
        #endregion
        
        #region Internal:  State machine state
        
        bool _sdlInitialized;
        
        #endregion
        
        #region Internal:  Public API state machine variables
        
        // Render controls
        Color _clearColor;
        bool _showCursor;
        
        // Performance feedback
        int _drawsPS;
        int _eventsPS;
        bool _fastRender;
        
        #endregion
        
        #region Public API:  Direct access to SDL objects
        
        /// <summary>
        /// The SDL_Window associated with this renderer.
        /// </summary>
        public IntPtr Window
        {
            get
            {
                return _sdlWindow;
            }
        }
        
        /// <summary>
        /// The SDL_Renderer associated with this renderer.
        /// </summary>
        public IntPtr Renderer
        {
            get
            {
                return _sdlRenderer;
            }
        }
        
        /// <summary>
        /// The SDL_PIXELFORMAT of the SDL_Window.
        /// </summary>
        public uint PixelFormat
        {
            get
            {
                return _sdlWindow_PixelFormat;
            }
        }
        
        /// <summary>
        /// The number of Bits Per Pixel (bpp) of the SDL_Window.
        /// </summary>
        public int BitsPerPixel
        {
            get
            {
                return _sdlWindow_bpp;
            }
        }
        
        /// <summary>
        /// The width of the SDL_Window.
        /// </summary>
        public int Width
        {
            get
            {
                return _windowSize.Width;
            }
        }
        
        /// <summary>
        /// The Height of the SDL_Window.
        /// </summary>
        public int Height
        {
            get
            {
                return _windowSize.Height;
            }
        }
        
        /// <summary>
        /// The alpha channel mask of the SDL_Window.
        /// </summary>
        public uint AlphaMask
        {
            get
            {
                return _sdlWindow_Amask;
            }
        }
        
        /// <summary>
        /// The red channel mask of the SDL_Window.
        /// </summary>
        public uint RedMask
        {
            get
            {
                return _sdlWindow_Rmask;
            }
        }
        
        /// <summary>
        /// The green channel mask of the SDL_Window.
        /// </summary>
        public uint GreenMask
        {
            get
            {
                return _sdlWindow_Gmask;
            }
        }
        
        /// <summary>
        /// The blue channel mask of the SDL_Window.
        /// </summary>
        public uint BlueMask
        {
            get
            {
                return _sdlWindow_Bmask;
            }
        }
        
        #endregion
        
        #region Public API:  State Machine States
        
        /// <summary>
        /// Is this instance of SDLRenderer ready for use?
        /// </summary>
        public bool IsReady
        {
            get
            {
                return
                    _sdlInitialized &&
                    INTERNAL_SDLThread_Running &&
                    !_exitRequested &&
                    !_windowResetRequired;
            }
        }
        
        /// <summary>
        /// Switch between using faster draw/blit methods without state preservation or slower state preserving draw/blit methods.
        /// </summary>
        public bool PreserveUserState
        {
            get
            {
                return _fastRender;
            }
            set
            {
                _fastRender = value;
                INTERNAL_UpdateState_FunctionPointers();
            }
        }
        
        /// <summary>
        /// Toggle the mouse cursor on/off when over the SDL_Window.
        /// </summary>
        public bool ShowCursor
        {
            get
            {
                return _showCursor;
            }
            set
            {
                // Update the mouse visibility
                _showCursor = value;
                INTERNAL_UpdateState_CursorVisibility();
            }
        }
        
        /// <summary>
        /// Get/Set the renderer clear color.
        /// </summary>
        public Color ClearColor
        {
            get
            {
                return _clearColor;
            }
            set
            {
                _clearColor = value;
            }
        }
        
        /// <summary>
        /// Get/Set the renderer alpha blend mode.
        /// </summary>
        public SDL.SDL_BlendMode BlendMode
        {
            get
            {
                SDL.SDL_BlendMode mode;
                return SDL.SDL_GetRenderDrawBlendMode( _sdlRenderer, out mode ) != 0 ? SDL.SDL_BlendMode.SDL_BLENDMODE_INVALID : mode;
            }
            set
            {
                SDL.SDL_SetRenderDrawBlendMode( _sdlRenderer, value );
            }
        }
        
        #endregion
        
        #region Internal:  Renderer initialization
        
        // The actual constructor
        void INTERNAL_Init_Main(
            Form mainForm,
            Control targetControl,
            int windowWidth,
            int windowHeight,
            string windowTitle,
            Client_Delegate_WindowClosed windowClosed,
            int drawsPerSecond,
            int eventsPerSecond,
            bool fastRender,
            bool showCursorOverControl )
        {
            // Will this be an anchored window?
            _anchored = ( targetControl != null );
            
            // mainForm must be set regardless
            if( mainForm == null )
                throw new ArgumentException( "mainForm/parentForm cannot be null!" );
            
            // windowClosed must be set for an unanchored window
            if( ( !_anchored )&&( windowClosed == null ) )
                throw new ArgumentException( "windowClosed cannot be null!" );
            
            // Assign the control objects the SDL_Window and SDL_Renderer will attach to
            _targetControl = targetControl;
            _parent = _anchored ? targetControl : mainForm;
            _parentHandle = _parent.Handle;
            _mainForm = mainForm;
            _mainFormHandle = _mainForm.Handle;
            _targetControlHandle = _anchored ? _targetControl.Handle : IntPtr.Zero;
            _windowSize = _anchored ? _targetControl.Size : new Size( windowWidth, windowHeight );
            _windowTitle = windowTitle;
            WindowClosed = windowClosed;
            
            // Clear SDLThread controls
            _threadState = SDLThreadState.Inactive;
            _exitRequested = false;
            _pauseThread = false;
            _windowResetRequired = false;
            _windowSaveRequested = false;
            
            // Clear SDLThread Performance Feedback variables
            _actualFPS = 0;
            _potentialFPS = 0;
            _averageFrameTime = 0;
            
            // Set initial API state
            _clearColor = Color.FromArgb( 0 );
            _showCursor = showCursorOverControl;
            _fastRender = fastRender;
            _drawsPS = drawsPerSecond;
            _eventsPS = eventsPerSecond;
            
            // Since we are not a procedural language, we'll tell SDL to stfu.
            SDL.SDL_SetMainReady();
            
            // Initialize SDL
            _sdlInitialized = INTERNAL_Init_SDLSystems(
                SDL.SDL_INIT_TIMER |
                SDL.SDL_INIT_VIDEO
            );
            if( !_sdlInitialized )
                throw new Exception( string.Format( "Unable to initialize SDL!\n\n{0}", SDL.SDL_GetError() ) );
            
            // Now start the "SDLThread" to handle this renderer
            var threadInitOk = INTERNAL_Init_SDLThread();
            if( !threadInitOk )
                throw new Exception( string.Format( "Error in thread startup!\n\n{0}", SDL.SDL_GetError() ) );
            
        }
        
        bool INTERNAL_Init_SDLSystems( uint subsysFlags )
        {
            #if DEBUG
            
            // Need to add the no parachute flag for debuggers so SDL will interact with them nicely
            // According to the wiki (http://wiki.libsdl.org/), this does nothing in SDL2 however,
            // we'll do it anyway "just in case".
            subsysFlags |= SDL.SDL_INIT_NOPARACHUTE;
            
            if( System.Diagnostics.Debugger.IsAttached )
            {
                // ¡Windows es muy estúpido!
                SDL.SDL_SetHint(
                    SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING,
                    "1"
                );
            }
            
            #endif
            
            return SDL.SDL_Init( subsysFlags ) == 0;
        }
        
        bool INTERNAL_Init_SDLThread()
        {
            // Create a thread for the object
            _sdlThread = new Thread( INTERNAL_SDLThread_Main );
            if( _sdlThread == null )
                return false;
                //throw new Exception( "Unable to create thread!" );
            
            // Start the thread for the object
            _sdlThread.Start();
            
            // Wait for the thread to finish creating the state machine and start looping
            while( ( _threadState == SDLThreadState.Inactive )||( _threadState == SDLThreadState.Starting ) )
                Thread.Sleep( 0 );
            
            // The thread is now running and ready for user code or not running with an error set
            return INTERNAL_SDLThread_Running;
        }
        
        #endregion
        
        #region Internal:  State Machine states
        
        void INTERNAL_UpdateState_FunctionPointers()
        {
            if( _fastRender )
            {
                DelFunc_ClearScene          = INTERNAL_DelFunc_ClearScene_Fast;
                DelFunc_DrawPoint           = INTERNAL_DelFunc_DrawPoint_Fast;
                DelFunc_DrawPoints          = INTERNAL_DelFunc_DrawPoints_Fast;
                DelFunc_DrawLine            = INTERNAL_DelFunc_DrawLine_Fast;
                DelFunc_DrawLines           = INTERNAL_DelFunc_DrawLines_Fast;
                DelFunc_DrawRect            = INTERNAL_DelFunc_DrawRect_Fast;
                DelFunc_DrawRects           = INTERNAL_DelFunc_DrawRects_Fast;
                DelFunc_DrawFilledRect      = INTERNAL_DelFunc_DrawFilledRect_Fast;
                DelFunc_DrawFilledRects     = INTERNAL_DelFunc_DrawFilledRects_Fast;
                DelFunc_DrawCircle          = INTERNAL_DelFunc_DrawCircle_Fast;
                DelFunc_DrawFilledCircle    = INTERNAL_DelFunc_DrawFilledCircle_Fast;
            }
            else
            {
                DelFunc_ClearScene          = INTERNAL_DelFunc_ClearScene;
                DelFunc_DrawPoint           = INTERNAL_DelFunc_DrawPoint;
                DelFunc_DrawPoints          = INTERNAL_DelFunc_DrawPoints;
                DelFunc_DrawLine            = INTERNAL_DelFunc_DrawLine;
                DelFunc_DrawLines           = INTERNAL_DelFunc_DrawLines;
                DelFunc_DrawRect            = INTERNAL_DelFunc_DrawRect;
                DelFunc_DrawRects           = INTERNAL_DelFunc_DrawRects;
                DelFunc_DrawFilledRect      = INTERNAL_DelFunc_DrawFilledRect;
                DelFunc_DrawFilledRects     = INTERNAL_DelFunc_DrawFilledRects;
                DelFunc_DrawCircle          = INTERNAL_DelFunc_DrawCircle;
                DelFunc_DrawFilledCircle    = INTERNAL_DelFunc_DrawFilledCircle;
            }
        }
        
        void INTERNAL_UpdateState_CursorVisibility()
        {
            SDL.SDL_ShowCursor( (int)( _showCursor ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE ) );
        }
        
        void INTERNAL_UpdateState_ThreadIntervals()
        {
            _drawTicks = (long)( (double)TimeSpan.TicksPerSecond / _drawsPS );
            _eventTicks = (long)( (double)TimeSpan.TicksPerSecond / _eventsPS );
            _baseFrameDelay = Math.Min( _drawTicks, _eventTicks );
            //Console.Write( string.Format(
            //    "INTERNAL_UpdateState_ThreadIntervals()\n\t_drawTicks={0}\n\t_eventTicks={1}\n\t_baseFrameDelay={2}\n",
            //    _drawTicks,
            //    _eventTicks,
            //    _baseFrameDelay
            //    ) );
        }
        
        Color INTERNAL_RenderColor
        {
            get
            {
                #if DEBUG
                if( !IsReady ) return Color.Black;
                #endif
                
                byte r, g, b, a;
                SDL.SDL_GetRenderDrawColor( _sdlRenderer, out r, out g, out b, out a );
                return Color.FromArgb( a, r, g, b );
            }
            set
            {
                #if DEBUG
                if( !IsReady ) return;
                #endif
                
                SDL.SDL_SetRenderDrawColor( _sdlRenderer, value.R, value.G, value.B, value.A );
            }
        }
        
        #endregion
        
    }
}
