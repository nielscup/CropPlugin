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
    internal static class Poco
    {
        internal static string ImagePath { get; set; }
        internal static string CroppedImagePath { get; set; }
        internal static Action CallBack { get; set; }
        internal static int CroppedImageWidth { get; set; }
        internal static int CroppedImageHeight { get; set; }
    }
}