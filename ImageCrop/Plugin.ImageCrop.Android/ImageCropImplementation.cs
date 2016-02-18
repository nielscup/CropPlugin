using Android.App;
using Android.Content;
using Plugin.ImageCrop.Abstractions;
using Plugin.ImageCrop.Crop;
using System;
using System.Threading.Tasks;

[assembly: Permission(Name = "android.permission.READ_EXTERNAL_STORAGE")]
[assembly: Permission(Name = "android.permission.WRITE_EXTERNAL_STORAGE")]
namespace Plugin.ImageCrop
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class ImageCropImplementation : IImageCrop
    {
        /// <summary>
        /// Crop image specified size
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="croppedImagePath"></param>
        /// <param name="callback"></param>
        /// <param name="croppedImageWidth"></param>
        /// <param name="croppedImageHeight"></param>
        /// <returns></returns>
        public async Task CropImage(string imagePath, string croppedImagePath, Action callback, int croppedImageWidth, int croppedImageHeight)
        {            
            Poco.CallBack = callback;
            Poco.ImagePath = imagePath;
            Poco.CroppedImagePath = croppedImagePath;
            Poco.CroppedImageWidth = croppedImageWidth;
            Poco.CroppedImageHeight = croppedImageHeight;

            var intent = new Intent(Android.App.Application.Context, typeof(ImageCropActivityLauncher));
            intent.SetFlags(ActivityFlags.ClearTop);
            intent.SetFlags(ActivityFlags.NewTask);

            Android.App.Application.Context.StartActivity(intent);            
        }

        /// <summary>
        /// Crop image any size
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="croppedImagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task CropImage(string imagePath, string croppedImagePath, Action callback)
        {
            await CropImage(imagePath, croppedImagePath, callback, 0, 0);
        }        
    }
}