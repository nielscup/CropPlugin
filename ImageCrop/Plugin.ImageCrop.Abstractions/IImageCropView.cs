using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.ImageCrop.Abstractions
{
    /// <summary>
    /// The view for cropping images
    /// </summary>
    public interface IImageCropView
    {        
        /// <summary>
        /// The local path to the image to be cropped, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto-cropped.jpg"
        /// </summary>
        string ImagePath { get; set; }

        /// <summary>
        /// The width after cropping, set to 0 for any width or height
        /// </summary> 
        int OutputWidth { get; set; }

        /// <summary>
        /// The height after cropping, set to 0 for any height or height
        /// </summary>
        int OutputHeight { get; set; }

        /// <summary>
        /// Determines wether the cropper is round. A round cropped image will be saved as a square image, therefor the OuputHeight will be ignored when set to true.
        /// </summary>
        bool IsRound { get; set; }

        /// <summary>
        /// Sets the image to be cropped
        /// </summary>
        /// <param name="imagePath">the image path, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto.jpg"</param>
        /// <param name="outputWidth">The width after cropping, leave empty or set to 0 for any width</param>
        /// <param name="outputHeight">The height after cropping, leave empty or set to 0 for any height</param>
        /// <param name="isRound">Determines wether the cropper is round. A round cropped image will be saved as a square image, therefor the OuputHeight will be ignored when set to true.</param>
        void SetImage(string imagePath, int outputWidth = 0, int outputHeight = 0, bool isRound = false);

        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="destinationPath">the image destination path after cropping, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto-cropped.jpg"</param>
        void CropAndSave(string destinationPath);
    }
}
