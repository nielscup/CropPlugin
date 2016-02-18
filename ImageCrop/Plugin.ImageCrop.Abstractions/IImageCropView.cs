using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.ImageCrop.Abstractions
{
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
        /// Sets the image to be cropped
        /// </summary>
        /// <param name="imagePath">the image path, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto.jpg"</param>
        /// <param name="croppedImageWidth">The width after cropping, leave empty or set to 0 for any width</param>
        /// <param name="croppedImageHeight">The height after cropping, leave empty or set to 0 for any height</param>
        void SetImage(string imagePath, int croppedImageWidth = 0, int croppedImageHeight = 0);

        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="destinationPath">the image destination path after cropping, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto-cropped.jpg"</param>
        void CropAndSave(string destinationPath);
    }
}
