/*
 * Example1.cs
 *
 * This is a basic example of how to use SDL2ThinLayer.
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
    const int ITTERATIONS = 1000;
    
    // How often should the profiling information be dumped to console?
    const int PROFILE_FREQUENCY_MS = 1000;
    
    #endregion
    
    #region SDLRenderer variables, You'll need these
    
    // These are the variables you care about.
    
    // Threaded renderer and specific event queue
    SDLRenderer sdlRenderer;
    
    // Target control to set the render target to, could be any control but the one with
    // the least complexity and most flexibility is probably best (hence Panel).
    Panel gamePanel;
    
    // Surfaces are a deprecated technology and there are performance costs when using them with SDLRenderer
    SDLRenderer.Surface surface;
    
    // Textures are hardware based and are much faster.
    SDLRenderer.Texture texture;
    
    #endregion
    
    #region Init/Denit SDLRenderer as well as create an example Surface and Texture
    
    void SetupRenderer()
    {
        // Example form changes, ignore the next few lines
        CalculateWindowSize();
        buttonInit.Text = "Denit";
        showLines = false;
        showSurfaces = false;
        showTextures = false;
        showSample1 = false;
        
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
        
        // Create a simple Surface
        //
        // NOTE:  Surfaces are deprecated and require conversion to Textures before blitting.
        surface = renderer.CreateSurface( 64, 64 );
        var rect = new SDL.SDL_Rect();
        rect.x = 0;
        rect.y = 0;
        rect.w = 64;
        rect.h = 64;
        
        // No blending while we create the surface
        surface.BlendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_NONE;
        
        // Fill the surface with magenta (alpha 0) and then a white checkerboard pattern (alpha 255)
        var c = Color.FromArgb( 0, 255, 0, 255 );
        surface.DrawFilledRect( rect, c );
        bool sToggle = false;
        bool toggle = false;
        rect.w = 8;
        rect.h = 8;
        c = Color.FromArgb( 255, 255, 255, 255 );
        for( int y = 0; y < 64; y += 8 )
        {
            toggle = sToggle;
            for( int x = 0; x < 64; x += 8 )
            {
                if( toggle )
                {
                    rect.x = x;
                    rect.y = y;
                    surface.DrawFilledRect( rect, c );
                }
                toggle = !toggle;
            }
            sToggle = !sToggle;
        }
        // Now set the blend mode for surface blitting
        surface.BlendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
        
        // Create a Texture from the Surface.
        //
        // No need to set the blend mode, etc - all rendering information is copied
        // directly in SDLRenderer.CreateTextureFromSurface() from the SDL_Surface settings.
        texture = renderer.CreateTextureFromSurface( surface );
        
    }
    
    void ShutdownRenderer()
    {
        // (I thought this was about SDLRenderer, not the example form!)
        // Example Form change state, ignore the next couple lines and the comment above
        CalculateWindowSize( true );
        buttonInit.Text = "Init";
        timer.Stop();
        
        // Tell SDLRenderer to stop it's thread.  We do this so we don't destroy resources
        // being used before destroying the renderer itself.
        if( sdlRenderer != null )
            sdlRenderer.DestroyWindow();
        
        // While it SDL2ThingLayer implements IDisposable in all it's classes and
        // explicitly disposes of their resources in their destructors, I always
        // like to clean up after myself (old habits).
        
        // Dispose of the Surface
        if( surface != null )
            sdlRenderer.DestroySurface( ref surface );
        
        // Dispose of the Texture
        if( texture != null )
            sdlRenderer.DestroyTexture( ref texture );
        
        // Dispose of the Renderer
        if( sdlRenderer != null )
            sdlRenderer.Dispose();
        
        // This is all you really need to do though, GC will handle the rest
        surface = null;
        texture = null;
        sdlRenderer = null;
        
    }
    
    #endregion
    
    #region Example SDLRenderer Events
    
    // NOTE:  These callbacks will be run in the SDLRenderer thread unless otherwise noted.
    //
    // Access to global resources should use the appropriate safe-guards for a
    // multi-threaded envirionment.
    
    // void SDLRenderer.Client_Delegate_DrawScene( SDLRenderer renderer )
    void DrawSomeLines( SDLRenderer renderer )
    {
        // You don't really want to uncomment the next line...
        // Console.WriteLine( "DrawSomeLines : Event from SDLRenderer.DrawScene" );
        
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
    
    // void SDLRenderer.Client_Delegate_DrawScene( SDLRenderer renderer )
    void DrawSomeSurfaces( SDLRenderer renderer )
    {
        // You don't really want to uncomment the next line...
        // Console.WriteLine( "DrawSomeSprites : Event from SDLRenderer.DrawScene" );
        
        var rect = new SDL.SDL_Rect();
        rect.w = 64;
        rect.h = 64;
        for( int i = 0; i < ITTERATIONS; i++ )
        {
            rect.x = random.Next( SDL_WINDOW_WIDTH );
            rect.y = random.Next( SDL_WINDOW_HEIGHT );
            unsafe
            {
                renderer.Blit( rect, surface );
            }
        }
    }
    
    // void SDLRenderer.Client_Delegate_DrawScene( SDLRenderer renderer )
    void DrawSomeTextures( SDLRenderer renderer )
    {
        // You don't really want to uncomment the next line...
        // Console.WriteLine( "DrawSomeTextures : Event from SDLRenderer.DrawScene" );
        
        var rect = new SDL.SDL_Rect();
        rect.w = 64;
        rect.h = 64;
        for( int i = 0; i < ITTERATIONS; i++ )
        {
            rect.x = random.Next( SDL_WINDOW_WIDTH );
            rect.y = random.Next( SDL_WINDOW_HEIGHT );
            renderer.Blit( rect, texture );
        }
    }
    
    // void SDLRenderer.Client_Delegate_DrawScene( SDLRenderer renderer )
    void DrawSample1( SDLRenderer renderer )
    {
        // You don't really want to uncomment the next line...
        // Console.WriteLine( "DrawSample1 : Event from SDLRenderer.DrawScene" );
        
        // Draw a blue rect
        var rect = new SDL.SDL_Rect();
        rect.x = 32;
        rect.y = 32;
        rect.w = 64;
        rect.h = 64;
        var c = Color.FromArgb( 255, 0, 128, 128 );
        renderer.DrawFilledRect( rect, c );
        
        // Draw a translucent red rect
        rect.x += 32;
        rect.y += 32;
        c = Color.FromArgb( 128, 255, 0, 0 );
        renderer.DrawFilledRect( rect, c );
        
        // Draw a couple translucent lines over the rects
        var p1 = new SDL.SDL_Point();
        p1.x = 32;
        p1.y = 32;
        var p2 = new SDL.SDL_Point();
        p2.x = p1.x + 64;
        p2.y = p1.y + 64;
        c = Color.FromArgb( 128, 255, 255, 255 );
        renderer.DrawLine( p1, p2, c );
        p2.x = p1.x + 32;
        p2.y = p1.y + 64;
        p1.x += 64;
        p1.y += 32;
        renderer.DrawLine( p1, p2, c );
    }
    
    // void SDLRenderer.Client_Delegate_SDL_Event( SDLRenderer renderer, SDL.SDL_Event e )
    void EventReporter( SDLRenderer renderer, SDL.SDL_Event e )
    {
        var str = string.Format( "EventReporter : Event from SDLRenderer.EventDispatcher: 0x{0}", e.type.ToString( "X" ) );
        Console.WriteLine( str );
    }
    
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
    public const int CONTROL_ELEMENTS = 6; // # of checkboxes, buttons, etc
    
    #endregion
    
    #region Example controls and variables
    
    Random random = new Random();
    
    CheckBox checkAnchored;
    Button buttonInit;
    Button buttonLines;
    Button buttonSurfaces;
    Button buttonTextures;
    Button buttonSample1;
    System.Timers.Timer timer;
    
    bool showLines = false;
    bool showSurfaces = false;
    bool showTextures = false;
    bool showSample1 = false;
    
    #endregion
    
    #region Calculcate and set Example Form size
    
    void CalculateWindowSize( bool forceSmall = false )
    {
        var wid = CONTROL_WIDTH + ( WINDOW_PADDING * 2 );
        var hei = WINDOW_TITLE + ( CONTROL_HEIGHT * CONTROL_ELEMENTS ) + ( WINDOW_PADDING * 2 ) + ( CONTROL_PADDING * ( CONTROL_ELEMENTS - 1 ) );
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
    
    #region Example Form Constructor
    
    public SDLRendererExampleForm()
    {
        // Make the WinForms window
        MaximizeBox = false;
        MinimizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        CalculateWindowSize();
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
        buttonLines     = MakeButton( "Lines"       , 2, LinesClicked );
        buttonSurfaces  = MakeButton( "Surfaces"    , 3, SurfacesClicked );
        buttonTextures  = MakeButton( "Textures"    , 4, TexturesClicked );
        buttonSample1   = MakeButton( "Sample 1"    , 5, Sample1Clicked );
        
        // Add a performance feedback timer
        timer = new System.Timers.Timer();
        timer.Interval = PROFILE_FREQUENCY_MS;
        timer.AutoReset = true;
        timer.Elapsed += TimerElapsed;
        
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
    
    #region Init/Toggle Button Event Methods
    
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
    
    void LinesClicked( object sender, EventArgs e )
    {
        ToggleOption( ref showLines, DrawSomeLines );
    }
    
    void SurfacesClicked( object sender, EventArgs e )
    {
        ToggleOption( ref showSurfaces, DrawSomeSurfaces );
    }
    
    void TexturesClicked( object sender, EventArgs e )
    {
        ToggleOption( ref showTextures, DrawSomeTextures );
    }
    
    void Sample1Clicked( object sender, EventArgs e )
    {
        ToggleOption( ref showSample1, DrawSample1 );
    }
    
    void ToggleOption( ref bool bValue, SDLRenderer.Client_Delegate_DrawScene scene )
    {
        if( sdlRenderer == null ) return;
        
        // Add/Remove a render scene callback
        bValue = !bValue;
        if( bValue )
            sdlRenderer.DrawScene += scene;
        else
            sdlRenderer.DrawScene -= scene;
    }
    
    #endregion
    
    #region Example Form Close Method
    
    void ExampleClosing( object sender, FormClosingEventArgs e )
    {
        ShutdownRenderer();
        
        // Old habits again...
        gamePanel.Dispose();
        checkAnchored.Dispose();
        buttonInit.Dispose();
        buttonLines.Dispose();
        buttonSurfaces.Dispose();
        buttonTextures.Dispose();
        buttonSample1.Dispose();
        timer.Dispose();
    }
    
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