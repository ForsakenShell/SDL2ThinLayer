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
        delegate void INTERNAL_Delegate_DrawFilledRect( SDL.SDL_Rect rect, Color c );
        delegate void INTERNAL_Delegate_DrawFilledRects( SDL.SDL_Rect[] rects, int count, Color c );
        
        #endregion
        
        
        INTERNAL_Delegate_ClearScene DelFunc_ClearScene;
        INTERNAL_Delegate_DrawLine DelFunc_DrawLine;
        INTERNAL_Delegate_DrawLines DelFunc_DrawLines;
        INTERNAL_Delegate_DrawRect DelFunc_DrawRect;
        INTERNAL_Delegate_DrawRects DelFunc_DrawRects;
        INTERNAL_Delegate_DrawRect DelFunc_DrawFilledRect;
        INTERNAL_Delegate_DrawRects DelFunc_DrawFilledRects;
        
        
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
        
        public void DrawFilledRect( SDL.SDL_Rect rect, Color c )
        {
            DelFunc_DrawFilledRect( rect, c );
        }
        
        public void DrawFilledRects( SDL.SDL_Rect[] rects, int count, Color c )
        {
            DelFunc_DrawFilledRects( rects, count, c );
        }
        
        #endregion
        
        #region Public Blitters
        
        #region Surface Blitters
        
        /// <summary>
        /// Blits a Surface to the SDL_Window.
        /// 
        /// NOTE:  A Texture will be created from the Surface.  Use Surface.DeleteTexture() after modifying a Surface between blits.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="surface">The Surface to render</param>
        public void Blit( SDL.SDL_Rect dstRect, Surface surface )
        {
            Blit( dstRect, surface.Texture );
        }
        
        /// <summary>
        /// Blits a Surface to the SDL_Window.
        /// 
        /// NOTE:  A Texture will be created from the Surface.  Use Surface.DeleteTexture() after modifying a Surface between blits.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="surface">The Surface to render</param>
        /// <param name="srcRect">Region of the Surface to render from.</param>
        public void Blit( SDL.SDL_Rect dstRect, Surface surface, SDL.SDL_Rect srcRect )
        {
            Blit( dstRect, surface.Texture, srcRect );
        }
        
        /// <summary>
        /// Blits a Surface to the SDL_Window.
        /// 
        /// NOTE:  A Texture will be created from the Surface.  Use Surface.DeleteTexture() after modifying a Surface between blits.
        /// NOTE 2:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="surface">The Surface to render</param>
        /// <param name="angle">Angle in degrees to rotate the Surface.</param>
        public void Blit( SDL.SDL_Rect dstRect, Surface surface, double angle )
        {
            Blit( dstRect, surface.Texture, angle );
        }
        
        /// <summary>
        /// Blits a Surface to the SDL_Window.
        /// 
        /// NOTE:  A Texture will be created from the Surface.  Use Surface.DeleteTexture() after modifying a Surface between blits.
        /// NOTE 2:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="surface">The Surface to render</param>
        /// <param name="srcRect">Region of the Surface to render from.</param>
        /// <param name="angle">Angle in degrees to rotate the Surface.</param>
        public void Blit( SDL.SDL_Rect dstRect, Surface surface, SDL.SDL_Rect srcRect, double angle )
        {
            Blit( dstRect, surface.Texture, srcRect, angle );
        }
        
        #endregion
        
        #region Texture Blitters
        
        /// <summary>
        /// Blits a Texture to the SDL_Window.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="texture">The exture to render</param>
        public void Blit( SDL.SDL_Rect dstRect, Texture texture )
        {
            if( texture == null ) return;
            SDL.SDL_RenderCopy( _sdlRenderer, texture.SDLTexture, IntPtr.Zero, ref dstRect  );
        }
        
        /// <summary>
        /// Blits a Texture to the SDL_Window.
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="texture">The Texture to render</param>
        /// <param name="srcRect">Region of the Texture to render from.</param>
        public void Blit( SDL.SDL_Rect dstRect, Texture texture, SDL.SDL_Rect srcRect )
        {
            if( texture == null ) return;
            SDL.SDL_RenderCopy( _sdlRenderer, texture.SDLTexture, ref srcRect, ref dstRect  );
        }
        
        /// <summary>
        /// Blits a Texture to the SDL_Window.
        /// 
        /// NOTE:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="texture">The Texture to render</param>
        /// <param name="angle">Angle in degrees to rotate the Texture.</param>
        public void Blit( SDL.SDL_Rect dstRect, Texture texture, double angle )
        {
            if( texture == null ) return;
            SDL.SDL_RenderCopyEx( _sdlRenderer, texture.SDLTexture, IntPtr.Zero, ref dstRect, angle, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE );
        }
        
        /// <summary>
        /// Blits a Texture to the SDL_Window.
        /// 
        /// NOTE:  Rotation is clockwise as per SDL.SDL_RenderCopyEx().
        /// </summary>
        /// <param name="dstRect">Position and size on the SDL_Window to render to.</param>
        /// <param name="texture">The Texture to render</param>
        /// <param name="srcRect">Region of the Texture to render from.</param>
        /// <param name="angle">Angle in degrees to rotate the Surface.</param>
        public void Blit( SDL.SDL_Rect dstRect, Texture texture, SDL.SDL_Rect srcRect, double angle )
        {
            if( texture == null ) return;
            SDL.SDL_RenderCopyEx( _sdlRenderer, texture.SDLTexture, ref srcRect, ref dstRect, angle, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE );
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
        
        #region DrawFilledRect
        
        void INTERNAL_DelFunc_DrawFilledRect_Fast( SDL.SDL_Rect rect, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderFillRect( _sdlRenderer, ref rect );
        }
        
        void INTERNAL_DelFunc_DrawFilledRect( SDL.SDL_Rect rect, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawFilledRect_Fast( rect, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #region DrawFilledRects
        
        void INTERNAL_DelFunc_DrawFilledRects_Fast( SDL.SDL_Rect[] rects, int count, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderFillRects( _sdlRenderer, rects, count );
        }
        
        void INTERNAL_DelFunc_DrawFilledRects( SDL.SDL_Rect[] rects, int count, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawFilledRects_Fast( rects, count, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #endregion
        
    }
    
}
