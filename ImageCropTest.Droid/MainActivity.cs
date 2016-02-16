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
        CropImageView cropImageView;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            var mainLayout = new LinearLayout(this){ Orientation = Orientation.Vertical};
            
            var buttonlayout1 = new LinearLayout(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)                
            };

            var buttonlayout2 = new LinearLayout(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            SetContentView(mainLayout);
                        
            var takePictureButton = new Button(this) { Text = "Take picture" };
            mainLayout.AddView(takePictureButton);

            var shareButton = new Button(this) { Text = "Share" };
            mainLayout.AddView(shareButton);

            var crop300x300Button1 = new Button(this) { Text = "Crop 300x300" };
            var crop200x300Button1 = new Button(this) { Text = "Crop 200x300" };
            var crop300x200Button1 = new Button(this) { Text = "Crop 300x200" };
            var freeCropButton1 = new Button(this) { Text = "Crop" };            

            // External crop buttons (opens image crop in a new activity)
            buttonlayout1.AddView(crop300x300Button1);
            buttonlayout1.AddView(crop200x300Button1);
            buttonlayout1.AddView(crop300x200Button1);
            buttonlayout1.AddView(freeCropButton1);

            var crop300x300Button2 = new Button(this) { Text = "Crop 300x300" };
            var crop200x300Button2 = new Button(this) { Text = "Crop 200x300" };
            var crop300x200Button2 = new Button(this) { Text = "Crop 300x200" };
            var freeCropButton2 = new Button(this) { Text = "Crop" };
            var cropViewButton2 = new Button(this) { Text = "Crop View" };
            var saveButton = new Button(this) { Text = "Save" };

            imageView = new ImageView(this);
            imageView.LayoutParameters = new ViewGroup.LayoutParams(300, 300);                                              
            
            // Internal crop buttons (crops image in the same activity)
            buttonlayout2.AddView(crop300x300Button2);
            buttonlayout2.AddView(crop200x300Button2);
            buttonlayout2.AddView(crop300x200Button2);
            buttonlayout2.AddView(freeCropButton2);
            buttonlayout2.AddView(cropViewButton2);
            buttonlayout2.AddView(saveButton);

            mainLayout.AddView(buttonlayout1);
            mainLayout.AddView(imageView);
            mainLayout.AddView(buttonlayout2);

            // Create and add crop view
            cropImageView = new CropImageView(this, null);
            //var cropImageViewLayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            //cropImageView.LayoutParameters = cropImageViewLayoutParameters;            
            mainLayout.AddView(cropImageView);
            

            takePictureButton.Click += takePictureButton_Click;

            crop300x300Button1.Click += (s,e) =>  Crop(300, 300);
            crop200x300Button1.Click += (s, e) => Crop(200, 300);
            crop300x200Button1.Click += (s, e) => Crop(300, 200);
            freeCropButton1.Click  += (s,e) =>  Crop(0, 0);

            crop300x300Button2.Click += (s, e) => Crop(300, 300, false);
            crop200x300Button2.Click += (s, e) => Crop(200, 300, false);
            crop300x200Button2.Click += (s, e) => Crop(300, 200, false);
            freeCropButton2.Click += (s, e) => Crop(0, 0, false);
            
            cropViewButton2.Click += (s, e) => StartActivity(typeof(CropActivity));
            saveButton.Click += saveButton_Click;
            shareButton.Click += shareButton_Click;
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(croppedPicturePath))
                return;
            
            cropImageView.CropAndSave(croppedPicturePath);
            SetPicture(croppedPicturePath);
        }

        void shareButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(croppedPicturePath))
                return;

            CrossShare.Current.ShareLocalFile(croppedPicturePath);
        }
                
        void takePictureButton_Click(object sender, EventArgs e)
        {
            //TakePicture();
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            var file = new Java.IO.File(CreateDirectoryForPictures(), string.Format("myPhoto.jpg", System.Guid.NewGuid()));
            picturePath = file.Path;
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
            StartActivityForResult(intent, 0);
        }
                
        void Crop(int width, int height, bool useExternalCropper = true)
        {
            if (string.IsNullOrEmpty(picturePath))
                return;

            // reset the picture to the taken picture
            SetPicture(picturePath);

            croppedPicturePath = picturePath.Replace(".", "-cropped.");

            if (useExternalCropper)
                CrossImageCrop.Current.CropImage(picturePath, croppedPicturePath, () => SetPicture(croppedPicturePath), width, height);
            else            
                cropImageView.SetImage(picturePath, width, height);
        }

        private void SetPicture(string path)
        {
            imageView.SetImageURI(Android.Net.Uri.Parse(path));            
        }

        ///// <summary>
        ///// TEST Front Facing Camera
        ///// </summary>
        //private void TakePicture()
        //{
        //    Intent intent = new Intent(MediaStore.ActionImageCapture);
        //    var file = new Java.IO.File(CreateDirectoryForPictures(), string.Format("myPhoto.jpg", System.Guid.NewGuid()));
        //    picturePath = file.Path;
        //    intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
        //    StartActivityForResult(intent, 0);
        //}
                
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

