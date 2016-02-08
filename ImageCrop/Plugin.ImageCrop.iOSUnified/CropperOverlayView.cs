using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UIKit;

namespace Plugin.ImageCrop
{
    public class CropperOverlayView : UIView
    {
        private CGPath _overlay;
        private float _width;
        private float _height;
        private float _x;
        private float _y;
        private CGRect _rect;

        public CropperOverlayView(CGRect rect, float width, float height)
        {
            _rect = rect;
            _width = width;
            _height = height;
            this.Frame = new RectangleF(0, 0, width, height);
            //DrawOverlay(rect, width, height);
            this.BackgroundColor = UIColor.Clear;
        }

        public void Redraw(CGRect rect)
        {
            _rect = rect;
        }
        
        public override void Draw(CGRect rect)
        {
            //get graphics context
            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                //set up drawing attributes
                UIColor.Black.SetFill();

                //create geometry
                _overlay = new CGPath();

                // top
                _overlay.AddRect(new RectangleF(0f, 0f, _width, (float)_rect.Y));
                // left
                _overlay.AddRect(new RectangleF(0f, (float)_rect.Y, (float)_rect.X, (float)_rect.Height));
                // right
                _overlay.AddRect(new RectangleF((float)_rect.X + (float)_rect.Width, (float)_rect.Y, _width - ((float)_rect.X + (float)_rect.Width), (float)_rect.Height));
                // bottom
                _overlay.AddRect(new RectangleF(0f, (float)_rect.Y + (float)_rect.Height, _width, _height - ((float)_rect.Y + (float)_rect.Height)));
                
                //_overlay.CloseSubpath();

                g.SetStrokeColor(UIColor.Clear.CGColor);
                g.SetAlpha(0.6f);

                //add geometry to graphics context and draw it
                g.AddPath(_overlay);
                g.DrawPath(CGPathDrawingMode.FillStroke);
            }
        }
    }
}
