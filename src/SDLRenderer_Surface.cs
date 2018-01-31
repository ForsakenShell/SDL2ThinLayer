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
        
        public Surface CreateSurface( int width, int height )
        {
            return Surface.CreateSurfaceFromRenderer( this, width, height );
        }
        
        public void DestroySurface( ref Surface surface )
        {
            surface.Dispose();
            surface = null;
        }
        
        public class Surface : IDisposable
        {
            
            public IntPtr SDLSurface;
            public unsafe SDL.SDL_Surface* SDLSurfacePtr;
            
            Texture _texture;
            public Texture Texture
            {
                get
                {
                    if( _texture == null )
                        _texture = Texture.CreateTextureFromSurface( this );
                    return _texture;
                }
            }
            
            public void DeleteTexture()
            {
                if( _texture == null ) return;
                _texture.Dispose();
                _texture = null;
            }
            
            SDLRenderer _renderer;
            public SDLRenderer Renderer
            {
                get
                {
                    return _renderer;
                }
            }
            
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
                
                _disposed = true;
            }
            
            public static Surface CreateSurfaceFromRenderer( SDLRenderer renderer, int width, int height )
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
            
        }
        
    }
}
