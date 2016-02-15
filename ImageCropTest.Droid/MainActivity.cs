using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using Plugin.ImageCrop;
using Plugin.Share;

namespace ImageCropTest.Droid
{
    [Activity(Label = "ImageCropTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        string picturePath;
        string croppedPicturePath;
        ImageView imageView;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            var mainLayout = new LinearLayout(this){ Orientation = Orientation.Vertical};
            
            var buttonlayout = new LinearLayout(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)                
            };

            SetContentView(mainLayout);
                        
            var takePictureButton = new Button(this) { Text = "Take picture" };
            var cropButton = new Button(this) { Text = "Crop 300x300" };
            var freeCropButton = new Button(this) { Text = "Crop" };
            var shareButton = new Button(this) { Text = "Share" };

            imageView = new ImageView(this);
            imageView.LayoutParameters = new ViewGroup.LayoutParams(300, 300);            
            
            buttonlayout.AddView(takePictureButton);
            buttonlayout.AddView(cropButton);
            buttonlayout.AddView(freeCropButton);
            buttonlayout.AddView(shareButton);
            
            mainLayout.AddView(buttonlayout);
            mainLayout.AddView(imageView);

            takePictureButton.Click += takePictureButton_Click;

            cropButton.Click += cropButton_Click;
            freeCropButton.Click += freeCropButton_Click;
            shareButton.Click += shareButton_Click;
        }

        void shareButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(croppedPicturePath))
                return;

            CrossShare.Current.ShareLocalFile(croppedPicturePath);
        }
                
        void takePictureButton_Click(object sender, EventArgs e)
        {
            TakePicture();
        }

        void cropButton_Click(object sender, EventArgs e)
        {
            Crop(300, 300);
        }

        void freeCropButton_Click(object sender, EventArgs e)
        {
            Crop(0, 0);
        }

        void Crop(int width, int height)
        {
            if (string.IsNullOrEmpty(picturePath))
                return;

            croppedPicturePath = picturePath.Replace(".", "-cropped.");
            CrossImageCrop.Current.CropImage(picturePath, croppedPicturePath, () => SetPicture(croppedPicturePath), width, height);
        }

        private void SetPicture(string path)
        {
            imageView.SetImageURI(Android.Net.Uri.Parse(path));           
        }

        /// <summary>
        /// TEST Front Facing Camera
        /// </summary>
        private void TakePicture()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            var file = new Java.IO.File(CreateDirectoryForPictures(), string.Format("myPhoto.jpg", System.Guid.NewGuid()));
            picturePath = file.Path;
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
            StartActivityForResult(intent, 0);
        }
                
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            SetPicture(picturePath);
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

