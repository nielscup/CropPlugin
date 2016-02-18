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
    public partial class CustomCropViewController : UIViewController
    {
        ImageCropView _imageCropView;
        int yPos = 30;
        int xPos;
        const int defaultSize = 300;
        string _imagePath;
        string _croppedImagePath;

        public CustomCropViewController(IntPtr handle) : base(handle) { }
        public CustomCropViewController(string imagePath)
        {
            this.View.BackgroundColor = UIColor.White;
            _imagePath = imagePath;
            _croppedImagePath = imagePath.Replace(".", "-cropped.");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            Initialize();             
        }        

        void Initialize()
        {            
            var buttonWidth = 85;
            var cropButton300x300 = AddButton("300x300", buttonWidth);
            cropButton300x300.TouchUpInside += (s, e) => SetCropper(300, 300);

            var cropButton200x300 = AddButton("200x300", buttonWidth, true);
            cropButton200x300.TouchUpInside += (s, e) => SetCropper(200, 300);

            var cropButton300x200 = AddButton("300x200", buttonWidth, true);
            cropButton300x200.TouchUpInside += (s, e) => SetCropper(300, 200);

            var anyCropButton = AddButton("Any", 40, true);
            anyCropButton.TouchUpInside += (s, e) => SetCropper(0, 0);

            var roundCropButton = AddButton("Round", 55, true);
            roundCropButton.TouchUpInside += (s, e) => SetCropper(200, 200, true);

            yPos += 20;
            var cropImageViewSize = Math.Min(UIScreen.MainScreen.Bounds.Size.Width, UIScreen.MainScreen.Bounds.Size.Height);
            _imageCropView = new ImageCropView(new CGRect(0, yPos, cropImageViewSize, cropImageViewSize));
            
            SetCropper(0, 0);
            yPos += (int)_imageCropView.Frame.Size.Height + (int)_imageCropView.Frame.Y;
            Add(_imageCropView);
                        
            var saveButton = AddButton("Save");
            saveButton.TouchUpInside += SaveButton_TouchUpInside;
            
            var cancelButton = AddButton("Cancel");
            cancelButton.TouchUpInside += (s, e) => DismissViewController(true, null);    
        }
          
        void SaveButton_TouchUpInside(object sender, EventArgs e)
        {
            if(_imageCropView == null)
                return;

            _imageCropView.CropAndSave(_croppedImagePath);
            SetCropper(0, 0);
        }
               
        void SetCropper(int width, int height, bool isRound = false)
        {
            _imageCropView.SetImage(_imagePath, width, height, isRound);
        }
                        
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
                
        #region ui controls
        
        private UIButton AddButton(string title, int width = defaultSize, bool behindPreviousControl = false)
        {
            var height = 40;
            
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