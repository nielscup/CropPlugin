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
        /// <param name="imagePath">The path to the image to be cropped</param>
        /// <param name="callback">The action to execute after cropping</param>
        /// <param name="croppedImageWidth">The width of the image after cropping</param>
        /// <param name="croppedImageHeight">The height of the image after cropping</param>
        /// <returns></returns>
        Task CropImage(string imagePath, Action callback, int croppedImageWidth, int croppedImageHeight);

        /// <summary>
        /// Crop Image
        /// </summary>
        /// <param name="imagePath">The path to the image to be cropped</param>
        /// <param name="callback">The action to execute after cropping</param>
        /// <returns></returns>
        Task CropImage(string imagePath, Action callback);
    }
}
