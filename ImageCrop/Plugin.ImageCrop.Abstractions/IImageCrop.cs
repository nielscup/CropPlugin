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
        /// <param name="imagePath">The path to the image to be cropped. example: ../pictures/picture.jpg</param>
        /// <param name="croppedImagePath">The path to the image after cropping. This can be the same as imagePath, the image will then be overwritten. example: ../pictures/picture-cropped.jpg</param>
        /// <param name="callback">The action to execute after cropping</param>
        /// <param name="croppedImageWidth">The width of the image after cropping</param>
        /// <param name="croppedImageHeight">The height of the image after cropping</param>
        /// <returns>Task</returns>
        Task CropImage(string imagePath, string croppedImagePath, Action callback, int croppedImageWidth, int croppedImageHeight);

        /// <summary>
        /// Crop Image
        /// </summary>
        /// <param name="imagePath">The path to the image to be cropped. example: ../pictures/picture.jpg</param>
        /// <param name="croppedImagePath">The path to the image after cropping. This can be the same as imagePath, the image will then be overwritten. example: ../pictures/picture-cropped.jpg</param>
        /// <param name="callback">The action to execute after cropping</param>
        /// <returns>Task</returns>
        Task CropImage(string imagePath, string croppedImagePath, Action callback);


    }
}
