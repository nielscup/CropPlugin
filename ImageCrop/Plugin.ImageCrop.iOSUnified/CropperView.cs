using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UIKit;

namespace Plugin.ImageCrop
{
    internal class CropperView : UIView
    {
        private PointF _targetLocation;
        private SizeF _size;
        private UIColor _color;
        private nfloat _transparancy;
        private nfloat _lineWidth;
        bool _isRound;

        internal CropperView(PointF targetLocation, SizeF size, UIColor color, nfloat transparancy, nfloat lineWidth, bool isRound)
        {
            _targetLocation = targetLocation;
            _size = size;
            _color = color ?? UIColor.White;
            _transparancy = transparancy == 0 ? 0.8f : transparancy;
            _lineWidth = lineWidth == 0 ? 3f : lineWidth;
            _isRound = isRound;
            
            RectangleF frameRect = new RectangleF(_targetLocation.X, _targetLocation.Y, _size.Width, _size.Height);
            this.Frame = frameRect;
            this.BackgroundColor = UIColor.Clear;
        }

        internal void Reset(CGRect frame, bool isRound)
        {
            _isRound = isRound;
            this.Frame = frame;
            this.SetNeedsDisplay();
        }
                                
        public override void Draw(CGRect rect)
        {
            //get graphics context
            using (CGContext g = UIGraphics.GetCurrentContext())
            {                
                g.SetLineWidth(_lineWidth);
                //set up drawing attributes
                UIColor.Clear.SetFill();

                if (_isRound)
                {
                    var circle = new CGPath();
                    var circleSize = rect.Size.Width / 2;
                    circle.AddArc(circleSize, circleSize, circleSize - _lineWidth, 0, 360, false);
                    g.AddPath(circle);
                }
                else
                {
                    //create geometry
                    var rectangle = new CGPath();
                    rectangle.AddRect(new RectangleF(0f, 0f, (float)rect.Size.Width, (float)rect.Size.Height));
                    rectangle.CloseSubpath();
                    //add geometry to graphics context and draw it
                    g.AddPath(rectangle);
                }

                g.SetStrokeColor(_color.CGColor);
                g.SetAlpha(_transparancy);
                
                g.DrawPath(CGPathDrawingMode.Stroke);
            }
        }
    }
}
