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
        internal ImageCropImplementation()
        {
            ImageCropInstance.ImageCropView = new ImageCropView();
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