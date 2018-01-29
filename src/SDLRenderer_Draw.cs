/*
 * SDLRenderer_Draw.cs
 *
 * Public and internal methods for drawing primitives and blitting SDL_Surfaces and SDL_Textures.
 *
 * User: 1000101
 * Date: 28/01/2018
 * Time: 3:06 AM
 * 
 */
using System;

using SDL2;

using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        #region Internal Delegate Prototypes
        
        delegate void INTERNAL_Delegate_ClearScene();
        delegate void INTERNAL_Delegate_DrawLine( int x1, int y1, int x2, int y2, Color c );
        delegate void INTERNAL_Delegate_DrawLines( SDL.SDL_Point[] points, int count, Color c );
        delegate void INTERNAL_Delegate_DrawRect( SDL.SDL_Rect rect, Color c );
        delegate void INTERNAL_Delegate_DrawRects( SDL.SDL_Rect[] rects, int count, Color c );
        
        #endregion
        
        
        INTERNAL_Delegate_ClearScene DelFunc_ClearScene;
        INTERNAL_Delegate_DrawLine DelFunc_DrawLine;
        INTERNAL_Delegate_DrawLines DelFunc_DrawLines;
        INTERNAL_Delegate_DrawRect DelFunc_DrawRect;
        INTERNAL_Delegate_DrawRects DelFunc_DrawRects;
        
        
        #region Public Render Primitives
        
        public void DrawLine( SDL.SDL_Point p1, SDL.SDL_Point p2, Color c )
        {
            DelFunc_DrawLine( p1.x, p1.y, p2.x, p2.y, c );
        }
        
        public void DrawLine( int x1, int y1, int x2, int y2, Color c )
        {
            DelFunc_DrawLine( x1, y1, x2, y2, c );
        }
        
        public void DrawLines( SDL.SDL_Point[] points, int count, Color c )
        {
            DelFunc_DrawLines( points, count, c );
        }
        
        public void DrawRect( SDL.SDL_Rect rect, Color c )
        {
            DelFunc_DrawRect( rect, c );
        }
        
        public void DrawRects( SDL.SDL_Rect[] rects, int count, Color c )
        {
            DelFunc_DrawRects( rects, count, c );
        }
        
        #endregion
        
        #region Public Blitters
        
        #region SDL_Surface Blitters
        
        /// <summary>
        /// Blits an SDL_Surface to the SDL_Window by first converting it to an SDL_Texture.
        /// 
        /// NOTE:  The resulting texture is stored in an internal cache, use MarkSurfaceDirty() after modifying an SDL_Surface between blits.
        /// NOTE 2:  The Dictionary access for the cache adds some overhead to the blit but less than the texture creation itself.  This provides a reasonable balance between code flexibility and speed.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcSurface">The SDL_Surface to render</param>
        public unsafe void Blit( SDL.SDL_Rect dstRect, SDL.SDL_Surface* srcSurface )
        {
            var sTex = TextureFromCache( srcSurface );
            SDL.SDL_RenderCopy( _sdlRenderer, sTex, IntPtr.Zero, ref dstRect  );
        }
        
        /// <summary>
        /// Blits an SDL_Surface to the SDL_Window by first converting it to an SDL_Texture.
        /// 
        /// NOTE:  The resulting texture is stored in an internal cache, use MarkSurfaceDirty() after modifying an SDL_Surface between blits.
        /// NOTE 2:  The Dictionary access for the cache adds some overhead to the blit but less than the texture creation itself.  This provides a reasonable balance between code flexibility and speed.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcSurface">The SDL_Surface to render</param>
        /// <param name="srcRect">Region of the SDL_Surface to render from.</param>
        public unsafe void Blit( SDL.SDL_Rect dstRect, SDL.SDL_Surface* srcSurface, SDL.SDL_Rect srcRect )
        {
            var sTex = TextureFromCache( srcSurface );
            SDL.SDL_RenderCopy( _sdlRenderer, sTex, ref srcRect, ref dstRect  );
        }
        
        /// <summary>
        /// Blits an SDL_Surface to the SDL_Window by first converting it to an SDL_Texture.
        /// 
        /// NOTE:  The resulting texture is stored in an internal cache, use MarkSurfaceDirty() after modifying an SDL_Surface between blits.
        /// NOTE 2:  The Dictionary access for the cache adds some overhead to the blit but less than the texture creation itself.  This provides a reasonable balance between code flexibility and speed.
        /// NOTE 3:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcSurface">The SDL_Surface to render</param>
        /// <param name="angle">Angle in degrees to rotate the SDL_Surface.</param>
        public unsafe void Blit( SDL.SDL_Rect dstRect, SDL.SDL_Surface* srcSurface, double angle )
        {
            var sTex = TextureFromCache( srcSurface );
            SDL.SDL_RenderCopyEx( _sdlRenderer, sTex, IntPtr.Zero, ref dstRect, angle, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE );
        }
        
        /// <summary>
        /// Blits an SDL_Surface to the SDL_Window by first converting it to an SDL_Texture.
        /// 
        /// NOTE:  The resulting texture is stored in an internal cache, use MarkSurfaceDirty() after modifying an SDL_Surface between blits.
        /// NOTE 2:  The Dictionary access for the cache adds some overhead to the blit but less than the texture creation itself.  This provides a reasonable balance between code flexibility and speed.
        /// NOTE 3:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcSurface">The SDL_Surface to render</param>
        /// <param name="srcRect">Region of the SDL_Surface to render from.</param>
        /// <param name="angle">Angle in degrees to rotate the SDL_Surface.</param>
        public unsafe void Blit( SDL.SDL_Rect dstRect, SDL.SDL_Surface* srcSurface, SDL.SDL_Rect srcRect, double angle )
        {
            var sTex = TextureFromCache( srcSurface );
            SDL.SDL_RenderCopyEx( _sdlRenderer, sTex, ref srcRect, ref dstRect, angle, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE );
        }
        
        #endregion
        
        #region SDL_Texture Blitters
        
        /// <summary>
        /// Blits an SDL_Texture to the SDL_Window.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcTexture">The SDL_Texture to render</param>
        public void Blit( SDL.SDL_Rect dstRect, IntPtr srcTexture )
        {
            SDL.SDL_RenderCopy( _sdlRenderer, srcTexture, IntPtr.Zero, ref dstRect  );
        }
        
        /// <summary>
        /// Blits an SDL_Texture to the SDL_Window.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcTexture">The SDL_Texture to render</param>
        /// <param name="srcRect">Region of the SDL_Texture to render from.</param>
        public void Blit( SDL.SDL_Rect dstRect, IntPtr srcTexture, SDL.SDL_Rect srcRect )
        {
            SDL.SDL_RenderCopy( _sdlRenderer, srcTexture, ref srcRect, ref dstRect  );
        }
        
        /// <summary>
        /// Blits an SDL_Texture to the SDL_Window.
        /// 
        /// NOTE:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcTexture">The SDL_Texture to render</param>
        /// <param name="angle">Angle in degrees to rotate the SDL_Texture.</param>
        public void Blit( SDL.SDL_Rect dstRect, IntPtr srcTexture, double angle )
        {
            SDL.SDL_RenderCopyEx( _sdlRenderer, srcTexture, IntPtr.Zero, ref dstRect, angle, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE );
        }
        
        /// <summary>
        /// Blits an SDL_Texture to the SDL_Window.
        /// 
        /// NOTE:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="srcTexture">The SDL_Texture to render</param>
        /// <param name="srcRect">Region of the SDL_Texture to render from.</param>
        /// <param name="angle">Angle in degrees to rotate the SDL_Surface.</param>
        public void Blit( SDL.SDL_Rect dstRect, IntPtr srcTexture, SDL.SDL_Rect srcRect, double angle )
        {
            SDL.SDL_RenderCopyEx( _sdlRenderer, srcTexture, ref srcRect, ref dstRect, angle, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE );
        }
        
        #endregion
        
        #endregion
        
        #region Internal Render Primitives
        
        // These all come in two flavours:
        // A fast version and a version that preserves the SDL state machine
        // (Which at this point is basically the render draw color)
        
        #region ClearScene
        
        void INTERNAL_DelFunc_ClearScene_Fast()
        {
            INTERNAL_RenderColor = _clearColor;
            SDL.SDL_RenderClear( _sdlRenderer );
        }
        
        void INTERNAL_DelFunc_ClearScene()
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_ClearScene_Fast();
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #region DrawLine
        
        void INTERNAL_DelFunc_DrawLine_Fast( int x1, int y1, int x2, int y2, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderDrawLine( _sdlRenderer, x1, y1, x2, y2 );
        }
        
        void INTERNAL_DelFunc_DrawLine( int x1, int y1, int x2, int y2, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawLine_Fast( x1, y1, x2, y2, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #region DrawLines
        
        void INTERNAL_DelFunc_DrawLines_Fast( SDL.SDL_Point[] points, int count, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderDrawLines( _sdlRenderer, points, count );
        }
        
        void INTERNAL_DelFunc_DrawLines( SDL.SDL_Point[] points, int count, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawLines_Fast( points, count, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #region DrawRect
        
        void INTERNAL_DelFunc_DrawRect_Fast( SDL.SDL_Rect rect, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderDrawRect( _sdlRenderer, ref rect );
        }
        
        void INTERNAL_DelFunc_DrawRect( SDL.SDL_Rect rect, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawRect_Fast( rect, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #region DrawRects
        
        void INTERNAL_DelFunc_DrawRects_Fast( SDL.SDL_Rect[] rects, int count, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderDrawRects( _sdlRenderer, rects, count );
        }
        
        void INTERNAL_DelFunc_DrawRects( SDL.SDL_Rect[] rects, int count, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawRects_Fast( rects, count, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #endregion
        
    }
    
}
