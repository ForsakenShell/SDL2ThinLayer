/*
 * TestBench.cs
 *
 * This is a basic example of how to use SDL2ThinLayer as well as serving as basic unit testing.
 *
 * SDL2ThinLayer was adapted from FNA by Ethan "flibitijibibo" Lee.
 *
 * SDL2ThinLayer is released under the unlicense.
 * http://unlicense.org/
 *
 * User: 1000101
 * Date: 28/01/2018
 * Time: 4:40 AM
 * 
 */

#region FNA License
/* FNA GameWindow for System.Windows.Forms Example
 *
 * Written by Ethan "flibitijibibo" Lee
 * http://www.flibitijibibo.com/
 *
 * Released under public domain.
 * No warranty implied; use at your own risk.
 */
#endregion

#region Using Statements

using System;
using System.Collections;
using System.Collections.Generic;

/* Bad awful Win32 Stuff */
using System.Drawing;
using System.Windows.Forms;
using Point = System.Drawing.Point;

/* Good nice SDL2 Stuff */
using SDL2ThinLayer;
using SDL2;

#endregion

public class SDLRendererExampleForm : Form
{
    
    #region The stuff that explains how to use SDLRenderer
    
    #region Example control constants
    
    // SDL_Window Size
    public const int SDL_WINDOW_WIDTH = 640;
    public const int SDL_WINDOW_HEIGHT = 480;
    
    // Number of whatevers to whatever each callback
    const int ITTERATIONS = 100;
    
    // How often should the profiling information be dumped to console?
    const int PROFILE_FREQUENCY_MS = 1000;
    
    #endregion
    
    #region SDLRenderer variables, You'll need these
    
    // These are the variables you care about.
    
    // SDL2ThinLayer.SDLRenderer
    //
    // Threaded renderer and specific event queue
    SDLRenderer sdlRenderer;
    
    // System.Windows.Forms.Control (well, Panel in this case)
    //
    // Target control to set the render target to, could be any control but the one with
    // the least complexity and most flexibility is probably best (hence Panel).
    Panel gamePanel;
    
    // Assets:
    
    // NOTE:  The following does not need to be static other than the nature of this example code
    
    // Surfaces are a deprecated technology and there are performance costs when using them with SDLRenderer
    static SDLRenderer.Surface surface;
    
    // Textures are hardware based and are much faster.
    static SDLRenderer.Texture texture;
    
    // Fonts are not very optimal, don't draw too much text.
    static SDLRenderer.Font font;
    
    #endregion
    
    #region Init/Denit SDLRenderer as well as create an example Surface and Texture
    
    void SetupRenderer()
    {
        // Example form changes, ignore the next few lines
        CalculateWindowSize();
        buttonInit.Text = "Denit";
        
        // Get anchoring from example form checkbox
        var anchor = checkAnchored.Checked;
        
        // Create the renderer
        if( anchor )
            // Create the SDLRenderer as an anchored window to the gamePanel.
            sdlRenderer = new SDLRenderer( this, gamePanel );
        else
            // Create the SDLRenderer as a stand-alone SDL_Window with the WS_EX_TOOLWINDOW extended style.
            sdlRenderer = new SDLRenderer( this,
                                          SDL_WINDOW_WIDTH, SDL_WINDOW_HEIGHT,
                                          "SDL_Window as a tool window in it's own thread!",
                                          SDLWindowClosed );
        
        // Add some event callbacks, this example just reports the event ID to console
        sdlRenderer.KeyDown += EventReporter;
        sdlRenderer.KeyUp += EventReporter;
        sdlRenderer.MouseButtonDown += EventReporter;
        sdlRenderer.MouseButtonUp += EventReporter;
        sdlRenderer.MouseMove += EventReporter;
        sdlRenderer.MouseWheel += EventReporter;
        
        // We have assets, we need to handle changes to the SDL_Window and SDL_Renderer
        sdlRenderer.RendererReset += SDLRendererReset;
        
        // Setting certain render states must be done from the SDL thread.
        // To execute something in the SDL thread, use SDLRenderer.Invoke()
        // or SDLRenderer.BeginInvoke() as appropriate.
        //
        // NOTE:  SDLRenderer.[Begin]Invoke() is not performing a standard
        // Invoke().  What is happening is a user event is being pushed onto
        // the SDL_Event queue to invoke the delegate.  This will cause the
        // actual execution of the method to be delayed.  As a result Invoke()
        // will block the calling thread until the event is handled.  If you
        // want a non-blocking async invocation use SDLRenderer.BeginInvoke()
        // instead.
        sdlRenderer.Invoke( InitInThread );
        
        // Start the performance feedback timer (more example form stuff)
        timer.Start();
    }
    
    void InitInThread( SDLRenderer renderer )
    {
        // Set the render blender mode
        renderer.BlendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
        
        // Load some assets
        CreateAssetsForRenderer( renderer );
        
        // Tell the examples the renderer to use
        SDLExampleSet.UpdateRenderer( sdlRenderer );
        
        // Barf to the console
        ConsoleDump( renderer );
        
    }
    
