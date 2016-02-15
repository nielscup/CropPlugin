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

namespace Plugin.ImageCrop
{
    /// <summary>
    /// Simple object for storing and passing image parameters
    /// </summary>
    public static class Poco
    {
        public static string ImagePath { get; set; }
        public static string CroppedImagePath { get; set; }
        public static Action CallBack { get; set; }
        public static int CroppedImageWidth { get; set; }
        public static int CroppedImageHeight { get; set; }
    }
}