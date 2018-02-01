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
        
        #region Public API:  Create & Destroy SDLRenderer.Texture
        
        public Texture CreateTextureFromSurface( Surface surface )
        {
            return Texture.FromSurface( surface );
        }
        
        public void DestroyTexture( ref Texture texture )
        {
            texture.Dispose();
            texture = null;
        }
        
        #endregion
        
        #region Public API:  SDLRenderer.Texture
        
        public class Texture
        {
            
            #region Semi-Public API:  The underlying SDL_Texture.
            
            public IntPtr SDLTexture;
            
            #endregion
            
            #region Internal API:  Surface control objects
            
            SDLRenderer _renderer;
            
            #endregion
            
            #region Public API:  The SDLRenderer associated with this Texture.
            
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
                _renderer = null;
                
                _disposed = true;
            }
            
            #endregion
            
            #region Public API:  Texture Creation
            
            public static Texture FromSurface( Surface surface )
            {
                var texture = new Texture();
                texture._renderer = surface.Renderer;
                texture.SDLTexture = SDL.SDL_CreateTextureFromSurface( texture.Renderer.Renderer, surface.SDLSurface );
                
                // Copy SDL_Surface characteristics to the SDL_Texture
                texture.BlendMode = surface.BlendMode;
                texture.AlphaMod = surface.AlphaMod;
                texture.ColorMod = surface.ColorMod;
                
                return texture;
            }
            
            #endregion
            
            #region Public API:  Texture Properties
            
            /// <summary>
            /// Get/Set the Texture alpha blend mode.
            /// </summary>
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
            
            /// <summary>
            /// Get/Set the whole Texture alpha modulation.
            /// </summary>
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
            
            /// <summary>
            /// Get/Set the whole Texture color modulation.
            /// </summary>
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
            
            #endregion
            
        }
        
        #endregion
        
    }
}
