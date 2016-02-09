using CoreGraphics;
using Foundation;
using ImageCropTest.iOS.Helpers;
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

        public ViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
                        
            var takePictureButton = AddButton("Take Picture");
            takePictureButton.TouchUpInside += takePictureButton_TouchUpInside;

            var selectPictureButton = AddButton("Select Picture");
            selectPictureButton.TouchUpInside += selectPictureButton_TouchUpInside;

            var shareButton = AddButton("Share Picture");
            shareButton.TouchUpInside += shareButton_TouchUpInside;

            _picture = AddImage(0, 0);            
        }

        void takePictureButton_TouchUpInside(object sender, EventArgs e)
        {            
            CameraHelper.TakePicture(this, (obj) => SaveAndCropImage(obj));
        }

        void selectPictureButton_TouchUpInside(object sender, EventArgs e)
        {
            CameraHelper.SelectPicture(this, (obj) => SaveAndCropImage(obj));
        }

        async void shareButton_TouchUpInside(object sender, EventArgs e)
        {
            await CrossShare.Current.ShareLocalFile(picturePath);
        }

        private void SaveAndCropImage(NSDictionary obj)
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
                CrossImageCrop.Current.CropImage(picturePath, picturePath, () => SetPicture(croppedPicturePath));
                //CrossImageCrop.Current.CropImage(picturePath, croppedPicturePath, () => SetPicture(), 300, 300);
            }
            else
            {
                Console.WriteLine("NOT saved as " + croppedPicturePath + " because" + err.LocalizedDescription);
            }
        }
                                
        private void SetPicture(string path)
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