    void ShutdownRenderer()
    {
        // (I thought this was about SDLRenderer, not the example form!)
        // Example Form change state, ignore the next couple lines and the comment above
        CalculateWindowSize( true );
        buttonInit.Text = "Init";
        timer.Stop();
        
        // Tell the examples the renderer is invalid
        SDLExampleSet.UpdateRenderer( null );
        
        // Tell SDLRenderer to stop it's thread.  We do this so we don't destroy resources
        // being used before destroying the renderer itself.
        if( sdlRenderer != null )
            sdlRenderer.DestroyWindow();
        
        // While SDL2ThingLayer implements IDisposable in all it's classes and
        // explicitly disposes of their resources in their destructors, I always
        // like to clean up after myself (old habits).
        ReleaseAssets();
        
        // Dispose of the Renderer
        if( sdlRenderer != null )
            sdlRenderer.Dispose();
        
        // This is all you really need to do though, GC will handle the rest
        surface = null;
        texture = null;
        font = null;
        sdlRenderer = null;
        
    }
    
    #endregion
    
    #region Assets
    
    void CreateAssetsForRenderer( SDLRenderer renderer )
    {
        // Load Surface from a file
        //
        // NOTE:  Surfaces are deprecated and require conversion to Textures before blitting.
        surface = renderer.LoadSurface( "pointsprite.png" );
        
        // Set the blend mode for surface blitting
        surface.BlendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
        
        // Create a Texture from the Surface.
        //
        // No need to set the blend mode, etc - all rendering information is copied
        // directly in SDLRenderer.CreateTextureFromSurface() from the Surface settings.
        texture = renderer.CreateTextureFromSurface( surface );
        
        // Load a font from file.
        font = renderer.CreateFont( 12, "LibertySans.ttf" );
        if( font == null )
            throw new Exception( string.Format( "Unable to create font!\n\n{0}", SDL.SDL_GetError() ) );
        
    }
    
    void ReleaseAssets()
    {
        // Dispose of the Surface
        if( surface != null )
            sdlRenderer.DestroySurface( ref surface );
        
        // Dispose of the Texture
        if( texture != null )
            sdlRenderer.DestroyTexture( ref texture );
        
        // Dispose of the Font
        if( Font != null )
            sdlRenderer.DestroyFont( ref font );
        
        surface = null;
        texture = null;
        font = null;
    }
    
    #endregion
    
    #region Console Dump
    
    void ConsoleDump( SDLRenderer renderer )
    {
        // Dump some renderer info to the console
        Console.WriteLine(
            string.Format(
                "SDLRenderer:\n\tResolution = {0}x{1} {2}bpp\n\tPixelFormat = 0x{3}\n\tAlpha Mask = 0x{4}\n\tRed Mask   = 0x{5}\n\tGreen Mask = 0x{6}\n\tBlue Mask  = 0x{7}",
                renderer.WindowSize.Width,
                renderer.WindowSize.Height,
                renderer.BitsPerPixel,
                renderer.PixelFormat.ToString( "X" ),
                renderer.AlphaMask.ToString( "X" ),
                renderer.RedMask  .ToString( "X" ),
                renderer.GreenMask.ToString( "X" ),
                renderer.BlueMask .ToString( "X" )
               ) );
        
        // Dump some surface info to the console
        Console.WriteLine(
            string.Format(
                "Surface:\n\tResolution = {0}x{1} {2}bpp\n\tPixelFormat = 0x{3}\n\tAlpha Mask = 0x{4}\n\tRed Mask   = 0x{5}\n\tGreen Mask = 0x{6}\n\tBlue Mask  = 0x{7}",
                surface.Width,
                surface.Height,
                surface.BitsPerPixel,
                surface.PixelFormat.ToString( "X" ),
                surface.AlphaMask.ToString( "X" ),
                surface.RedMask  .ToString( "X" ),
                surface.GreenMask.ToString( "X" ),
                surface.BlueMask .ToString( "X" )
               ) );
        
        // Dump some texture info to the console
        Console.WriteLine(
            string.Format(
                "Texture:\n\tResolution = {0}x{1} {2}bpp\n\tPixelFormat = 0x{3}\n\tAlpha Mask = 0x{4}\n\tRed Mask   = 0x{5}\n\tGreen Mask = 0x{6}\n\tBlue Mask  = 0x{7}",
                texture.Width,
                texture.Height,
                texture.BitsPerPixel,
                texture.PixelFormat.ToString( "X" ),
                texture.AlphaMask.ToString( "X" ),
                texture.RedMask  .ToString( "X" ),
                texture.GreenMask.ToString( "X" ),
                texture.BlueMask .ToString( "X" )
               ) );
        
        // Dump some texture info to the console
        var fontMetrics = font.Metrics;
        Console.WriteLine(
            string.Format(
                "Font:\n\tHeight = {0}\n\tAscent = {1}\n\tDescent = {2}\n\tLineSkip = {3}",
                fontMetrics.Height,
                fontMetrics.Ascent,
                fontMetrics.Descent,
                fontMetrics.LineSkip
               ) );
        
    }
    
