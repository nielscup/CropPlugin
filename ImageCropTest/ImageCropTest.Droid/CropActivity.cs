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
using Plugin.CustomCamera;
using Plugin.CustomCamera.Abstractions;
using Android.Graphics.Drawables;
using Plugin.ShareFile;

namespace ImageCropTest.Droid
{
    [Activity(Label = "Crop View", MainLauncher = true, Icon = "@drawable/icon")]
    public class CropActivity : Activity
    {
        string _imagePath;
        ImageView _croppedImage;
        RoundImageView _croppedImageRound;
        LinearLayout _buttonLayout;
        Button _buttonTakePicture;
        Button _buttonSave;
        Button _buttonShare;
        bool _isRound;
        string croppedImagePath;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _imagePath = Intent.GetStringExtra("imagepath");

            // Create your application here
            SetContentView(Resource.Layout.CropView);

            CrossCustomCamera.Current.CustomCameraView.Start(CameraSelection.Front);

            _buttonLayout = FindViewById<LinearLayout>(Resource.Id.buttonLayout);
            _buttonLayout.Visibility = ViewStates.Gone;

            ((View)CrossImageCrop.Current.ImageCropView).Visibility = ViewStates.Gone;
            _croppedImage = FindViewById<ImageView>(Resource.Id.croppedImage);
            _croppedImage.Visibility = ViewStates.Gone;
            _croppedImageRound = FindViewById<RoundImageView>(Resource.Id.croppedImageRound);
            _croppedImageRound.Visibility = ViewStates.Gone;

            _buttonTakePicture = FindViewById<Button>(Resource.Id.buttonTakePicture);
            _buttonTakePicture.Click += buttonTakePicture_Click;

            var button300x300 = FindViewById<Button>(Resource.Id.button300x300);
            button300x300.Click += (s, e) => SetCropper2(300, 300);
                        
            var button300x200 = FindViewById<Button>(Resource.Id.button300x200);
            button300x200.Click += (s, e) => SetCropper(300, 200);
            
            var buttonAny = FindViewById<Button>(Resource.Id.buttonAny);
            buttonAny.Click += (s, e) => SetCropper();

            var buttonRound = FindViewById<Button>(Resource.Id.buttonRound);
            buttonRound.Click += (s, e) => SetCropper(300, 300, true);

            _buttonSave = FindViewById<Button>(Resource.Id.buttonSave);
            _buttonSave.Click += (s, e) => CropAndSaveImage();
            _buttonSave.Visibility = ViewStates.Gone;

            _buttonShare = FindViewById<Button>(Resource.Id.buttonShare);
            _buttonShare.Click += (s, e) => ShareImage();
            _buttonShare.Visibility = ViewStates.Gone;
        }
                
        void buttonTakePicture_Click(object sender, EventArgs e)
        {
            if (_croppedImage.Visibility == ViewStates.Visible || _croppedImageRound.Visibility == ViewStates.Visible || ((View)CrossImageCrop.Current.ImageCropView).Visibility == ViewStates.Visible)
            {
                _croppedImage.SetImageBitmap(null);
                _croppedImage.Visibility = ViewStates.Gone;
                _buttonShare.Visibility = ViewStates.Gone;

                _croppedImageRound.Visibility = ViewStates.Gone;
                ((View)CrossImageCrop.Current.ImageCropView).Visibility = ViewStates.Gone;
                _buttonLayout.Visibility = ViewStates.Gone;
                _buttonSave.Visibility = ViewStates.Gone;
                ((View)CrossCustomCamera.Current.CustomCameraView).Visibility = ViewStates.Visible;
                CrossCustomCamera.Current.CustomCameraView.Reset();
                
                return;
            }

            CrossCustomCamera.Current.CustomCameraView.TakePicture((path) =>
            {
                try
                {
                    _imagePath = path;
                    SetCropper();
                    ((View)CrossCustomCamera.Current.CustomCameraView).Visibility = ViewStates.Gone;
                    ((View)CrossImageCrop.Current.ImageCropView).Visibility = ViewStates.Visible;
                    _buttonSave.Visibility = ViewStates.Visible;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            SetCropper();
        }

        private void SetCropper(int width = 0, int height = 0, bool isRound = false)
        {
            if (string.IsNullOrEmpty(_imagePath))
                return;

            _isRound = isRound;
            CrossImageCrop.Current.ImageCropView.SetImage(_imagePath, width, height, isRound);

            _buttonLayout.Visibility = ViewStates.Visible;
        }

        private void SetCropper2(int width = 0, int height = 0)
        {
            if (string.IsNullOrEmpty(_imagePath))
                return;

            CrossImageCrop.Current.ImageCropView.OutputWidth = width;
            CrossImageCrop.Current.ImageCropView.OutputHeight = height;

            _buttonLayout.Visibility = ViewStates.Visible;
        }

        private void CropAndSaveImage()
        {
            croppedImagePath = _imagePath.Replace(".", "-cropped.");

            CrossImageCrop.Current.ImageCropView.CropAndSave(croppedImagePath);
            
            // Set ImageUri to null, otherwise it will not update if set to the same URI, and prevent out of memory exceptions
            _croppedImage.SetImageURI(null);            
            _croppedImage.Visibility = ViewStates.Gone;

            _croppedImageRound.SetImageURI(null);
            _croppedImageRound.Visibility = ViewStates.Gone;

            _buttonSave.Visibility = ViewStates.Gone;
            _buttonShare.Visibility = ViewStates.Visible;

            _buttonLayout.Visibility = ViewStates.Gone;
            ((View)CrossImageCrop.Current.ImageCropView).Visibility = ViewStates.Gone;

            if (_isRound)
            {
                _croppedImageRound.SetImageURI(Android.Net.Uri.Parse(croppedImagePath));
                _croppedImageRound.Visibility = ViewStates.Visible;
            }
            else
            {
                _croppedImage.SetImageURI(Android.Net.Uri.Parse(croppedImagePath));
                _croppedImage.Visibility = ViewStates.Visible;
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

        private void ShareImage()
        {
            CrossShareFile.Current.ShareLocalFile(croppedImagePath);
        }
    }
}