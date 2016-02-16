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
    [Activity(Label = "CropActivity")]
    public class CropActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.CropView);

            //var imageCropper = FragmentManager.FindFragmentById<ImageCropFragment>(Resource.Id.imageCropper);
            //imageCropper.setim

            var imageCropper = FindViewById<CropImageView>(Resource.Id.imageCropper);
            imageCropper.SetImage("/storage/emulated/0/Pictures/TempPictures/myPhoto.jpg", 200, 300);
        }
    }
}