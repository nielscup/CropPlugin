using CoreAnimation;
using CoreGraphics;
using Foundation;
using ImageCropTest.iOS.Helpers;
using LocalAuthentication;
using Plugin.CustomCamera;
using Plugin.CustomCamera.Abstractions;
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
        UIView _customCameraView;
        UIView _imageCropView;
        bool _isRound;

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
            var frame = UIScreen.MainScreen.Bounds;
            frame.Y = 20;
            frame.Height -= 100;
            //frame.Width -= 40;
            _imageCropView = (UIView)CrossImageCrop.Current.ImageCropView;
            _imageCropView.Frame = frame;
            Add(_imageCropView);

            _customCameraView = (UIView)CrossCustomCamera.Current.CustomCameraView;            
            _customCameraView.BackgroundColor = UIColor.White;
            _customCameraView.Frame = frame;
            Add(_customCameraView);
            CrossCustomCamera.Current.CustomCameraView.Start(CameraSelection.Front);
            
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

            var anyCropButton = AddButton("Any", 38, true);
            anyCropButton.TouchUpInside += (s, e) => SetCropper(0, 0);

            var roundCropButton = AddButton("Round", 56, true);
            roundCropButton.TouchUpInside += (s, e) => SetCropper(200, 200, true);

            var saveImageButton = AddButton("Save", 56, true);
            saveImageButton.TouchUpInside += (s, e) =>
            {
                CrossImageCrop.Current.ImageCropView.CropAndSave(_imagePath);
                SetPicture(_imagePath);
            };
        }

        void takePictureButton_TouchUpInside(object sender, EventArgs e)
        {
            if (_customCameraView.Hidden == true)
            {
                _picture.Hidden = true;
                _customCameraView.Hidden = false;
                _imageCropView.Hidden = true;
                CrossCustomCamera.Current.CustomCameraView.Reset();
                return;
            }

            //CameraHelper.TakePicture(this, (obj) => SaveImage(obj));
            CrossCustomCamera.Current.CustomCameraView.TakePicture((path) =>
                {
                    _imagePath = path;
                    SetCropper();
                    _customCameraView.Hidden = true;
                });
        }

        void selectPictureButton_TouchUpInside(object sender, EventArgs e)
        {
            _picture.Hidden = true;
            _customCameraView.Hidden = true;
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
            _isRound = isRound;
            _imageCropView.Hidden = false;
            CrossImageCrop.Current.ImageCropView.SetImage(_imagePath, width, height, isRound);
        }

        void SetPicture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
                        
            _imageCropView.Hidden = true;
            _customCameraView.Hidden = true;

            if (_picture == null)
            {
                _picture = new UIImageView();
                Add(_picture);
            }

            _picture.Hidden = false;
            _picture.Image = new UIImage(path);

            var factorW = defaultSize / _picture.Image.Size.Width;
            var factorH = defaultSize / _picture.Image.Size.Height;
            var factor = Math.Min(factorW, factorH);
            var w = _picture.Image.Size.Width * factor;
            var h = _picture.Image.Size.Height * factor;
            var x = ((int)UIScreen.MainScreen.Bounds.Width - w) / 2;
            var y = ((int)UIScreen.MainScreen.Bounds.Height - h) / 2;
            _picture.Frame = new CGRect(x, y, w, h);

            CALayer profileImageCircle = _picture.Layer;
            profileImageCircle.MasksToBounds = true;  

            if (_isRound)
            {
                // make previewImage round                
                profileImageCircle.CornerRadius = _picture.Frame.Size.Width / 2;
            }
            else
            {
                profileImageCircle.CornerRadius = 0;
            }
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