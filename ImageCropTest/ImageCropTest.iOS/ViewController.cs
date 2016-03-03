using CoreGraphics;
using Foundation;
using ImageCropTest.iOS.Helpers;
using LocalAuthentication;
using Plugin.ImageCrop;
using Plugin.ImageCrop.Abstractions;
using Plugin.Share;
using System;
using UIKit;

namespace ImageCropTest.iOS
{
    public partial class ViewController : UIViewController
    {
        UIImageView _picture;
        int yPos = 30;
        int xPos;
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
                
        void Initialize()
        {     
            var _imageCropView = (UIView)CrossImageCrop.Current.ImageCropView;
            _imageCropView.Frame = UIScreen.MainScreen.Bounds;
            Add(_imageCropView);

            //var _customCameraView = (UIView)CrossCustomCamera.Current.CustomCameraView;
            //_customCameraView.BackgroundColor = UIColor.White;
            //_customCameraView.Frame = UIScreen.MainScreen.Bounds; //View.Frame; //new CGRect(0, yPos, cropImageViewSize, cropImageViewSize);
            //Add(_customCameraView);
            //CrossCustomCamera.Current.CustomCameraView.Start(CameraSelection.Front);

            yPos = (int)UIScreen.MainScreen.Bounds.Height - 75;
            var buttonWidth = (int)UIScreen.MainScreen.Bounds.Width / 2;

            var takePictureButton = AddButton("Take Picture", buttonWidth);
            takePictureButton.TouchUpInside += takePictureButton_TouchUpInside;

            var selectPictureButton = AddButton("Select Picture", buttonWidth, true);
            selectPictureButton.TouchUpInside += selectPictureButton_TouchUpInside;
                        
            buttonWidth = (int)UIScreen.MainScreen.Bounds.Width / 4;
            var cropButton300x300 = AddButton("300x300", buttonWidth);
            cropButton300x300.TouchUpInside += (s, e) => SetCropper(300, 300);
            
            var cropButton300x200 = AddButton("300x200", buttonWidth, true);
            cropButton300x200.TouchUpInside += (s, e) => SetCropper(300, 200);

            var anyCropButton = AddButton("Any", buttonWidth, true);
            anyCropButton.TouchUpInside += (s, e) => SetCropper(0, 0);

            var roundCropButton = AddButton("Round", buttonWidth, true);
            roundCropButton.TouchUpInside += (s, e) => SetCropper(200, 200, true);

            //var saveImageButton = AddButton("Save", buttonWidth, true);
            //roundCropButton.TouchUpInside += (s, e) =>
            //{
            //    CrossImageCrop.Current.ImageCropView.CropAndSave(_imagePath);
            //    SetPicture(_imagePath);
            //};
        }
                                
        void takePictureButton_TouchUpInside(object sender, EventArgs e)
        {            
            CameraHelper.TakePicture(this, (obj) => SaveImage(obj));
        }

        void selectPictureButton_TouchUpInside(object sender, EventArgs e)
        {
            CameraHelper.SelectPicture(this, (obj) => SaveImage(obj));
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
                SetCropper();
                //SetPicture(_imagePath);
            }
            else
            {
                Console.WriteLine("NOT saved as " + croppedImagePath + " because" + err.LocalizedDescription);
            }
        }
        
        void SetCropper(int width = 0, int height = 0, bool isRound = false)
        {
            CrossImageCrop.Current.ImageCropView.SetImage(_imagePath, width, height, isRound);
        }
                                
        void SetPicture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (_picture == null)
                _picture = new UIImageView();

            _picture.Image = new UIImage(path);

            var factorW = defaultSize / _picture.Image.Size.Width;
            var factorH = defaultSize / _picture.Image.Size.Height;
            var factor = Math.Min(factorW, factorH);
            
            _picture.Frame = new CGRect(_picture.Frame.X, _picture.Frame.Y, _picture.Image.Size.Width * factor, _picture.Image.Size.Height * factor);
        }

        #region Authentication Test
        void SetAuthentication()
        {
            yPos = 220;
            authenticateButton = AddButton("Authenticate");
            authenticateButton.TouchUpInside += (s, e) => AuthenticateMe();
        }

        /// <summary>
        /// TouchId authentication test
        /// </summary>
        private void AuthenticateMe()
        {
            var context = new LAContext();
            NSError AuthError;
            var myReason = new NSString("Login to take a picture");

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

        #endregion

        #region ui controls
        public UIImageView AddImage(int height, int width)
        {
            var image = new UIImageView(GetFrame(height, width, xPos));
            Add(image);

            return image;
        }

        private UIButton AddButton(string title, int width = defaultSize, bool behindPreviousControl = false)
        {
            var height = 36;

            if (behindPreviousControl)
                yPos -= height;
            else
                xPos = 15;

            var button = new UIButton(GetFrame(height, width, xPos));
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(new UIColor(1, 0, 0, 1), UIControlState.Normal);
            Add(button);

            xPos += width;
            yPos += height;

            return button;
        }

        private CGRect GetFrame(int height, int width, int x)
        {
            var rect = new CGRect(x, yPos, width, height);
            return rect;
        }

        #endregion
    }
}