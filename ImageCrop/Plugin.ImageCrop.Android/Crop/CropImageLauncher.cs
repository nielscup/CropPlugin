using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Plugin.ImageCrop.Crop
{
    /// <summary>
    /// Activity to launch the CropImage Activity and respond to its result
    /// </summary>
    [Activity]
    public class CropImageLauncher: Activity
    {
        Action _callback;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CropImage(Poco.ImagePath, Poco.CroppedImagePath, Poco.CallBack);
        }

        public void CropImage(string imagePath, string croppedImagePath, Action callback)
        {
            var pictureUri = Android.Net.Uri.Parse(imagePath);
            var croppedPictureUri = Android.Net.Uri.Parse(croppedImagePath);
            _callback = callback;

            Intent cropIntent = new Intent(this, typeof(CropImage));
            cropIntent.PutExtra("image-path", pictureUri.Path);
            cropIntent.PutExtra("cropped-image-path", croppedPictureUri.Path);
            SetCropVariables(ref cropIntent);
            StartActivityForResult(cropIntent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode != Result.Ok)
            {
                this.Finish();
                return;
            }

            var cb = _callback;
            _callback = null;

            cb();

            this.Finish();
        }

        private void SetCropVariables(ref Intent cropIntent)
        {
            if (Poco.CroppedImageWidth > 0 && Poco.CroppedImageHeight > 0)
            {
                cropIntent.PutExtra("outputX", 300);
                cropIntent.PutExtra("outputY", 300);
                cropIntent.PutExtra("aspectX", 1);
                cropIntent.PutExtra("aspectY", 1);
            }

            cropIntent.PutExtra("scale", "true");
            cropIntent.PutExtra("crop", "true");
        }  
    }
}