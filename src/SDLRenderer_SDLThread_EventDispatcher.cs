/*
 * SDLRenderer_SDLThread_EventDispatcher.cs
 *
 * The event dispatcher for the SDLThread has been separated from the rest of the threading code
 * just to keep the file from getting too bulky.  Like that section, don't mess with this unless
 * you know what's what with threads.
 * 
 * These delegates and functions handle callback delegates for the SDL_Window event queue.  Remember
 * that client callbacks will be run in the SDLThread and not the main thread.  All resources should
 * be guarded as needed for a multi-threaded environment.
 * 
 * User: 1000101
 * Date: 28/01/2018
 * Time: 3:24 AM
 * 
 */
using System;

using SDL2;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        #region Client Delegate Prototypes
        
        public delegate void Client_Delegate_DrawScene();
        public delegate void Client_Delegate_SDL_Event( SDL.SDL_Event e );
        public delegate void Client_Delegate_WindowClosed( SDLRenderer renderer );
        
        #endregion
        
        #region Client Callbacks
        
        // Actually used in INTERNAL_SDLThread_RenderScene() (see SDLRenderer_SDLThread.cs)
        public Client_Delegate_DrawScene DrawScene;
        
        // SDL_Events the client can handle, these will be called in the SDLRenderer
        // thread and the client should handle it's own mechanisms for data protection.
        public Client_Delegate_SDL_Event KeyDown;
        public Client_Delegate_SDL_Event KeyUp;
        public Client_Delegate_SDL_Event MouseButtonDown;
        public Client_Delegate_SDL_Event MouseButtonUp;
        public Client_Delegate_SDL_Event MouseMove;
        public Client_Delegate_SDL_Event MouseWheel;
        
        // For a stand-alone SDL_Window, we need an event handler for it to signal back that the user closed it.
        // Client code cannot explicitly [un]subscribe to this, the handler must be passed to the constructor.
        Client_Delegate_WindowClosed WindowClosed;
        
        #endregion
        
        #region SDL control objects
        
        SDL.SDL_Event _sdlEvent;
        
        #endregion
        
        void INTERNAL_SDLThread_EventDispatcher()  
        {
            //Console.Write( "SDLThread_HandleEvents()\n" );
            
            #if DEBUG
            if( !IsReady ) return;
            #endif
            
            // TODO:  Add other relevant events!
            
            while( SDL.SDL_PollEvent( out _sdlEvent ) != 0 )
            {
                switch( _sdlEvent.type )
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                    {
                        // Nothing else matter after a quit event, just return
                        _exitRequested = true;
                        return;
                    }
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        // Call user KeyDown handler
                        if( KeyDown != null )
                            KeyDown( _sdlEvent );
                        break;
                    }
                    case SDL.SDL_EventType.SDL_KEYUP:
                    {
                        // Call user KeyUp handler
                        if( KeyUp != null )
                            KeyUp( _sdlEvent );
                        break;
                    }
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    {
                        // Call user MouseButtonDown handler
                        if( MouseButtonDown != null )
                            MouseButtonDown( _sdlEvent );
                        break;
                    }
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    {
                        // Call user MouseButtonUp handler
                        if( MouseButtonUp != null )
                            MouseButtonUp( _sdlEvent );
                        break;
                    }
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    {
                        // Call user MouseMove handler
                        if( MouseMove != null )
                            MouseMove( _sdlEvent );
                        break;
                    }
                    case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    {
                        // Call user MouseWheel handler
                        if( MouseWheel != null )
                            MouseWheel( _sdlEvent );
                        break;
                    }
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    {
                        switch( _sdlEvent.window.windowEvent )
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                            {
                                // Signal this thread to terminate the main loop
                                _exitRequested = true;
                                
                                // Asyncronously signal the main thread that this thread is terminating
                                if( WindowClosed != null )
                                {
                                    var p = new object[ 1 ];
                                    p[ 0 ] = this;
                                    _mainForm.BeginInvoke( WindowClosed, p );
                                }
                                
                                // Nothing else matter after a window close event, just return
                                return;
                            }
                        }
                        break;
                    }
                }
            }
            
        }
        
    }
}
