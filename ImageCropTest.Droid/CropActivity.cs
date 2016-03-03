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
using Android.Provider;

namespace ImageCropTest.Droid
{
    [Activity(Label = "Crop View", MainLauncher = true, Icon = "@drawable/icon")]
    public class CropActivity : Activity
    {
        string imagePath;
        Button buttonSave;
        ImageCropView imageCropView;
        ImageView croppedImage;
        RoundImageView croppedImageRound;
        LinearLayout buttonLayout;
        bool _isRound;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            imagePath = Intent.GetStringExtra("imagepath");

            // Create your application here
            SetContentView(Resource.Layout.CropView);

            buttonLayout = FindViewById<LinearLayout>(Resource.Id.buttonLayout);
            buttonLayout.Visibility = ViewStates.Invisible;

            imageCropView = FindViewById<ImageCropView>(Resource.Id.imageCropper);
            croppedImage = FindViewById<ImageView>(Resource.Id.croppedImage);
            croppedImageRound = FindViewById<RoundImageView>(Resource.Id.croppedImageRound);

            var buttonTakePicture = FindViewById<Button>(Resource.Id.buttonTakePicture);
            buttonTakePicture.Click += buttonTakePicture_Click;

            var button300x300 = FindViewById<Button>(Resource.Id.button300x300);
            button300x300.Click += (s, e) => SetCropper(300, 300);

            var button200x300 = FindViewById<Button>(Resource.Id.button200x300);
            button200x300.Click += (s, e) => SetCropper(200, 300);

            var button300x200 = FindViewById<Button>(Resource.Id.button300x200);
            button300x200.Click += (s, e) => SetCropper(300, 200);

            var buttonAny = FindViewById<Button>(Resource.Id.buttonAny);
            buttonAny.Click += (s, e) => SetCropper();

            var buttonRound = FindViewById<Button>(Resource.Id.buttonRound);
            buttonRound.Click += (s, e) => SetCropper(300, 300, true);

            buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            buttonSave.Click += (s, e) => CropAndSaveImage();
            buttonSave.Visibility = ViewStates.Invisible;
        }

        void buttonTakePicture_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            var file = new Java.IO.File(CreateDirectoryForPictures(), string.Format("myPhoto.jpg", System.Guid.NewGuid()));
            imagePath = file.Path;
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
            StartActivityForResult(intent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            SetCropper();
        }

        private void SetCropper(int width = 0, int height = 0, bool isRound = false)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            _isRound = isRound;
            imageCropView.SetImage(imagePath, width, height, isRound);
            
            buttonSave.Visibility = ViewStates.Visible;
            buttonLayout.Visibility = ViewStates.Visible;
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

        private Java.IO.File CreateDirectoryForPictures()
        {
            var dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "TempPictures");
            if (!dir.Exists())
            {
                dir.Mkdirs();
            }

            return dir;
        }
    }
}