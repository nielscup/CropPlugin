using Plugin.ImageCrop.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using CoreGraphics;
using Foundation;
using System.Drawing;
using CoreAnimation;

namespace Plugin.ImageCrop
{
    public class ImageCropView: UIImageView, IImageCropView
    {
        UIImageView previewImage;
        CropperView cropper;
        CropperOverlayView overlay;
        bool dragCropper = false;
        bool resizeCropper = false;
        UIImage pic;
        CropperResizerView resizer;
        nfloat resizerSize = 40;
        UIColor cropperColor = UIColor.White;
        nfloat previewImageSize = 100;
        int cropperStartSize = 200;
        int cropperMinWidth = 100;
        int cropperMinHeight = 100;
        double maxResizeFactor = 1;
        double cropperAspectRatio = 1;
        nfloat cropperTransparency = 0.8f;
        nfloat cropperLineWidth = 3;
        nfloat cropperWidth;
        nfloat cropperHeight;
        nfloat cropperX;
        nfloat cropperY;
        CGRect _frame;

        /// <summary>
        /// Default contructor that initializes an instance of this class with no parameters
        /// </summary>
        public ImageCropView()
        {
            UserInteractionEnabled = true;
        }
        
        #region interface implementation

        string _imagePath;
        int _outputWidth;
        int _outputHeight;
        bool _isRound;
                 
        /// <summary>
        /// The local path to the image to be cropped, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto-cropped.jpg"
        /// </summary>
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                //if (_imagePath == value)
                //    return;

