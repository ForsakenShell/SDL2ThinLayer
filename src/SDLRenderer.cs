/*
 * SDLRenderer.cs
 *
 * This is the constructor, destructor and actual IDisposable interface.
 *
 * User: 1000101
 * Date: 19/12/2017
 * Time: 9:16 AM
 * 
 */

using System;
using System.Threading;

using System.Drawing;
using System.Windows.Forms;
using SDL2;

using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace SDL2ThinLayer
{
    
    public partial class SDLRenderer : IDisposable
    {
        
        #region Control constants
        
        public const int MAX_UPDATES_PER_SECOND = 240; // 240 FPS, do we need faster than that???
        public const int DEFAULT_DRAWS_PER_SECOND = 60;
        public const int DEFAULT_EVENTS_PER_SECOND = 120;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Creates a new anchored SDL_Window and SDL_Renderer.
        /// </summary>
        /// <param name="mainForm">The Form the target Control is in.</param>
        /// <param name="targetControl">Control to anchor the SDL_Window too.</param>
        /// <param name="drawsPerSecond">Number of times per second the scene should be rendered.</param>
        /// <param name="eventsPerSecond">Number of times per second the events should be checked, not the number of events to be processed.</param>
        /// <param name="fastRender">If false, will make more extensive sanity checks.</param>
        /// <param name="showCursorOverControl"> Show the mouse cursor when over the target Control?</param>
        public SDLRenderer(
            Form mainForm,
            Control targetControl,
            int drawsPerSecond = DEFAULT_DRAWS_PER_SECOND,
            int eventsPerSecond = DEFAULT_EVENTS_PER_SECOND,
            bool fastRender = true,
            bool showCursorOverControl = true
        ) : base()
        {
            INTERNAL_Init_Main( mainForm, targetControl, 0, 0, string.Empty, null, drawsPerSecond, eventsPerSecond, fastRender, showCursorOverControl );
        }
        
        /// <summary>
        /// Creates a new SDL_Window and SDL_Renderer.
        /// 
        /// NOTE:  windowClosed will be called asyncronously in parentForm's thread.
        /// </summary>
        /// <param name="parentForm">The parent Form of the SDL_Window.</param>
        /// <param name="windowWidth">Width of the window to create.</param>
        /// <param name="windowHeight">Height of the window to create.</param>
        /// <param name="windowTitle">Title to give the SDL_Window.</param>
        /// <param name="windowClosed">Delegate for the SDL_Window closing by the user clicking the close button.</param>
        /// <param name="drawsPerSecond">Number of times per second the scene should be rendered.</param>
        /// <param name="eventsPerSecond">Number of times per second the events should be checked, not the number of events to be processed.</param>
        /// <param name="fastRender">If false, will make more extensive sanity checks.</param>
        /// <param name="showCursorOverWindow"> Show the mouse cursor when over the SDL_Window?</param>
        public SDLRenderer(
            Form parentForm,
            int windowWidth,
            int windowHeight,
            string windowTitle,
            Client_Delegate_WindowClosed windowClosed,
            int drawsPerSecond = DEFAULT_DRAWS_PER_SECOND,
            int eventsPerSecond = DEFAULT_EVENTS_PER_SECOND,
            bool fastRender = true,
            bool showCursorOverWindow = true
        ) : base()
        {
            INTERNAL_Init_Main( parentForm, null, windowWidth, windowHeight, windowTitle, windowClosed, drawsPerSecond, eventsPerSecond, fastRender, showCursorOverWindow );
        }
        
        #endregion
        
        #region Destructor & IDispose
        
        ~SDLRenderer()
        {
            this.Dispose();
        }
        
        // Protect against "double-free" errors caused by combinations of explicit disposal[s] and GC disposal
        bool _disposed = false;
        
        public void Dispose()
        {
            if( _disposed ) return;
            _disposed = true;
            
            // Signal the event thread to stop
            _exitRequested = true;
            
            // Disable all scenes
            DrawScene = null;
            
            // And wait for it to stop
            while( INTERNAL_SDLThread_Active )
                Thread.Sleep( 0 );
            
            // Shutdown SDL itself
            if( _sdlInitialized )
                SDL.SDL_Quit();
            
            // No longer a valid SDL state
            _sdlInitialized = false;
        }
        
        #endregion
        
    }
}
