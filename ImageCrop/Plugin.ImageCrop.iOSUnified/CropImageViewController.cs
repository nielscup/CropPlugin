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
        CropperOverlayView overlay;
        bool dragCropper = false;
        bool resizeCropper = false;
        UIImage pic;
        CropperResizerView resizer;

        nfloat resizerSize = 40;
        UIColor cropperColor = UIColor.Red;
        
        nfloat previewImageSize = 100;
        int cropperStartSize = 200;
        int cropperMinWidth = 100;
        int cropperMinHeight = 100;
        double maxResizeFactor = 1;
        double cropperAspectRatio = 1;
        nfloat cropperTransparency = 0.6f;
        nfloat cropperLineWidth = 3;
        nfloat marginY = 60;
        nfloat marginX = 0;

        nfloat cropperWidth;
        nfloat cropperHeight;
        nfloat cropperX;
        nfloat cropperY;
        
        public event EventHandler OnSaved;

        string _picturePath;
        string _croppedPicturePath;
        int _croppedImageWidth;
        int _croppedImageHeight;

        public CropImageViewController(string path, string croppedPath, int croppedImageWidth = 0, int croppedImageHeight = 0)
        {
            _picturePath = path;
            _croppedPicturePath = croppedPath;
            _croppedImageWidth = croppedImageWidth;
            _croppedImageHeight = croppedImageHeight;

            if (_croppedImageWidth != 0 && _croppedImageHeight != 0)
            {
                SetCropperAspectRatio((double)_croppedImageWidth, (double)_croppedImageHeight);                
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            pic = new UIImage(_picturePath);
            var maxScaledPictureSize = UIScreen.MainScreen.Bounds.Size;
            maxScaledPictureSize.Width -= marginX;
            maxScaledPictureSize.Height -= (marginY + 100);

            var scaledPicture = ResizeImage(pic, (float)maxScaledPictureSize.Width, (float)maxScaledPictureSize.Height);
            picture = new UIImageView(scaledPicture);
            //marginY = (UIScreen.MainScreen.Bounds.Size.Height - picture.Frame.Height + 10) / 2;
            picture.Frame = new CGRect(marginX, marginY, (int)scaledPicture.Size.Width, (int)scaledPicture.Size.Height);

            Add(picture);
            
            resizer = new CropperResizerView(cropperColor, cropperTransparency, cropperLineWidth);
            Add(resizer);
                           
            SetCropper();

            var cancelButton = AddButton("Cancel", 150f, 20f, UIControlContentHorizontalAlignment.Left);
            cancelButton.TouchUpInside += cancelButton_TouchUpInside;

            var saveButton = AddButton("Save", 150f, 20f, UIControlContentHorizontalAlignment.Right);
            saveButton.TouchUpInside += saveButton_TouchUpInside;                        
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

        private UIButton AddButton(string title, nfloat width, nfloat margin, UIControlContentHorizontalAlignment contentHorizontalAlignment)
        {
            var x = margin;
            if (contentHorizontalAlignment == UIControlContentHorizontalAlignment.Right)
                x = PictureX - (width + margin);

            var button = new UIButton(new CGRect(x, UIScreen.MainScreen.Bounds.Size.Height - 60, width, 40f));
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.White, UIControlState.Normal);
            button.HorizontalAlignment = contentHorizontalAlignment;            
            Add(button);

            return button;
        }

        void cancelButton_TouchUpInside(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }
                                    
        void saveButton_TouchUpInside(object sender, EventArgs e)
        {
            var resizedImage = previewImage.Image;
            
            if(_croppedImageHeight > 0 || _croppedImageWidth > 0)
                resizedImage = ResizeImage(previewImage.Image, (float)_croppedImageWidth, (float)_croppedImageHeight);
            
            NSData imgData = resizedImage.AsJPEG();
            NSError err = null;
            if (imgData.Save(_croppedPicturePath, false, out err))
            {
                Console.WriteLine("saved as " + _croppedPicturePath);
                
                DismissViewController(true, null);
                var obj = new NSDictionary();
                
                if (OnSaved != null)
                    OnSaved(this, null);
            }
            else
            {
                Console.WriteLine("NOT saved as " + _croppedPicturePath + " because" + err.LocalizedDescription);
            }
        }

        private void SetCropperMinSize()
        {
            //if (_croppedImageHeight > 0 || _croppedImageWidth > 0)
            //{
            //    cropperMinWidth = (int)(_croppedImageWidth * maxResizeFactor);
            //    cropperMinHeight = (int)(_croppedImageHeight * maxResizeFactor);
            //}
        }

        private void SetPreviewImage()
        {
            if (previewImage == null)
            {
                previewImage = new UIImageView(new CGRect(10, PictureY - previewImageSize / 2, previewImageSize, previewImageSize / cropperAspectRatio));
                AddShadow(ref previewImage);
                Add(previewImage);
            }

            // set preview image size
            var width = previewImageSize;
            var height = width / cropperAspectRatio;

            // restrict preview image size
            if(height > previewImageSize)
            {
                height = previewImageSize;
                width = previewImageSize * (nfloat)cropperAspectRatio;
            }

            previewImage.Frame = new CGRect(10, 10 + PictureY - height, width, height);

            previewImage.Image = CropImage(pic,
                (float)(cropper.Frame.X / maxResizeFactor - marginX / maxResizeFactor),
                (float)(cropper.Frame.Y / maxResizeFactor - marginY / maxResizeFactor), 
                (float)(cropper.Frame.Width / maxResizeFactor),
                (float)(cropper.Frame.Height / maxResizeFactor));           
        }



        /// <summary>
        /// Sets the resizer triangle to resize the cropper.
        /// </summary>
        private void SetResizer()
        {
            resizer.Frame = new CGRect(
                (nfloat)(cropper.Frame.X + cropper.Frame.Width - resizerSize - cropperLineWidth),
                (nfloat)(cropper.Frame.Y + cropper.Frame.Height - resizerSize - cropperLineWidth),
                resizerSize,
                resizerSize);
        }

        /// <summary>
        /// Sets the cropper rectangle
        /// </summary>
        private void SetCropper()
        {
            PointF centerCropperLocation = new PointF(
                (float)(PictureX / 2 - cropperStartSize / 2), 
                (float)((PictureY / 2 - cropperStartSize / 2)));

            var restrictedSize = RestrictCropperSize((nfloat)cropperStartSize, (nfloat)(cropperStartSize / cropperAspectRatio), centerCropperLocation.X, centerCropperLocation.Y);
            SizeF size = new SizeF((float)restrictedSize.Width, (float)restrictedSize.Height);

            cropper = new CropperView(centerCropperLocation, size, cropperColor, cropperTransparency, cropperLineWidth);
                        
            // enable Pinch
            cropper.MultipleTouchEnabled = true;
            var pinchRecognizer = new UIPinchGestureRecognizer(HandlePinchGesture);
            HandlePinchGesture(pinchRecognizer);
            cropper.AddGestureRecognizer(pinchRecognizer);

            SetOverLay();
            Add(cropper);

            SetResizer();
            SetPreviewImage();
        }

        /// <summary>
        /// Sets the overlay
        /// </summary>
        private void SetOverLay()
        {
            if (overlay == null)
            {
                overlay = new CropperOverlayView(cropper.Frame, (float)PictureX, (float)PictureY);
                Add(overlay);
            }
            else
            {
                overlay.Redraw(cropper.Frame);
                overlay.SetNeedsDisplay();
            }
        }
                
        public void HandlePinchGesture(UIPinchGestureRecognizer sender)
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
                    var point = RestrictCropperPosition(cropperX - ((cropperWidth * scale - cropperWidth) / 2), cropperY - ((cropperHeight * scale - cropperHeight) / 2));
                    var size = RestrictCropperSize(cropperWidth * scale, cropperHeight * scale, cropper.Frame.X, cropper.Frame.Y);
                    cropper.Frame = new CGRect(point, size);
                    cropper.SetNeedsDisplay();
                    SetResizer();
                    SetOverLay();
                    //SetPreviewImage();
                    break;
                case UIGestureRecognizerState.Ended:
                    SetPreviewImage();
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

        /// <summary>
        /// Resize the image to be contained within a maximum width and height, keeping aspect ratio
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public UIImage ResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
        {
            var sourceSize = sourceImage.Size;
            maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
            SetCropperMinSize();
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

                cropper.Frame = GetCropperRect(cropper.Frame.Size.Width, cropper.Frame.Size.Height, x, y);
                cropper.SetNeedsDisplay();

                //SetPreviewImage();
                SetResizer();
                SetOverLay();
            }
            else if (resizeCropper)
            {
                nfloat width = cropper.Frame.Size.Width - offsetX;
                nfloat height = cropper.Frame.Size.Height - offsetY;
                if (_croppedImageHeight != 0 && _croppedImageWidth != 0)
                {
                    var offset = (offsetX + offsetY) / 2;

                    width = cropper.Frame.Size.Width - offset;
                    height = width / (nfloat)cropperAspectRatio;
                }
                cropper.Frame = GetCropperRect(width, height, cropper.Frame.X, cropper.Frame.Y);

                // make sure to redraw the cropper, otherwise the outline will get narrower and thicker when resizing
                cropper.SetNeedsDisplay();
                                
                //SetPreviewImage();
                SetResizer();
                SetOverLay();

            }
        }

        private CGRect GetCropperRect(nfloat width, nfloat height, nfloat x, nfloat y)
        {
            var position = RestrictCropperPosition(x, y);
            var size = RestrictCropperSize(width, height, position.X, position.Y);

            SetCropperAspectRatio(size.Width, size.Height);            
            return new CGRect(position, size);
        }

        private void SetCropperAspectRatio(double width, double height)
        {
            if (_croppedImageWidth == 0 || _croppedImageHeight == 0)
                cropperAspectRatio = 1;

            cropperAspectRatio = width / height;
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

        private CGSize RestrictCropperSize(nfloat width, nfloat height, nfloat x, nfloat y)
        {            
            // restrict cropper size to stay within picture when resizing: prevent resizing bigger than picture
            if (width + x > PictureX)
            {
                width -= (width + x - PictureX);
                height = width / (nfloat)cropperAspectRatio;
            }

            if (height + y > PictureY)
            {
                height -= (height + y - PictureY);
                width = height * (nfloat)cropperAspectRatio;
            }

            // restrict to minimum width
            if (width < cropperMinWidth)
            {
                width = (nfloat)cropperMinWidth;
                height = width / (nfloat)cropperAspectRatio;
            }

            // restrict to minimum height
            if (height < cropperMinHeight)
            {
                height = (nfloat)cropperMinHeight;
                width = height * (nfloat)cropperAspectRatio;
            }

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
