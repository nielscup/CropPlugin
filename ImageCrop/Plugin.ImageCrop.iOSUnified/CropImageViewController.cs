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
        public event EventHandler OnSaved;
        ImageCropView imageCropView;
        string _croppedImagePath;

        public CropImageViewController(string imagePath, string croppedImagePath, int croppedImageWidth = 0, int croppedImageHeight = 0)
        {            
            imageCropView = new ImageCropView(new CGRect(0, 80, UIScreen.MainScreen.Bounds.Size.Width, UIScreen.MainScreen.Bounds.Size.Height));
            
            imageCropView.OutputWidth = croppedImageWidth;
            imageCropView.OutputHeight = croppedImageHeight;
            imageCropView.ImagePath = imagePath;
            _croppedImagePath = croppedImagePath;

            Add(imageCropView);

            var cancelButton = AddButton("Cancel", 150f, 20f, UIControlContentHorizontalAlignment.Left);
            cancelButton.TouchUpInside += cancelButton_TouchUpInside;

            var saveButton = AddButton("Save", 150f, 20f, UIControlContentHorizontalAlignment.Right);
            saveButton.TouchUpInside += saveButton_TouchUpInside;                        
        }

        void saveButton_TouchUpInside(object sender, EventArgs e)
        {
            imageCropView.CropAndSave(_croppedImagePath);

            if (OnSaved != null)
                OnSaved(this, EventArgs.Empty);

            DismissViewController(true, null);
        }

        void cancelButton_TouchUpInside(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        private UIButton AddButton(string title, nfloat width, nfloat margin, UIControlContentHorizontalAlignment contentHorizontalAlignment)
        {
            var x = margin;
            if (contentHorizontalAlignment == UIControlContentHorizontalAlignment.Right)
                x = UIScreen.MainScreen.Bounds.Size.Width - (width + margin);

            var button = new UIButton(new CGRect(x, UIScreen.MainScreen.Bounds.Size.Height - 60, width, 40f));
            button.SetTitle(title, UIControlState.Normal);
            button.SetTitleColor(UIColor.White, UIControlState.Normal);
            button.HorizontalAlignment = contentHorizontalAlignment;
            Add(button);

            return button;
        }
    }        
}