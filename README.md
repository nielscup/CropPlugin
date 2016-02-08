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
CrossImageCrop.Current.CropImage(
	PicturePath, 
	() => { 
		// Do something with PicturePath 
	}, 300, 300);
```
