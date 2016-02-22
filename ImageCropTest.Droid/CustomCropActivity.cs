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
    [Activity(Label = "Custom Crop View")]
    public class CustomCropActivity : Activity
    {
        string imagePath;
        Button buttonSave;
        ImageCropView imageCropView;
        ImageView croppedImage;
        RoundImage croppedImageRound;
        bool _isRound;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            imagePath = Intent.GetStringExtra("imagepath");

            // Create your application here
            SetContentView(Resource.Layout.CustomCropView);

            imageCropView = FindViewById<ImageCropView>(Resource.Id.imageCropper);
            croppedImage = FindViewById<ImageView>(Resource.Id.croppedImage);
            croppedImageRound = FindViewById<RoundImage>(Resource.Id.croppedImageRound);

            var button300x300 = FindViewById<Button>(Resource.Id.button300x300);
            button300x300.Click += (s, e) => SetImage(300, 300);

            var button200x300 = FindViewById<Button>(Resource.Id.button200x300);
            button200x300.Click += (s, e) => SetImage(200, 300);

            var button300x200 = FindViewById<Button>(Resource.Id.button300x200);
            button300x200.Click += (s, e) => SetImage(300, 200);

            var buttonAny = FindViewById<Button>(Resource.Id.buttonAny);
            buttonAny.Click += (s, e) => SetImage(0, 0);

            var buttonRound = FindViewById<Button>(Resource.Id.buttonRound);
            buttonRound.Click += (s, e) => SetImage(300, 300, true);

            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonSave.Click += (s, e) => CropAndSaveImage();
            buttonSave.Visibility = ViewStates.Invisible;
        }

        private void SetImage(int width, int height, bool isRound = false)
        {
            _isRound = isRound;
            imageCropView.SetImage(imagePath, width, height, isRound);

            // or you can use:
            //cropImageView.ImagePath = imagePath;
            //cropImageView.OutputWidth = width;
            //cropImageView.OutputHeight = height;

            buttonSave.Visibility = ViewStates.Visible;
        }

        private void CropAndSaveImage()
        {
            var croppedImagePath = imagePath.Replace(".", "-cropped.");

            imageCropView.CropAndSave(croppedImagePath);

            // Set ImageUri to null, otherwise it will not update if set to the same URI
            croppedImage.SetImageURI(null);
            croppedImageRound.SetImageURI(null);
            croppedImage.Visibility = ViewStates.Gone;
            croppedImageRound.Visibility = ViewStates.Gone;

            if (_isRound)
            {
                croppedImageRound.SetImageURI(Android.Net.Uri.Parse(croppedImagePath));
                croppedImageRound.Visibility = ViewStates.Visible;
            }
            else
            {
                croppedImage.SetImageURI(Android.Net.Uri.Parse(croppedImagePath));
                croppedImage.Visibility = ViewStates.Visible;
            }
        }
    }
}