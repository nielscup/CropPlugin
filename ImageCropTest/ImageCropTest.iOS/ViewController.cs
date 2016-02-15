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
        string picturePath;
        string croppedPicturePath;
        UIButton authenticateButton;

        public ViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            
            yPos = 220;
            authenticateButton = AddButton("Authenticate");
            authenticateButton.TouchUpInside += authenticateButton_TouchUpInside;                               
        }

        void Init()
        {
            authenticateButton.Hidden = true;

            yPos = 50;
            var takePictureButton = AddButton("Take Picture");
            takePictureButton.TouchUpInside += takePictureButton_TouchUpInside;

            var selectPictureButton = AddButton("Select Picture");
            selectPictureButton.TouchUpInside += selectPictureButton_TouchUpInside;

            _picture = AddImage(300, 300);

            var cropButton300x300 = AddButton("Crop 300x300");
            cropButton300x300.TouchUpInside += cropButton300x300_TouchUpInside;

            var cropButton200x300 = AddButton("Crop 200x300");
            cropButton200x300.TouchUpInside += cropButton200x300_TouchUpInside;

            var freeCropButton = AddButton("Crop");
            freeCropButton.TouchUpInside += freeCropButton_TouchUpInside;

            var shareButton = AddButton("Share Picture");
            shareButton.TouchUpInside += shareButton_TouchUpInside;     
        }

        void authenticateButton_TouchUpInside(object sender, EventArgs e)
        {
            AuthenticateMe();
        }

        void cropButton300x300_TouchUpInside(object sender, EventArgs e)
        {
            Crop(300, 300);
        }

        void cropButton200x300_TouchUpInside(object sender, EventArgs e)
        {
            Crop(200, 300);
        }

        void freeCropButton_TouchUpInside(object sender, EventArgs e)
        {
            Crop(0, 0);
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
            await CrossShare.Current.ShareLocalFile(croppedPicturePath);
        }

        void SaveImage(NSDictionary obj)
        {
            var picture = obj.ValueForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;
            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            picturePath = System.IO.Path.Combine(documentsDirectory, "picture.jpg"); // hardcoded filename, overwritten each time
            croppedPicturePath = System.IO.Path.Combine(documentsDirectory, "picture-cropped.jpg");
            NSData imgData = picture.AsJPEG();
            NSError err = null;
            if (imgData.Save(picturePath, false, out err))
            {
                SetPicture(picturePath);

                // Crop
                //CrossImageCrop.Current.CropImage(picturePath, croppedPicturePath, () => SetPicture(croppedPicturePath));
                //CrossImageCrop.Current.CropImage(picturePath, croppedPicturePath, () => SetPicture(croppedPicturePath), 300, 300);
            }
            else
            {
                Console.WriteLine("NOT saved as " + croppedPicturePath + " because" + err.LocalizedDescription);
            }
        }

        void Crop(int croppedImageWidth, int croppedImageHeight)
        {
            CrossImageCrop.Current.CropImage(picturePath, croppedPicturePath, () => SetPicture(croppedPicturePath), croppedImageWidth, croppedImageHeight);
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
                            Init();
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