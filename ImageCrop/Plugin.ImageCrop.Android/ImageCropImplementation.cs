using Android.App;
using Android.Content;
using Plugin.ImageCrop.Abstractions;
//using Plugin.ImageCrop.Crop;
using System;
using System.Threading.Tasks;

[assembly: Permission(Name = "android.permission.READ_EXTERNAL_STORAGE")]
[assembly: Permission(Name = "android.permission.WRITE_EXTERNAL_STORAGE")]
namespace Plugin.ImageCrop
{
    /// <summary>
    /// Implementation for ImageCrop
    /// </summary>
    public class ImageCropImplementation : IImageCrop
    {
        internal ImageCropImplementation()
        {
            ImageCropInstance.ImageCropView = new ImageCropView(Application.Context, null);
        }

        /// <summary>
        /// Instance of the Custom Camera View
        /// </summary>
        public IImageCropView ImageCropView
        {
            get { return ImageCropInstance.ImageCropView; }
        }
    }

    internal static class ImageCropInstance
    {
        internal static IImageCropView ImageCropView { get; set; }        
    }
}