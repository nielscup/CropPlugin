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

**iOS**
```
UIView _imageCropView;

public override void ViewDidLoad()
{
	base.ViewDidLoad();
    	
	// Add the imageCropView control to your view:
	_imageCropView = (UIView)CrossImageCrop.Current.ImageCropView;
	_imageCropView.Frame = View.Frame frame;
	Add(_imageCropView);
}
```

**Android**
```
<plugin.imagecrop.android.ImageCropView
    android:id="@+id/imageCropper"
    android:layout_width="match_parent"
    android:layout_height="match_parent"
    android:layout_centerHorizontal="true" />
```
**Cross Platform**
```
...
// After selecting an image on your device you can set the cropper:
// For more info on how to select an image, have a look at the included test project
CrossImageCrop.Current.ImageCropView.SetImage(imagePath, width, height, isRound);
...

// When you have set the cropper, you can crop and save the image:
CrossImageCrop.Current.ImageCropView.CropAndSave(imagePath);
...
```


#### Credits
* [cropimage-xamarin by markuspalme](https://github.com/markuspalme/cropimage-xamarin)
* [cropimage by MMP-forTour](https://github.com/MMP-forTour/cropimage)
* [cropimage by biokys](https://github.com/biokys/cropimage)