                _imagePath = value;
                Initialize();
            }
        }

        /// <summary>
        /// The width after cropping, set to 0 for any width or height
        /// </summary> 
        public int OutputWidth
        {
            get
            {
                return _outputWidth;
            }
            set
            {
                if (_outputWidth == value)
                    return;

                _outputWidth = value;
                Initialize();
            }
        }

        /// <summary>
        /// The height after cropping, set to 0 for any height or height
        /// </summary>
        public int OutputHeight
        {
            get
            {
                return _outputHeight;
            }
            set
            {
                if (_outputHeight == value)
                    return;

                _outputHeight = value;
                Initialize();
            }
        }

        /// <summary>
        /// Determines wether the cropper is round. A round cropped round image will be saved as a square image, therefor the OuputHeight will be ignored when set to true.
        /// </summary>
        public bool IsRound
        {
            get
            {
                return _isRound;
            }
            set
            {
                if (_isRound == value)
                    return;

                _isRound = value;
                Initialize();
            }
        }

        /// <summary>
        /// Sets the image to be cropped
        /// </summary>
        /// <param name="imagePath">the image path, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto.jpg"</param>
        /// <param name="outputWidth">The width after cropping, leave empty or set to 0 for any width</param>
        /// <param name="outputHeight">The height after cropping, leave empty or set to 0 for any height</param>
        /// <param name="isRound">Determines wether the cropper is round. A round cropped image will be saved as a square image, therefor the OuputHeight will be ignored when set to true.</param>
        public void SetImage(string imagePath, int outputWidth = 0, int outputHeight = 0, bool isRound = false)
        {
            IsRound = isRound;
            OutputWidth = outputWidth;
            OutputHeight = outputHeight;

            if (isRound)
            {
                if (OutputWidth == 0)
                    _outputWidth = cropperMinWidth;

                OutputHeight = OutputWidth;
            }

            ImagePath = imagePath;            
        }

        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="destinationPath">the image destination path after cropping, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto-cropped.jpg"</param>
        public void CropAndSave(string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                return;

            var resizedImage = previewImage.Image;

            if (OutputHeight > 0 || OutputWidth > 0)
                resizedImage = ResizeImage(previewImage.Image, (float)OutputWidth, (float)OutputHeight);

            NSData imgData = resizedImage.AsJPEG();
            NSError err = null;
            if (imgData.Save(destinationPath, false, out err))
            {
                Console.WriteLine("saved as " + destinationPath);
            }
            else
            {
                Console.WriteLine("NOT saved as " + destinationPath + " because" + err.LocalizedDescription);
            }        
        }
        
        # endregion

        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                base.Frame = value;
                _frame = this.Frame;
            }
        }
         
        private void Initialize()
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
                return;

            if (OutputWidth != 0 && OutputHeight != 0)
            {
                SetCropperAspectRatio((double)OutputWidth, (double)OutputHeight);
            }

            pic = new UIImage(ImagePath);

            var scaledPicture = ResizeImage(pic, (float)_frame.Size.Width, (float)_frame.Size.Height);  
            this.Image = scaledPicture;
            base.Frame = new CGRect(_frame.X + (_frame.Width - scaledPicture.Size.Width) / 2, _frame.Y + (_frame.Height - scaledPicture.Size.Height) / 2, (int)scaledPicture.Size.Width, (int)scaledPicture.Size.Height);
                                            
            SetCropper();                        
        }

        /// <summary>
        /// Sets the cropper
        /// </summary>
        private void SetCropper()
        {            
            var restrictedSize = RestrictCropperSize((nfloat)cropperStartSize, (nfloat)(cropperStartSize / cropperAspectRatio), 0, 0);
            SizeF size = new SizeF((float)restrictedSize.Width, (float)restrictedSize.Height);

            PointF centerCropperLocation = new PointF(
                (float)(PictureX / 2 - restrictedSize.Width / 2),
                (float)((PictureY / 2 - restrictedSize.Height / 2)));

            if (cropper == null)
            {
                cropper = new CropperView(centerCropperLocation, size, cropperColor, cropperTransparency, cropperLineWidth, IsRound);

                EnablePinch();
                Add(cropper);
            }
            else
            {
                cropper.Reset(new RectangleF(centerCropperLocation.X, centerCropperLocation.Y, size.Width, size.Height), IsRound);
            }
                        
            SetOverLay();
            overlay.Frame = new CGRect(0,0, base.Frame.Width, base.Frame.Height);

            SetResizer();
            UpdateImage();
        }      

        private void EnablePinch()
        {
            this.MultipleTouchEnabled = true;
            var pinchRecognizer = new UIPinchGestureRecognizer(HandlePinchGesture);
            HandlePinchGesture(pinchRecognizer);
            this.AddGestureRecognizer(pinchRecognizer);
        }
        
        private void SetCropperMinSize()
        {
            if (OutputHeight > 0 || OutputWidth > 0)
            {
                cropperMinWidth = Math.Max((int)(OutputWidth * maxResizeFactor), cropperMinWidth);
                cropperMinHeight = Math.Max((int)(OutputHeight * maxResizeFactor), cropperMinHeight);
            }
        }

        private void UpdateImage()
        {
            SetPreviewImage();
        }

        private void SetPreviewImage()
        {
            return;

            if (cropper == null)
                return;

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

            previewImage.Frame = new CGRect(10, 20 + PictureY - height, width, height);

            previewImage.Image = CropImage(pic,
                (float)(cropper.Frame.X / maxResizeFactor),
                (float)(cropper.Frame.Y / maxResizeFactor), 
                (float)(cropper.Frame.Width / maxResizeFactor),
                (float)(cropper.Frame.Height / maxResizeFactor));

            previewImage.Layer.BorderColor = UIColor.White.CGColor;
            previewImage.Layer.BorderWidth = 3;

            CALayer profileImageCircle = previewImage.Layer;            
            profileImageCircle.MasksToBounds = true;  

            if (IsRound)
            {
                // make previewImage round                
                profileImageCircle.CornerRadius = previewImage.Frame.Size.Width / 2;  
            }
            else
            {
                profileImageCircle.CornerRadius = 0;
            }
        }
                
        /// <summary>
        /// Sets the overlay
        /// </summary>
        private void SetOverLay()
        {
            if (overlay == null)
            {
                overlay = new CropperOverlayView(cropper.Frame, (float)PictureX, (float)PictureY, IsRound);
                Add(overlay);
            }
            else
            {
                overlay.Redraw(cropper.Frame, IsRound);                
                overlay.SetNeedsDisplay();
            }
        }

        /// <summary>
        /// Sets the resizer triangle to resize the cropper.
        /// </summary>
        private void SetResizer()
        {
            if (resizer == null)
            {
                resizer = new CropperResizerView(cropperColor, cropperTransparency, cropperLineWidth);
                Add(resizer);
            }

            var x = (nfloat)(cropper.Frame.X + cropper.Frame.Width - resizerSize - cropperLineWidth);
            var y = (nfloat)(cropper.Frame.Y + cropper.Frame.Height - resizerSize - cropperLineWidth);

            if (IsRound)
            {
                // move resizer outward when cropper gets smaller, otherwise resizer will overlap circle
                var resizerOffset = -cropperLineWidth + ((this.Frame.Size.Width - cropper.Frame.Width) / this.Frame.Size.Width) * resizerSize;
                x += resizerOffset;
                y += resizerOffset;                
            }

            resizer.Frame = new CGRect(x, y, resizerSize, resizerSize);
        }
                
        private void HandlePinchGesture(UIPinchGestureRecognizer sender)
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
                    SetOverLay();
                    SetResizer();    
                    break;
                case UIGestureRecognizerState.Ended:
                    UpdateImage();
                    break;
            }
        }

        private void AddShadow(ref UIImageView view, int shadow = 3)
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
        private UIImage ResizeImage(UIImage sourceImage, float maxWidth, float maxHeight)
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

            if (cropper == null)
                return;

            // Get the current touch
            UITouch touch = touches.AnyObject as UITouch;
            if (touch == null) return;
            
            if (cropper.Frame.Contains(touch.LocationInView(this)))
            {
                // draggableRectangle touched, prepare to drag
                dragCropper = true;
            }

            if (resizer.Frame.Contains(touch.LocationInView(this)))
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
            UpdateImage();
        }
        
        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);
            // get the touch
            UITouch touch = touches.AnyObject as UITouch;
            
            if (touch == null)
                return;
      
            nfloat offsetX = touch.PreviousLocationInView(this).X - touch.LocationInView(this).X;
            nfloat offsetY = touch.PreviousLocationInView(this).Y - touch.LocationInView(this).Y;

            if (dragCropper)
            {
                // move the cropperRectangle                
                nfloat x = cropper.Frame.X - offsetX;
                nfloat y = cropper.Frame.Y - offsetY;

                cropper.Frame = GetCropperRect(cropper.Frame.Size.Width, cropper.Frame.Size.Height, x, y);
                cropper.SetNeedsDisplay();

                //SetPreviewImage();
                
                SetOverLay();
                SetResizer();
            }
            else if (resizeCropper)
            {
                nfloat width = cropper.Frame.Size.Width - offsetX;
                nfloat height = cropper.Frame.Size.Height - offsetY;
                if (OutputHeight != 0 && OutputWidth != 0)
                {
                    var offset = (offsetX + offsetY) / 2;

                    width = cropper.Frame.Size.Width - offset;
                    height = width / (nfloat)cropperAspectRatio;
                }
                cropper.Frame = GetCropperRect(width, height, cropper.Frame.X, cropper.Frame.Y);

                // make sure to redraw the cropper, otherwise the outline will get narrower and thicker when resizing
                cropper.SetNeedsDisplay();
                                
                //SetPreviewImage();
                
                SetOverLay();
                SetResizer();
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
            if (OutputWidth == 0 || OutputHeight == 0)
                cropperAspectRatio = 1;

            cropperAspectRatio = width / height;
        }

        private CGPoint RestrictCropperPosition(nfloat x, nfloat y)
        {
            // restrict x position (stay within picture)
            if (x > PictureX - cropper.Frame.Width) x = PictureX - cropper.Frame.Width;
            if (x < 0) x = 0;

            // restrict y position (stay within picture)
            if (y > PictureY - cropper.Frame.Height) y = PictureY - cropper.Frame.Height;
            if (y < 0) y = 0;

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
            get { 
                return this.Frame.Height; 
            }
        }

        private nfloat PictureX
        {
            get { 
                return this.Frame.Width;
            }
        }

        #endregion
    }
}
