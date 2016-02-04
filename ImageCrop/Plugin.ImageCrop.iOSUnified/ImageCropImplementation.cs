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
        
        public async Task CropImage(string imagePath, Action callback, int maxCroppedImageSize = 300)
        {
            try
            {
                var _cropImageViewController = new CropImageViewController(imagePath, maxCroppedImageSize);
                _cropImageViewController.OnSaved += cropImageViewController_OnSaved;
                _callback = callback;

                //if (activityController.PopoverPresentationController != null)
                //{
                //    activityController.PopoverPresentationController.SourceView =
                //      UIApplication.SharedApplication.KeyWindow.RootViewController.ChildViewControllers != null
                //        ? UIApplication.SharedApplication.KeyWindow.RootViewController.ChildViewControllers[0].View
                //        : UIApplication.SharedApplication.KeyWindow.RootViewController.View;
                //}

                var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
                await vc.PresentViewControllerAsync(_cropImageViewController, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to share text" + ex.Message);
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