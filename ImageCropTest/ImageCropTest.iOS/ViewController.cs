﻿using CoreAnimation;
using CoreGraphics;
using Foundation;
using ImageCropTest.iOS.Helpers;
using LocalAuthentication;
using Plugin.CustomCamera;
using Plugin.CustomCamera.Abstractions;
using Plugin.ImageCrop;
using Plugin.ImageCrop.Abstractions;
using Plugin.ShareFile;
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
        //string _croppedImagePath;
        UIButton _authenticateButton;
        bool _isRound;

        UIButton cropButton300x300;
        UIButton cropButton300x200;
        UIButton anyCropButton;
        UIButton roundCropButton;
        UIButton saveImageButton;
        UIButton shareImageButton;  

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
            //frame.Height -= 100;
            ((UIView)CrossImageCrop.Current.ImageCropView).Frame = frame;
            Add((UIView)CrossImageCrop.Current.ImageCropView);

            ((UIView)CrossCustomCamera.Current.CustomCameraView).BackgroundColor = UIColor.White;
            ((UIView)CrossCustomCamera.Current.CustomCameraView).Frame = frame;
            Add((UIView)CrossCustomCamera.Current.CustomCameraView);
            CrossCustomCamera.Current.CustomCameraView.Start(CameraSelection.Front);
            
            yPos = (int)UIScreen.MainScreen.Bounds.Height - 75;
            var buttonWidth = (int)UIScreen.MainScreen.Bounds.Width / 3;

            var takePictureButton = AddButton("Take Picture", buttonWidth);
            takePictureButton.TouchUpInside += takePictureButton_TouchUpInside;

            var selectPictureButton = AddButton("Select Picture", buttonWidth, true);
            selectPictureButton.TouchUpInside += selectPictureButton_TouchUpInside;

            shareImageButton = AddButton("Share", 56, true);
            shareImageButton.TouchUpInside += (s, e) =>
            {
                CrossShareFile.Current.ShareLocalFile(_imagePath);
            };

            buttonWidth = (int)UIScreen.MainScreen.Bounds.Width / 4;
            cropButton300x300 = AddButton("300x300", buttonWidth);
            cropButton300x300.TouchUpInside += (s, e) => SetCropper(300, 300);

            cropButton300x200 = AddButton("300x200", buttonWidth, true);
            cropButton300x200.TouchUpInside += (s, e) => SetCropper(300, 200);

            anyCropButton = AddButton("Any", 38, true);
            anyCropButton.TouchUpInside += (s, e) => SetCropper(0, 0);

            roundCropButton = AddButton("Round", 56, true);
            roundCropButton.TouchUpInside += (s, e) => SetCropper(200, 200, true);

            saveImageButton = AddButton("Save", 56, true);
            saveImageButton.TouchUpInside += (s, e) =>
            {
                CrossImageCrop.Current.ImageCropView.CropAndSave(_imagePath);
                SetPicture(_imagePath);
            };            

            ShowButtons(false);
        }

        void ShowButtons(bool isVisible)
        {
            cropButton300x300.Hidden = !isVisible;
            cropButton300x200.Hidden = !isVisible;
            anyCropButton.Hidden = !isVisible;
            roundCropButton.Hidden = !isVisible;
            saveImageButton.Hidden = !isVisible;
            shareImageButton.Hidden = !isVisible;
        }

        void takePictureButton_TouchUpInside(object sender, EventArgs e)
        {
            if (((UIView)CrossCustomCamera.Current.CustomCameraView).Hidden == true)
            {
                if(_picture != null)
                    _picture.Hidden = true;

                ((UIView)CrossCustomCamera.Current.CustomCameraView).Hidden = false;
                ((UIView)CrossImageCrop.Current.ImageCropView).Hidden = true;
                ShowButtons(false);
                CrossCustomCamera.Current.CustomCameraView.Reset();
                return;
            }

            CrossCustomCamera.Current.CustomCameraView.TakePicture((path) =>
                {
                    _imagePath = path;
                    SetCropper();
                    ((UIView)CrossCustomCamera.Current.CustomCameraView).Hidden = true;
                });
        }

        void selectPictureButton_TouchUpInside(object sender, EventArgs e)
        {
            _picture.Hidden = true;
            ((UIView)CrossCustomCamera.Current.CustomCameraView).Hidden = true;
            CameraHelper.SelectPicture(this, (obj) => SaveImage(obj));            
        }

        void SaveImage(NSDictionary obj)
        {
            var picture = obj.ValueForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;
            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _imagePath = System.IO.Path.Combine(documentsDirectory, "picture.jpg"); // hardcoded filename, overwritten each time
            //_croppedImagePath = System.IO.Path.Combine(documentsDirectory, "picture-cropped.jpg");
            NSData imgData = picture.AsJPEG();
            NSError err = null;
            if (imgData.Save(_imagePath, false, out err))
            {
                SetCropper();
                //SetPicture(_imagePath);
            }
            else
            {
                Console.WriteLine("NOT saved as " + _imagePath + " because" + err.LocalizedDescription);
            }
        }

        void SetCropper(int width = 0, int height = 0, bool isRound = false)
        {
            _isRound = isRound;
            ((UIView)CrossImageCrop.Current.ImageCropView).Hidden = false;
            ShowButtons(true);
            CrossImageCrop.Current.ImageCropView.SetImage(_imagePath, width, height, isRound);
        }

        void SetPicture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            ((UIView)CrossImageCrop.Current.ImageCropView).Hidden = true;
            ((UIView)CrossCustomCamera.Current.CustomCameraView).Hidden = true;

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
            _authenticateButton = AddButton("Authenticate");
            _authenticateButton.TouchUpInside += (s, e) => AuthenticateMe();
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
                            _authenticateButton.Hidden = true;
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