/*
 * SDLRenderer_Surface.cs
 *
 * Public and internal methods for abstracting SDL_Surfaces.
 *
 * For saving and loading from files, see SDLRenderer_Image.cs
 *
 * User: 1000101
 * Date: 1/31/2018
 * Time: 10:24 AM
 * 
 */
using System;

using Color = System.Drawing.Color;
using SDL2;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        #region Public API:  Create & Destroy SDLRenderer.Surface
        
        public Surface CreateSurface( int width, int height )
        {
            return Surface.INTERNAL_Surface_Create( this, width, height, this.BitsPerPixel, this.PixelFormat );
        }
        
        public Surface CreateSurface( int width, int height, uint pixelFormat )
        {
            return Surface.INTERNAL_Surface_Create( this, width, height, this.BitsPerPixel, pixelFormat );
        }
        
        public Surface CreateSurface( int width, int height, int bpp, uint pixelFormat )
        {
            return Surface.INTERNAL_Surface_Create( this, width, height, bpp, pixelFormat );
        }
        
        public void DestroySurface( ref Surface surface )
        {
            surface.Dispose();
            surface = null;
        }
        
        #endregion
        
        #region Public API:  SDLRenderer.Surface
        
        public class Surface : IDisposable
        {
            
            #region Semi-Public API:  The underlying SDL_Surface.
            
            public IntPtr SDLSurface;
            public unsafe SDL.SDL_Surface* SDLSurfacePtr;
            
            #endregion
            
            #region Internal API:  Surface control objects
            
            Texture _texture;
            SDLRenderer _renderer;
            
            uint _PixelFormat;
            int _bpp;
            int _Width;
            int _Height;
            uint _Rmask;
            uint _Gmask;
            uint _Bmask;
            uint _Amask;
            
            #endregion
            
            #region Public API:  Access to the cached Texture for the Surface.
            
            public Texture Texture
            {
                get
                {
                    if( _texture == null )
                        _texture = Texture.INTERNAL_Texture_Create( this );
                    return _texture;
                }
            }
            
            public void DeleteTexture()
            {
                if( _texture == null ) return;
                _texture.Dispose();
                _texture = null;
            }
            
            #endregion
            
            #region Public API:  The SDLRenderer associated with this Surface.
            
            public SDLRenderer Renderer
            {
                get
                {
                    return _renderer;
                }
            }
            
            #endregion
            
            #region Semi-Public API:  Destructor & IDispose
            
            bool _disposed = false;
            
            ~Surface()
            {
                Dispose( false );
            }
            
            public void Dispose()
            {
                Dispose( true );
                GC.SuppressFinalize( this );
            }
            
            protected virtual void Dispose( bool disposing )
            {
                if( _disposed ) return;
                
                DeleteTexture();
                
                if( SDLSurface != IntPtr.Zero )
                    SDL.SDL_FreeSurface( SDLSurface );
                SDLSurface = IntPtr.Zero;
                unsafe
                {
                    SDLSurfacePtr = null;
                }
                _renderer = null;
                
                _disposed = true;
            }
            
            #endregion
            
            #region Internal:  Surface Creation
            
            bool FillOutInfo()
            {
                unsafe
                {
                    // Get the SDL_Surface*
                    SDLSurfacePtr = (SDL.SDL_Surface*)( SDLSurface.ToPointer() );
                    
                    // Fetch the SDL_Surface pixel format
                    var fmtPtr = (uint*)( SDLSurfacePtr->format.ToPointer() );
                    _PixelFormat = *fmtPtr;
                    
                    // Get the size of the SDL_Surface
                    _Width = SDLSurfacePtr->w;
                    _Height = SDLSurfacePtr->h;
                }
                
                // And now it's bits per pixel and channel masks
                if( SDL.SDL_PixelFormatEnumToMasks(
                    _PixelFormat,
                    out _bpp,
                    out _Rmask,
                    out _Gmask,
                    out _Bmask,
                    out _Amask ) == SDL.SDL_bool.SDL_FALSE )
                    return false;
                
                return true;
            }
            
            internal static Surface INTERNAL_Surface_Create( SDLRenderer renderer, int width, int height, int bpp, uint pixelFormat )
            {
                // Create Surface instance
                var surface = new Surface();
                
                // Assign the renderer
                surface._renderer = renderer;
                
                // Create from the renderer
                surface.SDLSurface = SDL.SDL_CreateRGBSurfaceWithFormat( 0, width, height, bpp, pixelFormat );
                
                // Fetch the Surface formatting information
                if( !surface.FillOutInfo() )
                {
                    // Someting dun goned wrung
                    surface.Dispose();
                    return null;
                }
                
                return surface;
            }
            
            internal static Surface INTERNAL_Surface_Wrap( SDLRenderer renderer, IntPtr sdlSurface )
            {
                // Create Surface instance
                var surface = new Surface();
                
                // Assign the renderer
                surface._renderer = renderer;
                
                // Assign the SDL_Surface
                surface.SDLSurface = sdlSurface;
                
                // Fetch the Surface formatting information
                if( !surface.FillOutInfo() )
                {
                    // Someting dun goned wrung
                    surface.Dispose();
                    return null;
                }
                
                return surface;
            }
            
            #endregion
            
            #region Public API:  Surface Properties
            
            /// <summary>
            /// Get/Set the Surface alpha blend mode.
            /// </summary>
            public SDL.SDL_BlendMode BlendMode
            {
                get
                {
                    SDL.SDL_BlendMode mode;
                    return SDL.SDL_GetSurfaceBlendMode( SDLSurface, out mode ) != 0 ? SDL.SDL_BlendMode.SDL_BLENDMODE_INVALID : mode;
                }
                set
                {
                    SDL.SDL_SetSurfaceBlendMode( SDLSurface, value );
                }
            }
            
            /// <summary>
            /// Get/Set the whole Surface alpha modulation.
            /// </summary>
            public byte AlphaMod
            {
                get
                {
                    byte alpha;
                    return SDL.SDL_GetSurfaceAlphaMod( SDLSurface, out alpha ) != 0 ? (byte)0 : alpha;
                }
                set
                {
                    SDL.SDL_SetSurfaceAlphaMod( SDLSurface, value );
                }
            }
            
            /// <summary>
            /// Get/Set the whole Surface color modulation.
            /// </summary>
            public Color ColorMod
            {
                get
                {
                    byte r, g, b;
                    return SDL.SDL_GetSurfaceColorMod( SDLSurface, out r, out g, out b ) != 0 ? Color.Black : Color.FromArgb( r, g, b );
                }
                set
                {
                    SDL.SDL_SetSurfaceColorMod( SDLSurface, value.R, value.G, value.B );
                }
            }
            
            /// <summary>
            /// The SDL_PIXELFORMAT of the SDL_Surface.
            /// </summary>
            public uint PixelFormat
            {
                get
                {
                    return _PixelFormat;
                }
            }
            
            /// <summary>
            /// The number of Bits Per Pixel (bpp) of the SDL_Surface.
            /// </summary>
            public int BitsPerPixel
            {
                get
                {
                    return _bpp;
                }
            }
            
            /// <summary>
            /// The width of the SDL_Surface.
            /// </summary>
            public int Width
            {
                get
                {
                    return _Width;
                }
            }
            
            /// <summary>
            /// The Height of the SDL_Surface.
            /// </summary>
            public int Height
            {
                get
                {
                    return _Height;
                }
            }
            
            /// <summary>
            /// The alpha channel mask of the SDL_Surface.
            /// </summary>
            public uint AlphaMask
            {
                get
                {
                    return _Amask;
                }
            }
            
            /// <summary>
            /// The red channel mask of the SDL_Surface.
            /// </summary>
            public uint RedMask
            {
                get
                {
                    return _Rmask;
                }
            }
            
            /// <summary>
            /// The green channel mask of the SDL_Surface.
            /// </summary>
            public uint GreenMask
            {
                get
                {
                    return _Gmask;
                }
            }
            
            /// <summary>
            /// The blue channel mask of the SDL_Surface.
            /// </summary>
            public uint BlueMask
            {
                get
                {
                    return _Bmask;
                }
            }
            
            #endregion
            
            #region Public API:  Surface Methods
            
            public uint CompatColor( Color c )
            {
                unsafe
                {
                    return SDL.SDL_MapRGBA( SDLSurfacePtr->format, c.R, c.G, c.B, c.A );
                }
            }
            
            #endregion
            
            
        }
        
        #endregion
        
    }
}
