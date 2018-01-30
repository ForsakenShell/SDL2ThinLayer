/*
 * SDLRenderer_TextureCache.cs
 *
 * This section of the SDLRenderer class handles caching SDL_Surfaces to SDL_Textures for faster repeat rendering.
 *
 * Only clearing and removing an SDL_Surface from the cache is supported from client code.
 *
 * User: 1000101
 * Date: 29/01/2018
 * Time: 9:11 AM
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using SDL2;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        #region Texture cache
        
        unsafe Dictionary<int, IntPtr> _textureCache;
        
        #endregion
        
        unsafe IntPtr TextureFromCache( SDL.SDL_Surface* surface )
        {
            if( surface == null )
                return IntPtr.Zero;
            
            if( _textureCache == null )
                _textureCache = new Dictionary<int, IntPtr>();
            
            IntPtr texture;
            int hash = surface->GetHashCode();
            if( _textureCache.TryGetValue( hash, out texture ) )
                return texture;
            
            texture = CreateTextureFromSurface( surface );
            
            _textureCache.Add( hash, texture );
            return texture;
        }
        
        /// <summary>
        /// Number of SDL_Textures in the texture cache.
        /// </summary>
        public int TextureCacheCount
        {
            get
            {
                if( _textureCache == null ) return 0;
                return _textureCache.Count;
            }
        }
        
        /// <summary>
        /// Clears the texture cache which is used to copy SDL_Surfaces to temporary SDL_Textures.  This will be called
        /// automatically when SDLRenderer is disposed of and it is not necessary to call it directly.
        /// </summary>
        public void ClearTextureCache()
        {
            if( _textureCache == null ) return;
            
            foreach( var texture in _textureCache.Values )
                DestroyTexture( texture );
            
            _textureCache.Clear();
            _textureCache = null;
        }
        
        /// <summary>
        /// Removes an SDL_Surface from the SDL_Texture cache, forcing it to be recreated on the next Blit.
        /// 
        /// If blitting surfaces directly, you should call this after you make any changes to the surface to force it to be recached.
        /// </summary>
        /// <param name="surface">SDL_Surface to remove from the cache.</param>
        unsafe public void RemoveSurfaceFromCache( SDL.SDL_Surface* surface )
        {
            if( surface == null ) return;
            if( _textureCache == null ) return;
            
            IntPtr texture;
            int hash = surface->GetHashCode();
            if( !_textureCache.TryGetValue( hash, out texture ) ) return;
            
            DestroyTexture( texture );
            _textureCache.Remove( hash );
        }
        
    }
}
