using Plugin.ImageCrop.Abstractions;
using System;


namespace Plugin.ImageCrop
{
  /// <summary>
  /// Implementation for Feature
  /// </summary>
  public class ImageCropImplementation : IImageCrop
  {      
      public System.Threading.Tasks.Task CropImage(string imagePath, string croppedImagePath, Action callback, int croppedImageWidth, int croppedImageHeight)
      {
          throw new NotImplementedException();
      }

      public System.Threading.Tasks.Task CropImage(string imagePath, string croppedImagePath, Action callback)
      {
          throw new NotImplementedException();
      }
  }
}