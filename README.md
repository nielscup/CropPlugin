# ImageCrop Plugin

Simple way to crop an image in Xamarin.iOS and Xamarin.Android

#### Setup
* Coming soon to NuGet

**Supports**
* Xamarin.iOS (Unified)
* Xamarin.Android (WORK IN PROGRESS)

### API Usage

Call **CrossShare.Current** from any project or PCL to gain access to APIs.

**Crop Image**
```
/// <summary>
/// Crop Image
/// </summary>
/// <param name="imagePath">The path to the image to be cropped</param>
/// <param name="callback">The action to execute after cropping</param>
/// <param name="croppedImageWidth">The width of the image after cropping</param>
/// <param name="croppedImageHeight">The height of the image after cropping</param>
/// <returns></returns>
Task CropImage(string imagePath, Action callback, int croppedImageWidth, int croppedImageHeight);
```
```
/// <summary>
/// Crop Image
/// </summary>
/// <param name="imagePath">The path to the image to be cropped</param>
/// <param name="callback">The action to execute after cropping</param>
/// <returns></returns>
Task CropImage(string imagePath, Action callback);
```
**Example**
```
CrossImageCrop.Current.CropImage(
	PicturePath, 
	() => { 
		// Do something with PicturePath 
	}, 300, 300);
```
