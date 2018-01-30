/*
 * SDLRenderer_SurfaceTexture.cs
 *
 * Create, Destroy, Load, Save SDL_Surfaces and SDL_Textures compatible with the SDLRenderer state machine.
 *
 * TODO:  Add additional create/load/save methods.
 *
 * User: 1000101
 * Date: 28/01/2018
 * Time: 4:40 AM
 * 
 */
using System;

using SDL2;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        unsafe public SDL.SDL_Surface* CreateSurface( int x, int y )
        {
            var ipSurface = SDL.SDL_CreateRGBSurfaceWithFormat( 0, x, y, _sdlWindow_bpp, _sdlWindow_PixelFormat );
            var spSurface = (SDL.SDL_Surface*)( ipSurface.ToPointer() );
            return spSurface;
        }
        
        unsafe public IntPtr CreateTextureFromSurface( SDL.SDL_Surface* srcSurface )
        {
            var ipSurface = new IntPtr( srcSurface );
            var texture = SDL.SDL_CreateTextureFromSurface( _sdlRenderer, ipSurface );
            
            // Copy SDL_Surface characteristics to the SDL_Texture
            
            SDL.SDL_BlendMode sBlend;
            if( SDL.SDL_GetSurfaceBlendMode( ipSurface, out sBlend ) == 0 )
                SDL.SDL_SetTextureBlendMode( texture, sBlend );
            
            byte alpha;
            if( SDL.SDL_GetSurfaceAlphaMod( ipSurface, out alpha ) == 0 )
                SDL.SDL_SetTextureAlphaMod( texture, alpha );
            
            byte r;
            byte g;
            byte b;
            if( SDL.SDL_GetSurfaceColorMod( ipSurface, out r, out g, out b ) == 0 )
                SDL.SDL_SetTextureColorMod( texture, r, g, b );
            
            return texture;
        }
        
        unsafe public void DestroySurface( SDL.SDL_Surface* srcSurface )
        {
            var ipSurface = new IntPtr( srcSurface );
            SDL.SDL_FreeSurface( ipSurface );
        }
        
        public void DestroyTexture( IntPtr srcTexture )
        {
            SDL.SDL_DestroyTexture( srcTexture );
        }
    }
}