    #endregion
    
    #region Example SDLRenderer Events
    
    #region Scene Renderers
    
    // NOTE:  These callbacks will be run in the SDLRenderer thread.
    //
    // Access to global resources should use the appropriate safe-guards for a
    // multi-threaded envirionment.
    
    #region Example:  Draw Points
    
    /// <summary>
    /// Fills the SDL_Window with a single color using individual SDLRender.DrawPoint calls
    /// </summary>
    public class exDrawPoints : SDLExampleSceneRender
    {
        public exDrawPoints( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawPoints.DrawScene : Event from SDLRenderer.DrawScene" );
            
            var c = Color.FromArgb(
                random.Next( 256 ),
                random.Next( 256 ),
                random.Next( 256 ),
                random.Next( 256 )
            );
            for( int y = 0; y < SDL_WINDOW_HEIGHT; y++ )
            {
                for( int x = 0; x < SDL_WINDOW_WIDTH; x++ )
                {
                    renderer.DrawPoint( x, y, c );
                }
            }
        }
    }
    
    #endregion
    
    #region Example:  Draw Points 2
    
    /// <summary>
    /// Fills the SDL_Window with a single color by building an array of SDL_Points mapped to the
    /// screen and a single call to SDLRender.DrawPoints
    /// </summary>
    public class exDrawPoints2 : SDLExampleSceneRender
    {
        const int TOTAL_PIX = SDL_WINDOW_HEIGHT * SDL_WINDOW_WIDTH;
        SDL.SDL_Point[] _screenMap = new SDL.SDL_Point[ TOTAL_PIX ];
        
        public exDrawPoints2( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut )
        {
            CreateScreenMap();
        }
        
        void CreateScreenMap()
        {
            int index = 0;
            for( int y = 0; y < SDL_WINDOW_HEIGHT; y++ )
            {
                for( int x = 0; x < SDL_WINDOW_WIDTH; x++ )
                {
                    _screenMap[ index ].x = x;
                    _screenMap[ index ].y = y;
                    index++;
                }
            }
        }
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawPoints2.DrawScene : Event from SDLRenderer.DrawScene" );
            
