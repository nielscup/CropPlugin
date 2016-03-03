using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UIKit;

namespace Plugin.ImageCrop
{
    internal class CropperOverlayView : UIView
    {
        private CGPath _overlay;
        private float _width;
        private float _height;
        private CGRect _rect;
        bool _isRound;

        internal CropperOverlayView(CGRect rect, float width, float height, bool isRound)
        {
            _rect = rect;
            _isRound = isRound;
            _width = width;
            _height = height;            
            this.Frame = new RectangleF(0, 0, _width, _height);
            this.BackgroundColor = UIColor.Clear;
        }

        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                base.Frame = value;
                _width = (float)base.Frame.Width;
                _height = (float)base.Frame.Height;
            }
        }

        internal void Redraw(CGRect rect, bool isRound)
        {
            _isRound = isRound;
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
                
                _overlay.AddRect(new RectangleF(0f, 0f, _width, _height));
                
                if(_isRound)
                    _overlay.AddEllipseInRect(new RectangleF((float)_rect.X, (float)_rect.Y, (float)_rect.Width, (float)_rect.Height));
                else
                    _overlay.AddRect(new RectangleF((float)_rect.X, (float)_rect.Y, (float)_rect.Width, (float)_rect.Height));

                //_overlay.AddArc(20, 20, 10, 0, 360, false);
                //g.EOClip();
                

                //// top
                //_overlay.AddRect(new RectangleF(0f, 0f, _width, (float)_rect.Y));
                //// left
                //_overlay.AddRect(new RectangleF(0f, (float)_rect.Y, (float)_rect.X, (float)_rect.Height));
                //// right
                //_overlay.AddRect(new RectangleF((float)_rect.X + (float)_rect.Width, (float)_rect.Y, _width - ((float)_rect.X + (float)_rect.Width), (float)_rect.Height));
                //// bottom
                //_overlay.AddRect(new RectangleF(0f, (float)_rect.Y + (float)_rect.Height, _width, _height - ((float)_rect.Y + (float)_rect.Height)));

                g.SetStrokeColor(UIColor.Clear.CGColor);
                g.SetAlpha(0.6f);

                //add geometry to graphics context and draw it
                g.AddPath(_overlay);
                g.DrawPath(CGPathDrawingMode.EOFillStroke);                
            }
        }
    }
}
