/*
 * SDLRenderer_Surface.cs
 *
 * Public and internal methods for abstracting SDL_Surfaces.
 *
 * TODO:  Add additional create/load/save methods.
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
            return Surface.FromRenderer( this, width, height );
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
            
            #endregion
            
            #region Public API:  Access to the cached Texture for the Surface.
            
            public Texture Texture
            {
                get
                {
                    if( _texture == null )
                        _texture = Texture.FromSurface( this );
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
            
            #region Public API:  Surface Creation
            
            public static Surface FromRenderer( SDLRenderer renderer, int width, int height )
            {
                var surface = new Surface();
                surface._renderer = renderer;
                surface.SDLSurface = SDL.SDL_CreateRGBSurfaceWithFormat( 0, width, height, renderer.BitsPerPixel, renderer.PixelFormat );
                unsafe
                {
                    surface.SDLSurfacePtr = (SDL.SDL_Surface*)( surface.SDLSurface.ToPointer() );
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
                    if( SDL.SDL_GetSurfaceColorMod( SDLSurface, out r, out g, out b ) != 0 ) return Color.Black;
                    return  Color.FromArgb( r, g, b );
                }
                set
                {
                    SDL.SDL_SetSurfaceColorMod( SDLSurface, value.R, value.G, value.B );
                }
            }
            
            #endregion
            
            #region Public API:  Surface Drawing Primitives
            
            public void DrawFilledRect( SDL.SDL_Rect rect, Color c )
            {
                unsafe
                {
                    SDL.SDL_FillRect( SDLSurface, ref rect, SDL.SDL_MapRGBA( SDLSurfacePtr->format, c.R, c.G, c.B, c.A ) );
                }
            }
            
            public void DrawFilledRects( SDL.SDL_Rect[] rects, int count, Color c )
            {
                unsafe
                {
                    SDL.SDL_FillRects( SDLSurface, rects, count, SDL.SDL_MapRGBA( SDLSurfacePtr->format, c.R, c.G, c.B, c.A ) );
                }
            }
            
            #endregion
            
        }
        
        #endregion
        
    }
}
