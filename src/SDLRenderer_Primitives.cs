/*
 * SDLRenderer_Primitives.cs
 *
 * Public and internal methods for drawing primitives.
 *
 * User: 1000101
 * Date: 28/01/2018
 * Time: 3:06 AM
 * 
 */
using System;
using System.Collections.Generic;

using SDL2;

using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace SDL2ThinLayer
{
    public partial class SDLRenderer : IDisposable
    {
        
        #region Internal:  Rendering Delegate Prototypes
        
        delegate void INTERNAL_Delegate_ClearScene();
        
        delegate void INTERNAL_Delegate_DrawPoint( int x, int y, Color c );
        delegate void INTERNAL_Delegate_DrawPoints( SDL.SDL_Point[] points, int count, Color c );
        
        delegate void INTERNAL_Delegate_DrawLine( int x1, int y1, int x2, int y2, Color c );
        delegate void INTERNAL_Delegate_DrawLines( SDL.SDL_Point[] points, int count, Color c );
        
        delegate void INTERNAL_Delegate_DrawRect( SDL.SDL_Rect rect, Color c );
        delegate void INTERNAL_Delegate_DrawRects( SDL.SDL_Rect[] rects, int count, Color c );
        delegate void INTERNAL_Delegate_DrawFilledRect( SDL.SDL_Rect rect, Color c );
        delegate void INTERNAL_Delegate_DrawFilledRects( SDL.SDL_Rect[] rects, int count, Color c );
        
        delegate void INTERNAL_Delegate_DrawCircle( int x, int y, int r, Color c );
        
        #endregion
        
        #region Internal:  Rendering Function Pointers...I mean Method Delegates...
        
        INTERNAL_Delegate_ClearScene DelFunc_ClearScene;
        
        INTERNAL_Delegate_DrawPoint DelFunc_DrawPoint;
        INTERNAL_Delegate_DrawPoints DelFunc_DrawPoints;
        
        INTERNAL_Delegate_DrawLine DelFunc_DrawLine;
        INTERNAL_Delegate_DrawLines DelFunc_DrawLines;
        
        INTERNAL_Delegate_DrawRect DelFunc_DrawRect;
        INTERNAL_Delegate_DrawRects DelFunc_DrawRects;
        INTERNAL_Delegate_DrawRect DelFunc_DrawFilledRect;
        INTERNAL_Delegate_DrawRects DelFunc_DrawFilledRects;
        
        INTERNAL_Delegate_DrawCircle DelFunc_DrawCircle;
        INTERNAL_Delegate_DrawCircle DelFunc_DrawFilledCircle;
        
        #endregion
        
        #region Public API:  Rendering Primitives
        
        #region Points
        
        public void DrawPoint( SDL.SDL_Point p, Color c )
        {
            DelFunc_DrawPoint( p.x, p.y, c );
        }
        
        public void DrawPoint( int x, int y, Color c )
        {
            DelFunc_DrawPoint( x, y, c );
        }
        
        public void DrawPoints( SDL.SDL_Point[] points, int count, Color c )
        {
            DelFunc_DrawPoints( points, count, c );
        }
        
        #endregion
        
        #region Lines
        
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
        
        #endregion
        
        #region Rects
        
        public void DrawRect( int x1, int y1, int x2, int y2, Color c )
        {
            var rect = new SDL.SDL_Rect( x1, y1, x2 - x1, y2 - y1 );
            DelFunc_DrawRect( rect, c );
        }
        
        public void DrawRect( SDL.SDL_Rect rect, Color c )
        {
            DelFunc_DrawRect( rect, c );
        }
        
        public void DrawRects( SDL.SDL_Rect[] rects, int count, Color c )
        {
            DelFunc_DrawRects( rects, count, c );
        }
        
        public void DrawFilledRect( int x1, int y1, int x2, int y2, Color c )
        {
            var rect = new SDL.SDL_Rect( x1, y1, x2 - x1, y2 - y1 );
            DelFunc_DrawFilledRect( rect, c );
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
        
        #region Circles
        
        public void DrawCircle( int x, int y, int r, Color c )
        {
            DelFunc_DrawCircle( x, y, r, c );
        }
        
        public void DrawFilledCircle( int x, int y, int r, Color c )
        {
            DelFunc_DrawFilledCircle( x, y, r, c );
        }
        
        #endregion
        
        #endregion
        
        #region Internal:  Rendering Primitives
        
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
        
        #region DrawPoint
        
        void INTERNAL_DelFunc_DrawPoint_Fast( int x, int y, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderDrawPoint( _sdlRenderer, x, y );
        }
        
        void INTERNAL_DelFunc_DrawPoint( int x, int y, Color c)
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawPoint_Fast( x, y, c);
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #region DrawPoints
        
        void INTERNAL_DelFunc_DrawPoints_Fast( SDL.SDL_Point[] points, int count, Color c )
        {
            INTERNAL_RenderColor = c;
            SDL.SDL_RenderDrawPoints( _sdlRenderer, points, count );
        }
        
        void INTERNAL_DelFunc_DrawPoints( SDL.SDL_Point[] points, int count, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawPoints_Fast( points, count, c);
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
        
        #region DrawCircle
        
        void INTERNAL_DelFunc_DrawCircle_Fast( int x, int y, int r, Color c )
        {
            // Draw a circle by using the midpoint circle algorithm
            // This algo only computes the first 45 degrees of a circle and mirrors all other points
            
            // Expected number of points on circumfrence
            // Use standard 2*PI*r and then round up to the next '8'
            var expected = ( 1 + ( (int)Math.Round( 2d * Math.PI * (double)r ) >> 3 ) ) << 3;
            
            // Actual number of points on circumfrence
            int count = 0;
            
            // Allocate an array for the points
            var points = new SDL.SDL_Point[ expected ];
            
            // Calculcate the points using circle midpoint
            int offsetX = r - 1;
            int offsetY = 0;
            int deltaX = 1;
            int deltaY = 1;
            int r2 = r << 1;
            int correction = deltaX - r2;
            
            while( offsetX >= offsetY )
            {
                #region Add points
                
                points[ count + 0 ] = new SDL.SDL_Point( x - offsetX, y + offsetY );
                points[ count + 1 ] = new SDL.SDL_Point( x - offsetX, y - offsetY );
                
                points[ count + 2 ] = new SDL.SDL_Point( x - offsetY, y + offsetX );
                points[ count + 3 ] = new SDL.SDL_Point( x - offsetY, y - offsetX );
                
                points[ count + 4 ] = new SDL.SDL_Point( x + offsetX, y + offsetY );
                points[ count + 5 ] = new SDL.SDL_Point( x + offsetX, y - offsetY );
                
                points[ count + 6 ] = new SDL.SDL_Point( x + offsetY, y + offsetX );
                points[ count + 7 ] = new SDL.SDL_Point( x + offsetY, y - offsetX );
                
                count += 8;
                
                #endregion
                
                if( correction <= 0 )
                {
                    offsetY++;
                    correction += deltaY;
                    deltaY += 2;
                }
                
                if( correction > 0 )
                {
                    offsetX--;
                    deltaX += 2;
                    correction += deltaX - r2;
                }
            }
            
            INTERNAL_DelFunc_DrawPoints_Fast( points, count, c );
        }
        
        void INTERNAL_DelFunc_DrawCircle( int x, int y, int r, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawCircle_Fast( x, y, r, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #region DrawFIlledCircle
        
        void INTERNAL_DelFunc_DrawFilledCircle_Fast( int x, int y, int r, Color c )
        {
            // Draw a circle by using the midpoint circle algorithm
            // This algo only computes the first 45 degrees of a circle and mirrors all other points
            INTERNAL_RenderColor = c;
            
            // Was this line drawn?
            // This is a "quick" way to make sure we don't redraw a scanline and thus fixing blending issues.
            // However, it does introduce accuracy issues in the final circle if a smaller scanline is drawn first.
            var lineWasDrawn = new bool[ r * 2 ];
            
            // Calculcate the points using circle midpoint
            int offsetX = r - 1;
            int offsetY = 0;
            int deltaX = 1;
            int deltaY = 1;
            int r2 = r << 1;
            int correction = deltaX - r2;
            
            while( offsetX >= offsetY )
            {
                #region Draw some lines for a filled circle
                
                if( !lineWasDrawn[ r - offsetY ] )
                {
                    SDL.SDL_RenderDrawLine( _sdlRenderer,
                                           x - offsetX, y - offsetY,
                                           x + offsetX, y - offsetY );
                    lineWasDrawn[ r - offsetY ] = true;
                }
                
                if( !lineWasDrawn[ r - offsetX ] )
                {
                    SDL.SDL_RenderDrawLine( _sdlRenderer,
                                           x - offsetY, y - offsetX,
                                           x + offsetY, y - offsetX );
                    lineWasDrawn[ r - offsetX ] = true;
                }
                
                if( !lineWasDrawn[ r + offsetY ] )
                {
                    SDL.SDL_RenderDrawLine( _sdlRenderer,
                                           x - offsetX, y + offsetY,
                                           x + offsetX, y + offsetY );
                    lineWasDrawn[ r + offsetY ] = true;
                }
                
                if( !lineWasDrawn[ r + offsetX ] )
                {
                    SDL.SDL_RenderDrawLine( _sdlRenderer,
                                           x - offsetY, y + offsetX,
                                           x + offsetY, y + offsetX );
                    lineWasDrawn[ r + offsetX ] = true;
                }
                
                #endregion
                
                if( correction <= 0 )
                {
                    offsetY++;
                    correction += deltaY;
                    deltaY += 2;
                }
                
                if( correction > 0 )
                {
                    offsetX--;
                    deltaX += 2;
                    correction += deltaX - r2;
                }
            }
        }
        
        void INTERNAL_DelFunc_DrawFilledCircle( int x, int y, int r, Color c )
        {
            var oldColor = INTERNAL_RenderColor;
            INTERNAL_DelFunc_DrawFilledCircle_Fast( x, y, r, c );
            INTERNAL_RenderColor = oldColor;
        }
        
        #endregion
        
        #endregion
        
    }
    
}
