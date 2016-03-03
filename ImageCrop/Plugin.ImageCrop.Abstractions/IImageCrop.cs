using System;
using System.Threading.Tasks;

namespace Plugin.ImageCrop.Abstractions
{
    /// <summary>
    /// Interface for ImageCrop
    /// </summary>
    public interface IImageCrop
    {        
        IImageCropView ImageCropView { get; }
    }
}