            var c = Color.FromArgb(
                random.Next( 256 ),
                random.Next( 256 ),
                random.Next( 256 ),
                random.Next( 256 )
            );
            renderer.DrawPoints( _screenMap, TOTAL_PIX, c );
        }
    }
    
    #endregion
    
    #region Example:  Draw Lines
    
    /// <summary>
    /// Draws ITTERATIONS random lines using SDLRenderer.DrawLine
    /// </summary>
    public class exDrawLines : SDLExampleSceneRender
    {
        public exDrawLines( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawLines.DrawScene : Event from SDLRenderer.DrawScene" );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                var x1 = random.Next( SDL_WINDOW_WIDTH );
                var y1 = random.Next( SDL_WINDOW_HEIGHT );
                var x2 = random.Next( SDL_WINDOW_WIDTH );
                var y2 = random.Next( SDL_WINDOW_HEIGHT );
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                renderer.DrawLine( x1, y1, x2, y2, c );
            }
        }
    }
    
    #endregion
    
    #region Example:  Draw Rects
    
    /// <summary>
    /// Draws ITTERATIONS random rectangles using SDLRenderer.DrawRect
    /// </summary>
    public class exDrawRects : SDLExampleSceneRender
    {
        public exDrawRects( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawRects.DrawScene : Event from SDLRenderer.DrawScene" );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                var x1 = random.Next( SDL_WINDOW_WIDTH );
                var y1 = random.Next( SDL_WINDOW_HEIGHT );
                var x2 = random.Next( SDL_WINDOW_WIDTH );
                var y2 = random.Next( SDL_WINDOW_HEIGHT );
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                renderer.DrawRect( x1, y1, x2, y2, c );
            }
        }
    }
    
    #endregion
    
    #region Example:  Draw Filled Rects
    
    /// <summary>
    /// Draws ITTERATIONS random filled rectangles using SDLRenderer.DrawFilledRect
    /// </summary>
    public class exDrawFilledRects : SDLExampleSceneRender
    {
        public exDrawFilledRects( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawFilledRects.DrawScene : Event from SDLRenderer.DrawScene" );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                var x1 = random.Next( SDL_WINDOW_WIDTH );
                var y1 = random.Next( SDL_WINDOW_HEIGHT );
                var x2 = random.Next( SDL_WINDOW_WIDTH );
                var y2 = random.Next( SDL_WINDOW_HEIGHT );
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                renderer.DrawFilledRect( x1, y1, x2, y2, c );
            }
        }
    }
    
    #endregion
    
    #region Example:  Draw Circles
    
    /// <summary>
    /// Draws ITTERATIONS random circles using SDLRenderer.DrawCircle
    /// </summary>
    public class exDrawCircles : SDLExampleSceneRender
    {
        public exDrawCircles( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawCircles.DrawScene : Event from SDLRenderer.DrawScene" );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                var x = random.Next( SDL_WINDOW_WIDTH );
                var y = random.Next( SDL_WINDOW_HEIGHT );
                var r = random.Next( SDL_WINDOW_WIDTH / 10 );
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                renderer.DrawCircle( x, y, r, c );
            }
        }
    }
    
    #endregion
    
    #region Example:  Draw Filled Circles
    
    /// <summary>
    /// Draws ITTERATIONS random filled circles using SDLRenderer.DrawFilledCircle
    /// </summary>
    public class exDrawFilledCircles : SDLExampleSceneRender
    {
        public exDrawFilledCircles( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawFilledCircles.DrawScene : Event from SDLRenderer.DrawScene" );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                var x = random.Next( SDL_WINDOW_WIDTH );
                var y = random.Next( SDL_WINDOW_HEIGHT );
                var r = random.Next( SDL_WINDOW_WIDTH / 10 );
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                renderer.DrawFilledCircle( x, y, r, c );
            }
        }
    }
    
    #endregion
    
    #region Example:  Blit Surfaces
    
    /// <summary>
    /// Blits ITTERATIONS random sprites using SDLRenderer.Blit (surface)
    /// </summary>
    public class exBlitSurfaces : SDLExampleSceneRender
    {
        public exBlitSurfaces( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exBlitSurfaces.DrawScene : Event from SDLRenderer.DrawScene" );
            
            var sW = surface.Width;
            var sH = surface.Height;
            var rect = new SDL.SDL_Rect( 0, 0, sW, sH );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                // Adjust color modulation
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                surface.ColorMod = c;
                
                // Blit the Surface now
                rect.x = random.Next( SDL_WINDOW_WIDTH  - sW );
                rect.y = random.Next( SDL_WINDOW_HEIGHT - sH );
                renderer.Blit( rect, surface );
            }
        }
    }
    
    #endregion
    
    #region Example:  Blit Textures
    
    /// <summary>
    /// Blits ITTERATIONS random sprites using SDLRenderer.Blit (texture)
    /// </summary>
    public class exBlitTextures : SDLExampleSceneRender
    {
        public exBlitTextures( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exBlitTextures.DrawScene : Event from SDLRenderer.DrawScene" );
            
            var tW = texture.Width;
            var tH = texture.Height;
            var rect = new SDL.SDL_Rect( 0, 0, tW, tH );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                // Adjust color modulation
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                texture.ColorMod = c;
                
                // Blit the Texture now
                rect.x = random.Next( SDL_WINDOW_WIDTH  - tW );
                rect.y = random.Next( SDL_WINDOW_HEIGHT - tH );
                renderer.Blit( rect, texture );
            }
        }
    }
    
    #endregion
    
    #region Example:  Draw Text
    
    /// <summary>
    /// Draws ITTERATIONS random text using SDLRenderer.DrawText
    /// </summary>
    public class exDrawText : SDLExampleSceneRender
    {
        const string TEXT = "Narf!";
        
        public exDrawText( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawText.DrawScene : Event from SDLRenderer.DrawScene" );
            
            var size = font.TextSize( TEXT );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                var p = new SDL.SDL_Point(
                    random.Next( SDL_WINDOW_WIDTH  - size.Width ),
                    random.Next( SDL_WINDOW_HEIGHT - size.Height )
                );
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                renderer.DrawText( p, font, TEXT, c );
            }
        }
    }
    
    #endregion
    
    #region Example:  Draw Text 2
    
    /// <summary>
    /// Draws ITTERATIONS random text using SDLRenderer.Blit (texture)
    /// </summary>
    public class exDrawText2 : SDLExampleSceneRender
    {
        const string TEXT = "Are you pondering what I'm pondering?";
        
        SDLRenderer.Texture textTure;
        Size textSize;
        
        public exDrawText2( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exDrawText2.DrawScene : Event from SDLRenderer.DrawScene" );
            
            for( int i = 0; i < ITTERATIONS; i++ )
            {
                var rect = new SDL.SDL_Rect(
                    random.Next( SDL_WINDOW_WIDTH  - textSize.Width ),
                    random.Next( SDL_WINDOW_HEIGHT - textSize.Height ),
                    textSize.Width,
                    textSize.Height
                );
                var c = Color.FromArgb(
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 ),
                    random.Next( 256 )
                );
                textTure.ColorMod = c;
                renderer.Blit( rect, textTure );
            }
        }
        
        public override void RendererInit( SDLRenderer render )
        {
            CreateAssetsFor( render );
        }
        
        public override void RendererDenit( SDLRenderer render )
        {
            ReleaseAssets();
        }
        
        public override void RendererReset( SDLRenderer render )
        {
            ReleaseAssets();
            CreateAssetsFor( render );
        }
        
        void CreateAssetsFor( SDLRenderer renderer )
        {
            // Get the size of the message
            textSize = font.TextSize( TEXT );
            
            // Create a Surface for the message
            var surface = font.TextBlended( TEXT, Color.White );
            
            // Turn the surface into a Texture
            textTure = renderer.CreateTextureFromSurface( surface );
        }
        
        void ReleaseAssets()
        {
            if( textTure != null )
                textTure.Dispose();
            textTure = null;
        }
    }
    
    #endregion
    
    #region Example:  Sample 1
    
    /// <summary>
    /// Draws a simple scene composed of static primitives, text and, dynamic sprites
    /// </summary>
    public class exSample1 : SDLExampleSceneRender
    {
        
        const double DEG_TO_RAD = Math.PI / 180.0d;
        
        double spinAngle = 0;
        const double spinStep = 5;
        const double spinDistance = 32;
        const int spriteSize = 32;
        SDL.SDL_Rect spriteRect = new SDL.SDL_Rect( -spriteSize / 2, -spriteSize / 2, spriteSize, spriteSize );
        SDL.SDL_Point spinCentre = new SDL.SDL_Point( SDL_WINDOW_WIDTH / 2, SDL_WINDOW_HEIGHT / 2 );
        Color surfaceColor = Color.Peru;
        Color textureColor = Color.Aquamarine;
            
        public exSample1( Form form, string optIn, string optOut = null ) : base( form, optIn, optOut ) {}
        
        public override void DrawScene( SDLRenderer renderer )
        {
            // You don't really want to uncomment the next line...
            // Console.WriteLine( "exSample1.DrawScene : Event from SDLRenderer.DrawScene" );
            
            if( true )
            {
                // Draw a couple solid lines over the window
                var p1 = new SDL.SDL_Point( SDL_WINDOW_WIDTH / 2, 0 );
                var p2 = new SDL.SDL_Point( SDL_WINDOW_WIDTH / 2, SDL_WINDOW_HEIGHT );
                var p3 = new SDL.SDL_Point( 0, SDL_WINDOW_HEIGHT / 2 );
                var p4 = new SDL.SDL_Point( SDL_WINDOW_WIDTH, SDL_WINDOW_HEIGHT / 2 );
                var c = Color.White;
                renderer.DrawLine( p1, p2, c );
                renderer.DrawLine( p3, p4, c );
            }
            
            if( true )
            {
                // Draw a blue rect
                var rect1 = new SDL.SDL_Rect( 32, 32, 64, 64 );
                var c = Color.FromArgb( 255, 0, 128, 128 );
                renderer.DrawFilledRect( rect1, c );
                
                // Draw a translucent red rect
                var rect2 = new SDL.SDL_Rect( rect1.x + 32, rect1.y + 32, rect1.w, rect1.h );
                c = Color.FromArgb( 128, 255, 0, 0 );
                renderer.DrawFilledRect( rect2, c );
                // Outline it
                c = Color.FromArgb( 255, 255, 0, 0 );
                renderer.DrawRect( rect2, c );
                
                // Draw a couple translucent lines over the rects
                var p1 = new SDL.SDL_Point( rect1.x, rect1.x );
                var p2 = new SDL.SDL_Point( p1.x + 64, p1.y + 64 );
                var p3 = new SDL.SDL_Point( p1.x + 32, p1.y + 64 );
                var p4 = new SDL.SDL_Point( p1.x + 64, p1.y + 32 );
                c = Color.FromArgb( 128, 255, 255, 255 );
                renderer.DrawLine( p1, p2, c );
                renderer.DrawLine( p3, p4, c );
            }
            
            if( true )
            {
                // Draw a yellow circle
                var p1 = new SDL.SDL_Point( 192, 64 );
                var c = Color.Yellow;
                renderer.DrawFilledCircle( p1, 32, c );
                
                // Draw a magenta circle
                var p2 = new SDL.SDL_Point( p1.x + 32, p1.y + 32 );
                c = Color.FromArgb( 128, 255, 0, 255 );
                renderer.DrawFilledCircle( p2, 32, c );
                // Outline it
                c = Color.FromArgb( 255, 255, 0, 255 );
                renderer.DrawCircle( p2, 32, c );
            }
            
            if( true )
            {
                var c = Color.White;
                
                // Blit the surface
                var rect1 = new SDL.SDL_Rect( 32, 192, surface.Width, surface.Height );
                surface.ColorMod = c;
                renderer.Blit( rect1, surface );
                
                // Blit the texture
                var rect2 = new SDL.SDL_Rect( rect1.x + surface.Width / 4, rect1.y + surface.Height / 4, rect1.w, rect1.h );
                texture.ColorMod = c;
                renderer.Blit( rect2, texture );
            }
            
            if( true )
            {
                // Don't do scene control in DrawScene, this is just an example
                spinAngle += spinStep;
                if( spinAngle > 360.0 ) spinAngle -= 360.0;
                
                // Orbit
                var c = Color.FromArgb( 255, 64, 64, 64 );
                renderer.DrawCircle( spinCentre, (int)spinDistance, c );
                
                // Blit the surface as a sprite
                BlitAsSprite( renderer, surface, spinAngle, surfaceColor );
                
                // Blit the texture as a sprite
                BlitAsSprite( renderer, texture, -spinAngle, textureColor );
            }
            
            if( true )
            {
                // Draw some text
                const string text = "They're Pinky and the Brain!";
                
                // Solid White Bold
                var c = Color.White;
                var p = new SDL.SDL_Point( 32, 384 );
                renderer.DrawText( p, font, text, c, SDL_ttf.TTF_STYLE_BOLD );
                
                // Alpha Red Italic
                c = Color.FromArgb( 128, 255, 0, 0 );
                p.y += 16;
                renderer.DrawText( p, font, text, c, SDL_ttf.TTF_STYLE_ITALIC );
                
                // Solid Green Strike-Through
                c = Color.FromArgb( 255, 0, 255, 0 );
                p.y += 16;
                renderer.DrawText( p, font, text, c, SDL_ttf.TTF_STYLE_STRIKETHROUGH );
                
                // Alpha Blue Underline
                c = Color.FromArgb( 128, 0, 0, 255 );
                p.y += 16;
                renderer.DrawText( p, font, text, c, SDL_ttf.TTF_STYLE_UNDERLINE );
                
            }
        }
        
        SDL.SDL_Point RotateAround( double d, double a )
        {
            var c = Math.Cos( a * DEG_TO_RAD );
            var s = Math.Sin( a * DEG_TO_RAD );
            var rX = d * c;
            var rY = d * s;
            return new SDL.SDL_Point( (int)rX, (int)rY );
        }
        
        void BlitAsSprite( SDLRenderer renderer, SDLRenderer.Surface surface, double angle, Color c )
        {
            var pos = spinCentre.Add( RotateAround( spinDistance, angle ) );
            var rect = new SDL.SDL_Rect( spriteRect.x + pos.x, spriteRect.y + pos.y, spriteRect.w, spriteRect.h );
            surface.ColorMod = c;
            renderer.Blit( rect, surface );
        }
        
        void BlitAsSprite( SDLRenderer renderer, SDLRenderer.Texture texture, double angle, Color c )
        {
            var pos = spinCentre.Add( RotateAround( spinDistance, angle ) );
            var rect = new SDL.SDL_Rect( spriteRect.x + pos.x, spriteRect.y + pos.y, spriteRect.w, spriteRect.h );
            texture.ColorMod = c;
            renderer.Blit( rect, texture );
        }
        
    }
    
    #endregion
    
    #endregion
    
    #region User Input Events
    
    // NOTE:  These callbacks will be run in the SDLRenderer thread.
    //
    // Access to global resources should use the appropriate safe-guards for a
    // multi-threaded envirionment.
    
    // void SDLRenderer.Client_Delegate_SDL_Event( SDLRenderer renderer, SDL.SDL_Event e )
    void EventReporter( SDLRenderer renderer, SDL.SDL_Event e )
    {
        var str = string.Format( "EventReporter : Event from SDLRenderer.EventDispatcher: 0x{0}", e.type.ToString( "X" ) );
        Console.WriteLine( str );
    }
    
    #endregion
    
    #region RendererReset
    
    // NOTE:  This callback will be run in the SDLRenderer thread.
    //
    // Access to global resources should use the appropriate safe-guards for a
    // multi-threaded envirionment.
    
    // void SDLRenderer.Client_Delegate_RendererReset( SDLRenderer renderer );
    void SDLRendererReset( SDLRenderer renderer )
    {
        // The underlying SDL_Window and/or SDL_Renderer changed, we need to
        // recreate our assets
        ReleaseAssets();
        CreateAssetsForRenderer( renderer );
        
        // Tell all the example scenes to recreate their assets
        SDLExampleSet.RendererReset( renderer );
    }
    
    #endregion
    
    #region SDL_Window Closed Event
    
    // void SDLRenderer.Client_Delegate_WindowClosed( SDLRenderer renderer );
    //
    // NOTE: This will be invoked asyncronously in the example forms thread (the
    // thread the SDLRenderer was created in) when the SDL_Window has been closed
    // by the user clicking the close window button.
    //
    // NOTE 2: This event only occurs and can only be registered for when creating
    // the SDLRenderer as a stand-alone SDL_Window.  See: SetupRenderer()
    void SDLWindowClosed( SDLRenderer renderer )
    {
        Console.WriteLine( "SDLWindowClosed : Event from SDLRenderer.EventDispatcher" );
        
        // User closed the SDL_Window
        ShutdownRenderer();
    }
    
    #endregion
    
    #endregion
    
    #region Performance Feedback Timer Event Method
    
    void TimerElapsed( object sender, EventArgs e )
    {
        if( sdlRenderer == null ) return;
        var str = string.Format( "FPS (Actual/Potential): {0}/{1}\tAverage Frame Time: {2}ms\tAverage Frame Ticks: {3}", sdlRenderer.ActualFPS, sdlRenderer.PotentialFPS, sdlRenderer.AverageFrameTimeMS, sdlRenderer.AverageFrameTimeTicks );
        Console.WriteLine( str );
    }
    
    #endregion
    
    #endregion
    
    #region Example Form, Nothing interesting in here, This would be your project code otherwise
    
    #region Example control constants
    
    // Literal constants suck, the layout is semi-dynamic by changing
    // the values of these and the SDL_WINDOW named constants.
    public const int WINDOW_TITLE = 24;
    public const int WINDOW_PADDING = 13;
    public const int CONTROL_PADDING = 6;
    public const int CONTROL_WIDTH = 100;
    public const int CONTROL_HEIGHT = 24;
    public const int BASE_ELEMENTS = 3; // # of static checkboxes, buttons, etc
    
    #endregion
    
    #region Example controls and variables
    
    static Random random = new Random();
    
    CheckBox checkAnchored;
    Button buttonInit;
    Button buttonSave;
    System.Timers.Timer timer;
    
    #endregion
    
    #region Calculcate and set Example Form size
    
    void CalculateWindowSize( bool forceSmall = false )
    {
        int elements = BASE_ELEMENTS + SDLExampleSet.Count;
        var wid = CONTROL_WIDTH + ( WINDOW_PADDING * 2 );
        var hei = WINDOW_TITLE + ( CONTROL_HEIGHT * elements ) + ( WINDOW_PADDING * 2 ) + ( CONTROL_PADDING * ( elements - 1 ) );
        if(
            ( !forceSmall )&&
            ( checkAnchored != null )&&
            ( checkAnchored.Checked )
        )
        {
            wid += CONTROL_PADDING + SDL_WINDOW_WIDTH;
            hei = Math.Max( hei, ( WINDOW_TITLE + SDL_WINDOW_HEIGHT + ( WINDOW_PADDING * 2 ) ) );
        }
        Size = new Size( wid, hei );
    }
    
    #endregion
    
    #region Example Form Constructor <--- SDLExampleSceneRenders are instanced here
    
    public SDLRendererExampleForm()
    {
        // Make the WinForms window
        MaximizeBox = false;
        MinimizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        FormClosing += ExampleClosing;
        
        // This is what we're going to attach the SDL2 window to
        gamePanel = new Panel();
        gamePanel.Size = new Size( SDL_WINDOW_WIDTH, SDL_WINDOW_HEIGHT );
        gamePanel.Location = new Point( WINDOW_PADDING + CONTROL_WIDTH + CONTROL_PADDING, WINDOW_PADDING );
        Controls.Add( gamePanel );
        
        // Anchored checkbox
        checkAnchored = new CheckBox();
        checkAnchored.Text = "Anchor";
        checkAnchored.CalculcateControlSizeAndLocation( 0 );
        checkAnchored.Checked = true;
        Controls.Add( checkAnchored );
        
        // Add some buttons
        buttonInit      = MakeButton( "Init"        , 1, InitClicked );
        buttonSave      = MakeButton( "Save PNG"    , 2, SaveClicked );
        SDLExampleSet.Add( new exDrawPoints(        this, "Points"          ) );
        SDLExampleSet.Add( new exDrawPoints2(       this, "Points 2"        ) );
        SDLExampleSet.Add( new exDrawLines(         this, "Lines"           ) );
        SDLExampleSet.Add( new exDrawRects(         this, "Rects"           ) );
        SDLExampleSet.Add( new exDrawFilledRects(   this, "Filled Rects"    ) );
        SDLExampleSet.Add( new exDrawCircles(       this, "Circles"         ) );
        SDLExampleSet.Add( new exDrawFilledCircles( this, "Filled Circles"  ) );
        SDLExampleSet.Add( new exBlitSurfaces(      this, "Surfaces"        ) );
        SDLExampleSet.Add( new exBlitTextures(      this, "Textures"        ) );
        SDLExampleSet.Add( new exDrawText(          this, "Text"            ) );
        SDLExampleSet.Add( new exDrawText2(         this, "Text 2"          ) );
        SDLExampleSet.Add( new exSample1(           this, "Sample 1"        ) );
        
        // Add a performance feedback timer
        timer = new System.Timers.Timer();
        timer.Interval = PROFILE_FREQUENCY_MS;
        timer.AutoReset = true;
        timer.Elapsed += TimerElapsed;
        
        // Now all the controls are created, set the form size
        CalculateWindowSize( true );
    }
    
    Button MakeButton( string text, int position, EventHandler click )
    {
        var button = new Button();
        button.Text = text;
        button.CalculcateControlSizeAndLocation( position );
        button.Click += click;
        Controls.Add( button );
        return button;
    }
    
    #endregion
    
    #region Init Button Event
    
    void InitClicked( object sender, EventArgs e )
    {
        if( sdlRenderer == null )
        {
            SetupRenderer();
        }
        else
        {
            ShutdownRenderer();
        }
    }
    
    void SaveClicked( object sender, EventArgs e )
    {
        if( sdlRenderer != null )
            sdlRenderer.SaveSurface( ImageTypes.PNG, "example1.png" );
    }
    
    #endregion
    
    #region Example Form Close Method
    
    void ExampleClosing( object sender, FormClosingEventArgs e )
    {
        ShutdownRenderer();
        
        // Old habits again...
        timer.Dispose();
    }
    
    #endregion
    
    #region SDL example "scene manager" and "scene base class"
    
    #region Example "Scene Base Class"
    
    public abstract class SDLExampleSceneRender
    {
        bool _enabled = false;
        public bool Enabled { get { return _enabled; } }
        
        SDLRenderer _renderer;
        public SDLRenderer Renderer
        {
            get
            {
                return _renderer;
            }
            set
            {
                if( value != null )
                    RendererInit( value );
                else
                    RendererDenit( _renderer );
                _renderer = value;
                _enabled = false;
            }
        }
        
        Button _button = null;
        Form _form = null;
        
        string _optOn = string.Empty;
        string _optOff = string.Empty;
        
        public SDLExampleSceneRender()
        {
            throw new NotImplementedException( "Cannot create an SDLExampleSceneRender() via a constructor taking no arguements!" );
        }
        
        public SDLExampleSceneRender( Form form, string optOn, string optOff = null )
        {
            if( form == null ) throw new ArgumentNullException( "form", "Cannot be null!" );
            if( string.IsNullOrEmpty( optOn ) ) throw new ArgumentNullException( "optOn", "Cannot be null!" );
            
            _form = form;
            _optOn = optOn;
            _optOff = string.IsNullOrEmpty( optOff ) ? optOn : optOff;
            _enabled = false;
            _renderer = null;
            
            _button = new Button();
            _button.Text = optOn;
            _button.CalculcateControlSizeAndLocation( BASE_ELEMENTS + SDLExampleSet.Count );
            _button.Click += ButtonClick;
            form.Controls.Add( _button );
        }
        
        void ButtonClick( object sender, EventArgs e )
        {
            ToggleState();
        }
        
        public void ToggleState()
        {
            EnableState( !_enabled );
        }
        
        public void EnableState( bool enabled )
        {
            if( enabled == _enabled ) return;
            if( _renderer == null )
            {
                _enabled = false;
                _button.Text = _optOn;
                return;
            }
            _enabled = enabled;
            
            // Add/Remove a render scene callback
            if( _enabled )
            {
                _button.Text = _optOff;
                _renderer.DrawScene += DrawScene;
            }
            else
            {
                _button.Text = _optOn;
                _renderer.DrawScene -= DrawScene;
            }
        }
        
        public abstract void DrawScene( SDLRenderer renderer );
        
        public virtual void RendererInit( SDLRenderer render ) {}
        public virtual void RendererDenit( SDLRenderer render ) {}
        public virtual void RendererReset( SDLRenderer render ) {}
        
    }
    
    #endregion
    
    #region Example "scene manager"
    
    public static class SDLExampleSet
    {
        static List<SDLExampleSceneRender> _examples = new List<SDLExampleSceneRender>();
        
        public static void RendererReset( SDLRenderer renderer )
        {
            foreach( var example in _examples )
                if( example.Enabled ) example.RendererReset( renderer );
        }
        
        public static void UpdateRenderer( SDLRenderer renderer )
        {
            foreach( var example in _examples )
                example.Renderer = renderer;
        }
        
        public static void EnableDisableAll( bool enable )
        {
            foreach( var example in _examples )
                example.EnableState( enable );
        }
        
        public static void Add( SDLExampleSceneRender scene )
        {
            _examples.Add( scene );
        }
        
        public static int Count
        {
            get
            {
                return _examples.Count;
            }
        }
    }
    
    #endregion
    
    #endregion
    
    #endregion
    
    #region Program Entry Point
    
    [STAThread]
    static void Main()
    {
        Application.Run( new SDLRendererExampleForm() );
    }
    
    #endregion
    
}


#region Simple helper class for this example program

public static class ExampleHelper
{
    
    public static void CalculcateControlSizeAndLocation<T>( this T control, int position ) where T: Control
    {
        var x = SDLRendererExampleForm.WINDOW_PADDING;
        var y = SDLRendererExampleForm.WINDOW_PADDING + ( position * SDLRendererExampleForm.CONTROL_HEIGHT ) + ( position > 0 ? ( position * SDLRendererExampleForm.CONTROL_PADDING ) : 0 );
        control.Location = new Point(
            x,
            y
        );
        control.Size = new Size( SDLRendererExampleForm.CONTROL_WIDTH, SDLRendererExampleForm.CONTROL_HEIGHT );
    }
    
}

#endregion

