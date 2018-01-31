/*
 * SDLRenderer_Texture.cs
 *
 * Public and internal methods for abstracting SDL_Textures.
 *
 * TODO:  Add additional create/load/save methods.
 *
 * User: 1000101
 * Date: 1/31/2018
 * Time: 10:27 AM
 * 
 */
using System;

using Color = System.Drawing.Color;
using SDL2;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        public Texture CreateTextureFromSurface( Surface surface )
        {
            return Texture.CreateTextureFromSurface( surface );
        }
        
        public void DestroyTexture( ref Texture texture )
        {
            texture.Dispose();
            texture = null;
        }
        
        public class Texture
        {
            
            public IntPtr SDLTexture;
            
            bool _disposed = false;
            
            ~Texture()
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
                
                if( SDLTexture != IntPtr.Zero )
                    SDL.SDL_DestroyTexture( SDLTexture );
                SDLTexture = IntPtr.Zero;
                
                _disposed = true;
            }
            
            public static Texture CreateTextureFromSurface( Surface surface )
            {
                var texture = new Texture();
                texture.SDLTexture = SDL.SDL_CreateTextureFromSurface( surface.Renderer.Renderer, surface.SDLSurface );
                
                // Copy SDL_Surface characteristics to the SDL_Texture
                texture.BlendMode = surface.BlendMode;
                texture.AlphaMod = surface.AlphaMod;
                texture.ColorMod = surface.ColorMod;
                
                return texture;
            }
            
            public SDL.SDL_BlendMode BlendMode
            {
                get
                {
                    SDL.SDL_BlendMode mode;
                    return SDL.SDL_GetTextureBlendMode( SDLTexture, out mode ) != 0 ? SDL.SDL_BlendMode.SDL_BLENDMODE_INVALID : mode;
                }
                set
                {
                    SDL.SDL_SetTextureBlendMode( SDLTexture, value );
                }
            }
            
            public byte AlphaMod
            {
                get
                {
                    byte alpha;
                    return SDL.SDL_GetTextureAlphaMod( SDLTexture, out alpha ) != 0 ? (byte)0 : alpha;
                }
                set
                {
                    SDL.SDL_SetTextureAlphaMod( SDLTexture, value );
                }
            }
            
            public Color ColorMod
            {
                get
                {
                    byte r, g, b;
                    if( SDL.SDL_GetTextureColorMod( SDLTexture, out r, out g, out b ) != 0 ) return Color.Black;
                    return  Color.FromArgb( r, g, b );
                }
                set
                {
                    SDL.SDL_SetTextureColorMod( SDLTexture, value.R, value.G, value.B );
                }
            }
            
        }
        
    }
}
