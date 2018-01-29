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
    const int SDL_WINDOW_WIDTH = 640;
    const int SDL_WINDOW_HEIGHT = 480;
    
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
    
    // SDL_Surfaces are a deprecated technology and there are performance costs when using them with SDLRenderer
    unsafe SDL.SDL_Surface* sprite;
    
    // SDL_Textures are hardware based and are much faster.
    IntPtr texture;
    
    #endregion
    
    #region Init/Denit SDLRenderer as well as create an example SDL_Surface and SDL_Texture
    
    void SetupRenderer()
    {
        // Example form changes, ignore the next few lines
        buttonInit.Text = "Denit";
        showLines = false;
        showSprites = false;
        showTextures = false;
        
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
        
        // Set the render blender mode
        SDL.SDL_SetRenderDrawBlendMode( sdlRenderer.Renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND );
        
        // Create a simple SDL_Surface
        //
        // NOTE:  SDL_Surfaces are deprecated and require the use of unsafe code blocks.
        unsafe
        {
            sprite = sdlRenderer.CreateSurface( 64, 64 );
            var rect = new SDL.SDL_Rect();
            rect.x = 0;
            rect.y = 0;
            rect.w = 64;
            rect.h = 64;
            var ipSprite = new IntPtr( sprite );
            SDL.SDL_FillRect( ipSprite, ref rect, 0x00FF00FF );
            bool sToggle = false;
            bool toggle = false;
            rect.w = 8;
            rect.h = 8;
            for( int y = 0; y < 64; y += 8 )
            {
                toggle = sToggle;
                for( int x = 0; x < 64; x += 8 )
                {
                    if( toggle )
                    {
                        rect.x = x;
                        rect.y = y;
                        SDL.SDL_FillRect( ipSprite, ref rect, 0xFFFFFFFF );
                    }
                    toggle = !toggle;
                }
                sToggle = !sToggle;
            }
            SDL.SDL_SetSurfaceBlendMode( ipSprite, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND );
        }
        
        // Create an SDL_Texture from the SDL_Surface.
        //
        // NOTE:  SDL_Textures do not require unsafe code blocks but because we are
        // creating one from an SDL_Surface we do.
        unsafe
        {
            // No need to set the blend mode, etc - all rendering information is copied
            // directly in SDLRenderer.CreateTextureFromSurface() from the SDL_Surface settings.
            texture = sdlRenderer.CreateTextureFromSurface( sprite );
        }
        
        // Start the performance feedback timer (more example form stuff)
        timer.Start();
    }
    
    void ShutdownRenderer()
    {
        // (I thought this was about SDLRenderer, not the example form!)
        // Example Form change state, ignore the next couple lines and the comment above
        buttonInit.Text = "Init";
        timer.Stop();
        
        unsafe
        {
            
            // SDL_Surfaces are unmanaged so we need to explicitly destroy them
            if( sprite != null )
                sdlRenderer.DestroySurface( sprite );
            sprite = null;
        }
        
        // SDL_Textures are unmanaged so we need to explicitly destroy them
        if( texture != IntPtr.Zero )
            sdlRenderer.DestroyTexture( texture );
        texture = IntPtr.Zero;
        
        // SDLRenderer has a thread and a mix of managed and unmanaged resources.
        // SDLRenderer itself is managed and will be garbage collected, therefore
        // releasing all of said resources in this process.
        
        // While it implements IDisposable and it also explicitly disposes of it's resources
        // in it's destructor, I always like to clean up after myself (old habits).
        if( sdlRenderer != null )
            sdlRenderer.Dispose();
        
        // This is all you really need to do though, GC will handle the rest
        sdlRenderer = null;
        
    }
    
    #endregion
    
    #region Example SDLRenderer Events
    
    // NOTE:  These callbacks will be run in the SDLRenderer thread unless otherwise noted.
    //
    // Access to global resources should use the appropriate safe-guards for a
    // multi-threaded envirionment.
    
    // void SDLRenderer.Client_Delegate_DrawScene()
    void DrawSomeLines()
    {
        // You don't really want to uncomment the next line...
        // Console.WriteLine( "DrawSomeLines : Event from SDLRenderer.DrawScene" );
        
        for( int i = 0; i < ITTERATIONS; i++ )
        {
            var x1 = random.Next( gamePanel.Width );
            var y1 = random.Next( gamePanel.Height );
            var x2 = random.Next( gamePanel.Width );
            var y2 = random.Next( gamePanel.Height );
            var c = Color.FromArgb(
                random.Next( 256 ),
                random.Next( 256 ),
                random.Next( 256 ),
                random.Next( 256 )
            );
            sdlRenderer.DrawLine( x1, y1, x2, y2, c );
        }
    }
    
    // void SDLRenderer.Client_Delegate_DrawScene()
    void DrawSomeSprites()
    {
        // You don't really want to uncomment the next line...
        // Console.WriteLine( "DrawSomeSprites : Event from SDLRenderer.DrawScene" );
        
        var rect = new SDL.SDL_Rect();
        rect.w = 64;
        rect.h = 64;
        for( int i = 0; i < ITTERATIONS; i++ )
        {
            rect.x = random.Next( gamePanel.Width );
            rect.y = random.Next( gamePanel.Height );
            unsafe
            {
                sdlRenderer.Blit( rect, sprite );
            }
        }
    }
    
    // void SDLRenderer.Client_Delegate_DrawScene()
    void DrawSomeTextures()
    {
        // You don't really want to uncomment the next line...
        // Console.WriteLine( "DrawSomeTextures : Event from SDLRenderer.DrawScene" );
        
        var rect = new SDL.SDL_Rect();
        rect.w = 64;
        rect.h = 64;
        for( int i = 0; i < ITTERATIONS; i++ )
        {
            rect.x = random.Next( gamePanel.Width );
            rect.y = random.Next( gamePanel.Height );
            sdlRenderer.Blit( rect, texture );
        }
    }
    
    // void SDLRenderer.Client_Delegate_SDL_Event( SDL.SDL_Event e )
    void EventReporter( SDL.SDL_Event e )
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
    
    const int WINDOW_WIDTH = 800;
    const int WINDOW_HEIGHT = 600;
    
    #endregion
    
    #region Example controls and variables
    
    Random random = new Random();
    
    CheckBox checkAnchored;
    Button buttonInit;
    Button buttonLines;
    Button buttonSprites;
    Button buttonTextures;
    System.Timers.Timer timer;
    
    bool showLines = false;
    bool showSprites = false;
    bool showTextures = false;
    
    #endregion
    
    #region Example Form Constructor
    
    public SDLRendererExampleForm()
    {
        // Make the WinForms window
        Size = new Size( WINDOW_WIDTH, WINDOW_HEIGHT );
        FormClosing += ExampleClosing;
        
        // This is what we're going to attach the SDL2 window to
        gamePanel = new Panel();
        gamePanel.Size = new Size( SDL_WINDOW_WIDTH, SDL_WINDOW_HEIGHT );
        gamePanel.Location = new Point( 80, 10 );
        
        // Add some buttons
        
        // Init demo button and anchored checkbox
        buttonInit = new Button();
        buttonInit.Text = "Init";
        buttonInit.Location = new Point(
            10,
            gamePanel.Location.Y + gamePanel.Size.Height + 10
        );
        buttonInit.Click += InitClicked;
        
        checkAnchored = new CheckBox();
        checkAnchored.Text = "Anchor";
        checkAnchored.Location = new Point(
            buttonInit.Location.X,
            buttonInit.Location.Y + buttonInit.Size.Height + 10
        );
        checkAnchored.Checked = true;
        
        // Toggle lines
        buttonLines = new Button();
        buttonLines.Text = "Lines";
        buttonLines.Location = new Point(
            buttonInit.Location.X + buttonInit.Size.Width + 10,
            buttonInit.Location.Y
        );
        buttonLines.Click += LinesClicked;
        
        // Toggle sprites
        buttonSprites = new Button();
        buttonSprites.Text = "Sprites";
        buttonSprites.Location = new Point(
            buttonLines.Location.X + buttonLines.Size.Width + 10,
            buttonLines.Location.Y
        );
        buttonSprites.Click += SpritesClicked;
        
        // Toggle textures
        buttonTextures = new Button();
        buttonTextures.Text = "Textures";
        buttonTextures.Location = new Point(
            buttonSprites.Location.X + buttonSprites.Size.Width + 10,
            buttonSprites.Location.Y
        );
        buttonTextures.Click += TexturesClicked;
        
        // Add a performance feedback timer
        timer = new System.Timers.Timer();
        timer.Interval = PROFILE_FREQUENCY_MS;
        timer.AutoReset = true;
        timer.Elapsed += TimerElapsed;
        
        Controls.Add( gamePanel );
        Controls.Add( buttonInit );
        Controls.Add( checkAnchored );
        Controls.Add( buttonLines );
        Controls.Add( buttonSprites );
        Controls.Add( buttonTextures );
        
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
    
    void SpritesClicked( object sender, EventArgs e )
    {
        ToggleOption( ref showSprites, DrawSomeSprites );
    }
    
    void TexturesClicked( object sender, EventArgs e )
    {
        ToggleOption( ref showTextures, DrawSomeTextures );
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
        buttonInit.Dispose();
        buttonLines.Dispose();
        buttonSprites.Dispose();
        buttonTextures.Dispose();
        checkAnchored.Dispose();
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
