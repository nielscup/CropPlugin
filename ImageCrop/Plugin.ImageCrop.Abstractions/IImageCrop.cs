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
        /// <returns></returns>
        Task CropImage(string imagePath, Action callback, int maxCroppedImageSize = 300);
    }
}
