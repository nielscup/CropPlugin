using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Plugin.ImageCrop
{
    internal class CropperResizerView : UIView
    {
        private UIColor _color;
        private nfloat _transparancy;
        private nfloat _lineWidth;

        internal CropperResizerView(UIColor color = null, nfloat transparancy = default(nfloat), nfloat lineWidth = default(nfloat))
        {
            this.BackgroundColor = UIColor.Clear;
            _color = color ?? UIColor.Red;
            _transparancy = transparancy == 0 ? 0.8f : transparancy;
            _lineWidth = lineWidth == 0 ? 3f : lineWidth;
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            this.BackgroundColor = UIColor.Clear;

            //get graphics context
            using (CGContext g = UIGraphics.GetCurrentContext())
            {

                //set up drawing attributes
                g.SetLineWidth(_lineWidth);
                _color.SetFill();
                //_color.SetStroke();
                UIColor.Clear.SetStroke();
                g.SetAlpha(_transparancy);

                //create geometry
                var path = new CGPath();

                path.AddLines(new CGPoint[]{
					new CGPoint (rect.X + rect.Width, rect.Y),
					new CGPoint (rect.X + rect.Width, rect.Y + rect.Height), 
					new CGPoint (rect.X, rect.Y + rect.Height)});

                path.CloseSubpath();

                //add geometry to graphics context and draw it
                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.FillStroke);
            }
        }
    }
}
