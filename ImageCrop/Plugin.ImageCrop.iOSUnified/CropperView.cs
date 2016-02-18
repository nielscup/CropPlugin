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
        private CGPath _path;
        private PointF _targetLocation;
        private SizeF _size;
        private UIColor _color;
        private nfloat _transparancy;
        private nfloat _lineWidth;

        internal CropperView(PointF targetLocation, SizeF size, UIColor color = null, nfloat transparancy = default(nfloat), nfloat lineWidth = default(nfloat))
        {
            _targetLocation = targetLocation;
            _size = size;
            _color = color ?? UIColor.White;
            _transparancy = transparancy == 0 ? 0.8f : transparancy;
            _lineWidth = lineWidth == 0 ? 3f : lineWidth;
            
            RectangleF frameRect = new RectangleF(_targetLocation.X, _targetLocation.Y, _size.Width, _size.Height);
            this.Frame = frameRect;
            this.BackgroundColor = UIColor.Clear;
        }

        internal void Reset(CGRect frame)
        {
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

                //create geometry
                _path = new CGPath();
                _path.AddRect(new RectangleF(0f, 0f, (float)rect.Size.Width, (float)rect.Size.Height));
                _path.CloseSubpath();

                g.SetStrokeColor(_color.CGColor);
                g.SetAlpha(_transparancy);

                //add geometry to graphics context and draw it
                g.AddPath(_path);
                g.DrawPath(CGPathDrawingMode.FillStroke);

                //// Draw overlay circle
                //var pathStatus = new CGPath();
                ////_frontColor.SetStroke();
                //pathStatus.AddArc(rect.Size.Width / 2, rect.Size.Height / 2, rect.Size.Width / 2, 0, 360 * (float)Math.PI, false);
                //g.AddPath(pathStatus);
                //g.DrawPath(CGPathDrawingMode.Stroke);
            }
        }
    }
}
