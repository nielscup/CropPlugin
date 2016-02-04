using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UIKit;

namespace Plugin.ImageCrop
{    
    public class CropImageViewController : UIViewController
    {        
        UIImageView picture;
        UIImageView previewImage;
        CropperView cropper;
        bool dragCropper = false;
        bool resizeCropper = false;
        UIImage pic;
        CropperResizerView resizer;

        nfloat resizerSize = 40;
        UIColor cropperColor = UIColor.Red;
        int cropperSize = 200;
        nfloat previewImageSize = 100;
        int cropperMinSize = 60;
        double maxResizeFactor = 1;
        nfloat cropperTransparency = 0.6f;
        nfloat cropperLineWidth = 5;
        nfloat marginY = 60;
        nfloat marginX = 0;
        
        public event EventHandler OnSaved;

        string picturePath;
        int maxReturnImageWidth;
        int maxReturnImageHeight;

        public CropImageViewController(string path, int maxReturnImageSize)
        {
            picturePath = path;
            maxReturnImageWidth = maxReturnImageSize;
            maxReturnImageHeight = maxReturnImageSize;

            //this.View.MultipleTouchEnabled = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            pic = new UIImage(picturePath);
            var maxScaledPictureSize = UIScreen.MainScreen.Bounds.Size;
            maxScaledPictureSize.Width -= marginX;
            maxScaledPictureSize.Height -= (marginY + 100);

            var scaledPicture = MaxResizeImage(pic, (float)maxScaledPictureSize.Width, (float)maxScaledPictureSize.Height);
            picture = new UIImageView(scaledPicture);             
            picture.Frame = new CGRect(marginX, marginY, (int)scaledPicture.Size.Width, (int)scaledPicture.Size.Height);

            Add(picture);

            SetCropper();

            previewImage = new UIImageView(new CGRect(10, PictureY - previewImageSize / 2, previewImageSize, previewImageSize)); //AddImage((int)cropSizeF.Height, (int)cropSizeF.Width);
            AddShadow(ref previewImage);
            Add(previewImage);

            resizer = new CropperResizerView(cropperColor, cropperTransparency, cropperLineWidth);
            Add(resizer);

            SetPreviewImage();
            SetResizer();

            var saveButton = new UIButton(new CGRect(PictureX - 210, PictureY + 10, 200f, 40f)); //AddImage((int)cropSizeF.Height, (int)cropSizeF.Width);
            saveButton.SetTitle("Save", UIControlState.Normal);
            saveButton.SetTitleColor(new UIColor(1, 0, 0, 1), UIControlState.Normal);
            saveButton.TouchUpInside += (s, e) =>
            {
                Save();
            };

            Add(saveButton);
        }
                    
        public void Save()
        {
            var resizedImage = previewImage.Image;
            
            if(maxReturnImageHeight > 0 || maxReturnImageWidth > 0)
                resizedImage = MaxResizeImage(previewImage.Image, (float)maxReturnImageWidth, (float)maxReturnImageHeight);
            
            NSData imgData = resizedImage.AsJPEG();
            NSError err = null;
            if (imgData.Save(picturePath, false, out err))
            {
                Console.WriteLine("saved as " + picturePath);
                
                DismissViewController(true, null);
                var obj = new NSDictionary();
                
                if (OnSaved != null)
                    OnSaved(this, null);
            }
            else
            {
                Console.WriteLine("NOT saved as " + picturePath + " because" + err.LocalizedDescription);
            }
        }

        private void SetPreviewImage()
        {
            previewImage.Image = CropImage(pic,
                (float)(cropper.Frame.X / maxResizeFactor - marginX / maxResizeFactor), 
                (float)(cropper.Frame.Y / maxResizeFactor - marginY / maxResizeFactor), 
                (float)(cropper.Frame.Width / maxResizeFactor), 
                (float)(cropper.Frame.Height / maxResizeFactor));           
        }

        private void SetResizer()
        {
            resizer.Frame = new CGRect(
                (nfloat)(cropper.Frame.X + cropper.Frame.Width - resizerSize - cropperLineWidth),
                (nfloat)(cropper.Frame.Y + cropper.Frame.Height - resizerSize - cropperLineWidth),
                resizerSize,
                resizerSize);
        }

        private void SetCropper()
        {
            PointF startPoint = new PointF((float)(PictureX / 2 - cropperSize / 2), (float)(PictureY / 2 - cropperSize / 2));
            SizeF size = new SizeF(cropperSize, cropperSize);

            cropper = new CropperView(startPoint, size, cropperColor, cropperTransparency, cropperLineWidth);
            
            // enable Pinch
            cropper.MultipleTouchEnabled = true;
            var pinchRecognizer = new UIPinchGestureRecognizer(handlePinchGesture);
            handlePinchGesture(pinchRecognizer);
            cropper.AddGestureRecognizer(pinchRecognizer);

            Add(cropper);
        }

