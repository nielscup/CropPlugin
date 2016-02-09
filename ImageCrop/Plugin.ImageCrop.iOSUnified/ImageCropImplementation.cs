using Foundation;
using Plugin.ImageCrop.Abstractions;
using System;
using System.Threading.Tasks;
using UIKit;


namespace Plugin.ImageCrop
{
    /// <summary>
    /// Implementation for ImageCrop
    /// </summary>
    public class ImageCropImplementation : IImageCrop
    {
        Action _callback;

        public async Task CropImage(string imagePath, string croppedImagePath, Action callback)
        {
            await CropImage(imagePath, croppedImagePath, callback, 0, 0);
        }

        public async Task CropImage(string imagePath, string croppedImagePath, Action callback, int croppedImageWidth, int croppedImageHeight)        
        {
            try
            {
                var _cropImageViewController = new CropImageViewController(imagePath, croppedImagePath, croppedImageWidth, croppedImageHeight);
                _cropImageViewController.OnSaved += cropImageViewController_OnSaved;
                _callback = callback;
                
                var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
                await vc.PresentViewControllerAsync(_cropImageViewController, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to crop image" + ex.Message);
            }
        }
        
        void cropImageViewController_OnSaved(object sender, EventArgs e)
        {
            var cb = _callback;
            _callback = null;

            cb();
        }                
    }
}