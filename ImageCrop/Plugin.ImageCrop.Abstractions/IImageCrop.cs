using System;
using System.Threading.Tasks;

namespace Plugin.ImageCrop.Abstractions
{
    /// <summary>
    /// Interface for ImageCrop
    /// </summary>
    public interface IImageCrop
    {
        /// <summary>
        /// Crop Image
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <param name="The width of the image after cropping"></param>
        /// <param name="The height of the image after cropping"></param>
        /// <returns></returns>
        Task CropImage(string imagePath, Action callback, int croppedImageWidth, int croppedImageHeight);

        /// <summary>
        /// Crop Image
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task CropImage(string imagePath, Action callback);
    }
}