        nfloat cropperWidth;
        nfloat cropperHeight;
        nfloat cropperX;
        nfloat cropperY;
        public void handlePinchGesture(UIPinchGestureRecognizer sender)
        {
            resizeCropper = false;
            dragCropper = false;
            var scale = sender.Scale;
            switch (sender.State)
            {
                case UIGestureRecognizerState.Began:
                    cropperWidth = cropper.Frame.Width;
                    cropperHeight = cropper.Frame.Height;
                    cropperX = cropper.Frame.X;
                    cropperY = cropper.Frame.Y;
                    break;
                case UIGestureRecognizerState.Changed:
                    //cropper.Frame = new CGRect(cropperX - ((cropperWidth * scale - cropperWidth) / 2), cropperY - ((cropperHeight * scale - cropperHeight) / 2), cropperWidth * scale, cropperHeight * scale);
                    var point = RestrictCropperPosition(cropperX - ((cropperWidth * scale - cropperWidth) / 2), cropperY - ((cropperHeight * scale - cropperHeight) / 2));
                    var size = RestrictCropperSize(cropperWidth * scale, cropperHeight * scale);
                    cropper.Frame = new CGRect(point, size);
                    cropper.SetNeedsDisplay();
                    SetResizer();
                    //SetPreviewImage();
                    break;
                case UIGestureRecognizerState.Ended:
                    SetPreviewImage();
                //pinchLayout.setPinchedCellScale((float)sender.Scale);
                    break;
            }
        }

        public void AddShadow(ref UIImageView view, int shadow = 3)
        {
            view.Layer.CornerRadius = (nfloat)shadow;
            view.Layer.ShadowColor = UIColor.Black.CGColor;
            view.Layer.ShadowOpacity = 0.6f;
            view.Layer.ShadowRadius = (nfloat)shadow;
            view.Layer.ShadowOffset = new System.Drawing.SizeF((float)shadow, (float)shadow);
            view.BackgroundColor = UIColor.White;
        }

        // resize the image to be contained within a maximum width and height, keeping aspect ratio
        public UIImage MaxResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            var sourceSize = sourceImage.Size;
            maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
            if (maxResizeFactor > 1) return sourceImage;
            float width = (float)maxResizeFactor * (float)sourceSize.Width;
            float height = (float)maxResizeFactor * (float)sourceSize.Height;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            sourceImage.Draw(new RectangleF(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }
        
        // crop the image, without resizing
        private UIImage CropImage(UIImage sourceImage, float crop_x, float crop_y, float width, float height)
        {
            var imgSize = sourceImage.Size;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            var context = UIGraphics.GetCurrentContext();
            var clippedRect = new RectangleF(0, 0, width, height);
            context.ClipToRect(clippedRect);
            var drawRect = new RectangleF(-crop_x, -crop_y, (float)imgSize.Width, (float)imgSize.Height);
            sourceImage.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return modifiedImage;
        }

        #region touch
        //https://developer.xamarin.com/guides/cross-platform/application_fundamentals/touch/part_2_ios_touch_walkthrough/

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            // Get the current touch
            UITouch touch = touches.AnyObject as UITouch;
            if (touch == null) return;
            
            if (cropper.Frame.Contains(touch.LocationInView(View)))
            {
                // draggableRectangle touched, prepare to drag
                dragCropper = true;
            }

            if (resizer.Frame.Contains(touch.LocationInView(View)))
            {
                // draggableRectangle touched, prepare to drag
                dragCropper = false;
                resizeCropper = true;
            }            
        }
                
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            dragCropper = false;
            resizeCropper = false;
            SetPreviewImage();
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            // get the touch
            UITouch touch = touches.AnyObject as UITouch;
            
            if (touch == null)
                return;
      
            nfloat offsetX = touch.PreviousLocationInView(View).X - touch.LocationInView(View).X;
            nfloat offsetY = touch.PreviousLocationInView(View).Y - touch.LocationInView(View).Y;

            if (dragCropper)
            {
                // move the cropperRectangle                
                nfloat x = cropper.Frame.X - offsetX;
                nfloat y = cropper.Frame.Y - offsetY;
                
                cropper.Frame = new CGRect(RestrictCropperPosition(x, y), RestrictCropperSize(cropper.Frame.Size.Width, cropper.Frame.Size.Height));
                cropper.SetNeedsDisplay();

                //SetPreviewImage();
                SetResizer();
            }
            else if (resizeCropper)
            {
                var offset = (offsetX + offsetY) / 2;

                nfloat width = cropper.Frame.Size.Width - offset;
                nfloat height = cropper.Frame.Size.Height - offset;
                
                cropper.Frame = new CGRect(
                    RestrictCropperPosition(cropper.Frame.X, cropper.Frame.Y), 
                    RestrictCropperSize(width, height));

                // make sure to redraw the cropper, otherwise the outline will get narrower and thicker when resizing
                cropper.SetNeedsDisplay();
                                
                //SetPreviewImage();
                SetResizer();
            }
        }

        private CGPoint RestrictCropperPosition(nfloat x, nfloat y)
        {
            // restrict x position (stay within picture)
            if (x > PictureX - cropper.Frame.Width) x = PictureX - cropper.Frame.Width;
            if (x < marginX) x = marginX;

            // restrict y position (stay within picture)
            if (y > PictureY - cropper.Frame.Height) y = PictureY - cropper.Frame.Height;
            if (y < marginY) y = marginY;

            return new CGPoint(x, y);
        }

        private CGSize RestrictCropperSize(nfloat width, nfloat height)
        {
            // restrict to cropper to stay within picture when resizing: prevent resizing bigger than picture
            if (width + cropper.Frame.X > PictureX || height + cropper.Frame.Y > PictureY)
            {
                width -= (width + cropper.Frame.X - PictureX);
                height = width; //(size.Width + cropper.Frame.X - PictureX);
            }

            // restrict to minimum width
            if (width < cropperMinSize)
                width = (nfloat)cropperMinSize;

            // restrict to minimum height
            if (height < cropperMinSize)
                height = (nfloat)cropperMinSize;

            return new CGSize(width, height);
        }

        private nfloat PictureY
        {
            get { return picture.Frame.Height + marginY; }
        }

        private nfloat PictureX
        {
            get { return picture.Frame.Width + marginX; }
        }

        #endregion
    }        
}
