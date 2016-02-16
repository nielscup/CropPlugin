using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.ImageCrop;

namespace ImageCropTest.Droid
{
    [Activity(Label = "Crop View")]
    public class CropActivity : Activity
    {
        const string imagePath = "/storage/emulated/0/Pictures/TempPictures/myPhoto.jpg";
        Button buttonSave;
        CropImageView cropImageView;
        ImageView croppedImage;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.CropView);
            
            cropImageView = FindViewById<CropImageView>(Resource.Id.imageCropper);
            croppedImage = FindViewById<ImageView>(Resource.Id.croppedImage);

            var button300x300 = FindViewById<Button>(Resource.Id.button300x300);
            button300x300.Click += (s, e) => SetImage(300, 300);

            var button200x300 = FindViewById<Button>(Resource.Id.button200x300);
            button200x300.Click += (s, e) => SetImage(200, 300);

            var button300x200 = FindViewById<Button>(Resource.Id.button300x200);
            button300x200.Click += (s, e) => SetImage(300, 200);

            var buttonAny = FindViewById<Button>(Resource.Id.buttonAny);
            buttonAny.Click += (s, e) => SetImage(0, 0);

            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonSave.Click += (s, e) => CropAndSaveImage();
            buttonSave.Visibility = ViewStates.Invisible;
        }

        private void SetImage(int width, int height)
        {
            cropImageView.SetImage(imagePath, width, height);
            buttonSave.Visibility = ViewStates.Visible;
        }

        private void CropAndSaveImage()
        {
            var croppedImagePath = imagePath.Replace(".", "-cropped."); 
           
            cropImageView.CropAndSave(croppedImagePath);

            // Set ImageUri to null, otherwise it will not update if set to the same URI
            croppedImage.SetImageURI(null);

            croppedImage.SetImageURI(Android.Net.Uri.Parse(croppedImagePath));            
        }
    }
}