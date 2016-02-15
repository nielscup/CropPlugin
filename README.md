# ImageCrop Plugin for Xamarin

Simple way to crop an image in Xamarin.iOS and Xamarin.Android

#### Setup
* Coming soon to NuGet
* Or run the following command to create a nuget package:
```
nuget pack Plugin.ImageCrop.nuspec
```

**Supports**
* Xamarin.iOS (Unified)
* Xamarin.Android

### API Usage

Call **CrossShare.Current** from any project or PCL to gain access to APIs.

**Crop Image**
```
/// <summary>
/// Crop Image to a specified width and height, maintains aspect ratio
/// </summary>
/// <param name="imagePath">The path to the image to be cropped. example: ../pictures/picture.jpg</param>
/// <param name="croppedImagePath">The path to the image after cropping. This can be the same as imagePath, the image will then be overwritten. example: ../pictures/picture-cropped.jpg</param>
/// <param name="callback">The action to execute after cropping</param>
/// <param name="croppedImageWidth">The width of the image after cropping</param>
/// <param name="croppedImageHeight">The height of the image after cropping</param>
/// <returns>Task</returns>
Task CropImage(string imagePath, string croppedImagePath, Action callback, int croppedImageWidth, int croppedImageHeight);
```
```
/// <summary>
/// Crop Image
/// </summary>
/// <param name="imagePath">The path to the image to be cropped. example: ../pictures/picture.jpg</param>
/// <param name="croppedImagePath">The path to the image after cropping. This can be the same as imagePath, the image will then be overwritten. example: ../pictures/picture-cropped.jpg</param>
/// <param name="callback">The action to execute after cropping</param>
/// <returns>Task</returns>
Task CropImage(string imagePath, string croppedImagePath, Action callback);
```
**Example**
```
CrossImageCrop.Current.CropImage(
	PicturePath, 
	() => { 
		// Do something with PicturePath 
	}, 300, 300);
```
