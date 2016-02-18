using CoreGraphics;
using Foundation;
using ImageCropTest.iOS.Helpers;
using LocalAuthentication;
using Plugin.ImageCrop;
using Plugin.Share;
using System;
using UIKit;

namespace ImageCropTest.iOS
{
    public partial class ViewController : UIViewController
    {
        UIImageView _picture;
        int yPos = 50;
        const int defaultSize = 300;
        string _imagePath;
        string croppedImagePath;
        UIButton authenticateButton;

        public ViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            //SetAuthentication();
            Initialize();             
        }

        void SetAuthentication()
        {
            yPos = 220;
            authenticateButton = AddButton("Authenticate");
            authenticateButton.TouchUpInside += (s, e) => AuthenticateMe(); 
        }

        void Initialize()
        {            
            yPos = 50;
            var takePictureButton = AddButton("Take Picture");
            takePictureButton.TouchUpInside += takePictureButton_TouchUpInside;

            var selectPictureButton = AddButton("Select Picture");
            selectPictureButton.TouchUpInside += selectPictureButton_TouchUpInside;

            _picture = AddImage(300, 300);

            var cropButton300x300 = AddButton("Crop 300x300");
            cropButton300x300.TouchUpInside += (s, e) => Crop(300, 300);

            var cropButton200x300 = AddButton("Crop 200x300");
            cropButton200x300.TouchUpInside += (s, e) => Crop(200, 300);

            var freeCropButton = AddButton("Crop");
            freeCropButton.TouchUpInside += (s, e) => Crop(0, 0);

            var shareButton = AddButton("Share Picture");
            shareButton.TouchUpInside += shareButton_TouchUpInside;

            var cropViewButton = AddButton("Custom Crop View");
            cropViewButton.TouchUpInside += cropViewButton_TouchUpInside;
        }

        void cropViewButton_TouchUpInside(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_imagePath))
                return;

            PresentViewController(new CustomCropViewController(_imagePath), true, null);
        }
                
        void takePictureButton_TouchUpInside(object sender, EventArgs e)
        {            
            CameraHelper.TakePicture(this, (obj) => SaveImage(obj));
        }

        void selectPictureButton_TouchUpInside(object sender, EventArgs e)
        {
            CameraHelper.SelectPicture(this, (obj) => SaveImage(obj));
        }

        async void shareButton_TouchUpInside(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(croppedImagePath))
                return;

            await CrossShare.Current.ShareLocalFile(croppedImagePath);
        }

        void SaveImage(NSDictionary obj)
        {
            var picture = obj.ValueForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;
            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _imagePath = System.IO.Path.Combine(documentsDirectory, "picture.jpg"); // hardcoded filename, overwritten each time
            croppedImagePath = System.IO.Path.Combine(documentsDirectory, "picture-cropped.jpg");
            NSData imgData = picture.AsJPEG();
            NSError err = null;
            if (imgData.Save(_imagePath, false, out err))
            {
                SetPicture(_imagePath);
            }
            else
            {
                Console.WriteLine("NOT saved as " + croppedImagePath + " because" + err.LocalizedDescription);
            }
        }

        void Crop(int croppedImageWidth, int croppedImageHeight)
        {
            if (string.IsNullOrWhiteSpace(_imagePath))
                return;

            CrossImageCrop.Current.CropImage(_imagePath, croppedImagePath, () => SetPicture(croppedImagePath), croppedImageWidth, croppedImageHeight);
        }
                                
        void SetPicture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            _picture.Image = new UIImage(path);

            var factorW = defaultSize / _picture.Image.Size.Width;
            var factorH = defaultSize / _picture.Image.Size.Height;
            var factor = Math.Min(factorW, factorH);
            
            _picture.Frame = new CGRect(_picture.Frame.X, _picture.Frame.Y, _picture.Image.Size.Width * factor, _picture.Image.Size.Height * factor);
        }
                
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        /// <summary>
        /// TouchId authentication test
        /// </summary>
        private void AuthenticateMe()
        {
            var context = new LAContext();
            NSError AuthError;
            var myReason = new NSString("To take a picture");

            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out AuthError))
            {
                var replyHandler = new LAContextReplyHandler((success, error) =>
                {
                    this.InvokeOnMainThread(() =>
                    {
                        if (success)
                        {
                            Console.WriteLine("You logged in!");
                            authenticateButton.Hidden = true;
                            Initialize();
                        }
                        else
                        {
                            // Inform the user authentication failed
                        }
                    });

                });
                context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, myReason, replyHandler);
            };
        }

        #region ui controls
        public UIImageView AddImage(int height, int width)
        {
            var image = new UIImageView(GetFrame(height, width));
            Add(image);

            return image;
        }

        public UIButton AddButton(string title, int height = 40)
        {
            var button = new UIButton(GetFrame(height));
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(new UIColor(1, 0, 0, 1), UIControlState.Normal);
            Add(button);

            return button;
        }

        public CGRect GetFrame(int height = 40, int width = defaultSize)
        {
            var rect = new CGRect(40, yPos, width, height);
            yPos += height;

            return rect;
        }
        #endregion
    }
}