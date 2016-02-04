using Plugin.ImageCrop.Abstractions;
using System;

namespace Plugin.ImageCrop
{
  /// <summary>
  /// Cross platform ImageCrop implemenations
  /// </summary>
  public class CrossImageCrop
  {
    static Lazy<IImageCrop> Implementation = new Lazy<IImageCrop>(() => CreateImageCrop(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IImageCrop Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static IImageCrop CreateImageCrop()
    {
#if PORTABLE
        return null;
#else
        return new ImageCropImplementation();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}
