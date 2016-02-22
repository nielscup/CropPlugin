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
        ImageCropView imageCropView;
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

            var customCropViewButton = new Button(this) { Text = "Custom Crop View" };
            mainLayout.AddView(customCropViewButton);

            var crop300x300Button1 = new Button(this) { Text = "Crop 300x300" };
            var crop200x300Button1 = new Button(this) { Text = "Crop 200x300" };
            var crop300x200Button1 = new Button(this) { Text = "Crop 300x200" };
            var anyCropButton1 = new Button(this) { Text = "Crop" };            

            // External crop buttons (opens image crop in a new activity)
            buttonlayout1.AddView(crop300x300Button1);
            buttonlayout1.AddView(crop200x300Button1);
            buttonlayout1.AddView(crop300x200Button1);
            buttonlayout1.AddView(anyCropButton1);

            var crop300x300Button2 = new Button(this) { Text = "300x300" };
            var crop200x300Button2 = new Button(this) { Text = "200x300" };
            var crop300x200Button2 = new Button(this) { Text = "300x200" };
            var anyCropButton2 = new Button(this) { Text = "Any" };
            var roundCropButton2 = new Button(this) { Text = "Round" };
            
            var saveButton = new Button(this) { Text = "Save" };

            imageView = new ImageView(this);
            imageView.LayoutParameters = new ViewGroup.LayoutParams(300, 300);                                              
            
            // Internal crop buttons (crops image in the same activity)
            buttonlayout2.AddView(crop300x300Button2);
            buttonlayout2.AddView(crop200x300Button2);
            buttonlayout2.AddView(crop300x200Button2);
            buttonlayout2.AddView(anyCropButton2);
            buttonlayout2.AddView(roundCropButton2);      
            buttonlayout2.AddView(saveButton);

            mainLayout.AddView(new TextView(this) { Text = "These buttons open the imagecropper intent" });
            mainLayout.AddView(buttonlayout1);
            mainLayout.AddView(imageView);

            mainLayout.AddView(new TextView(this) { Text = "These buttons set the programatically added imagecropper" });
            mainLayout.AddView(buttonlayout2);

            // Create and add crop view
            imageCropView = new ImageCropView(this, null);           
            mainLayout.AddView(imageCropView);            

            takePictureButton.Click += takePictureButton_Click;

            crop300x300Button1.Click += (s,e) =>  Crop(300, 300);
            crop200x300Button1.Click += (s, e) => Crop(200, 300);
            crop300x200Button1.Click += (s, e) => Crop(300, 200);
            anyCropButton1.Click  += (s,e) =>  Crop(0, 0);

            crop300x300Button2.Click += (s, e) => SetCropper(300, 300);
            crop200x300Button2.Click += (s, e) => SetCropper(200, 300);
            crop300x200Button2.Click += (s, e) => SetCropper(300, 200);
            anyCropButton2.Click += (s, e) => SetCropper(0, 0);
            roundCropButton2.Click += (s, e) => SetCropper(0, 0, true);

            customCropViewButton.Click += customCropViewButton_Click;
            saveButton.Click += saveButton_Click;
            shareButton.Click += shareButton_Click;
        }

        void customCropViewButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(picturePath))
                return;

            var customCropActivity = new Intent(this, typeof(CustomCropActivity));
            customCropActivity.PutExtra("imagepath", picturePath);
            StartActivity(customCropActivity);
        }
        
        void saveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(croppedPicturePath))
                return;
            
            imageCropView.CropAndSave(croppedPicturePath);
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
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            var file = new Java.IO.File(CreateDirectoryForPictures(), string.Format("myPhoto.jpg", System.Guid.NewGuid()));
            picturePath = file.Path;
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(file));
            StartActivityForResult(intent, 0);
        }
                
        void Crop(int width, int height)
        {
            if (string.IsNullOrEmpty(picturePath))
                return;

            // reset the picture to the taken picture
            SetPicture(picturePath);

            croppedPicturePath = picturePath.Replace(".", "-cropped.");            
            CrossImageCrop.Current.CropImage(picturePath, croppedPicturePath, () => SetPicture(croppedPicturePath), width, height);            
        }

        void SetCropper(int width, int height, bool isRound = false)
        {
            imageCropView.SetImage(picturePath, width, height, isRound);
        }

        private void SetPicture(string path)
        {
            if (path == null)
                return;

            imageView.SetImageURI(null);
            imageView.SetImageURI(Android.Net.Uri.Parse(path));            
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

