/*
 * Copyright (C) 2009 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using Android.Runtime;

namespace Plugin.ImageCrop
{
    /// <summary>
    /// The activity can crop specific region of interest from an image.
    /// </summary>
	[Activity]
	public class CropImage : MonitoredActivity
    {
        #region Private members

        // These are various options can be specified in the intent.
        private Bitmap.CompressFormat outputFormat = Bitmap.CompressFormat.Jpeg;
        private Android.Net.Uri saveUri = null;
        private Handler mHandler = new Handler();

        // These options specifiy the output image size and whether we should
        // scale the output to fit it (or just crop it).
        private int outputWidth, outputHeight;
        private bool scale = true;
        private bool scaleUp = true;

        private CropImageView cropImageView;
        private HighlightView highlightView;
        private Bitmap bitmap;
        
        private string imagePath;

        private const int NO_STORAGE_ERROR = -1;
        private const int CANNOT_STAT_ERROR = -2;

        #endregion

        #region Properties

        /// <summary>
        /// Whether the "save" button is already clicked.
        /// </summary>
        public bool Saving { get; set; }

        #endregion

        #region Overrides

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
                        
            var mainLayout = new FrameLayout(this);
            var mainLayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
                     
            var buttonLayout = new LinearLayout(this);            
            buttonLayout.Orientation = Orientation.Horizontal;
            buttonLayout.SetHorizontalGravity(GravityFlags.Center);
            buttonLayout.SetVerticalGravity(GravityFlags.Bottom);

            SetContentView(mainLayout);
            
            cropImageView = new CropImageView(this, null);
            var imageViewLayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            cropImageView.LayoutParameters = imageViewLayoutParameters;
            
            var saveButton = new Button(this) { Text = "Save" };
            var cancelButton = new Button(this) { Text = "Cancel" };
                        
            buttonLayout.AddView(cancelButton);
            buttonLayout.AddView(saveButton);

            mainLayout.AddView(cropImageView);
            mainLayout.AddView(buttonLayout);
            
            showStorageToast(this);

            Bundle extras = Intent.Extras;

            if (extras != null)
            {
                imagePath = extras.GetString("image-path");
                var croppedImagePath = extras.GetString("cropped-image-path");

                saveUri = Util.GetImageUri(croppedImagePath);
                if (extras.GetString(MediaStore.ExtraOutput) != null)
                {
                    saveUri = Util.GetImageUri(extras.GetString(MediaStore.ExtraOutput));
                }

                bitmap = Util.GetBitmap(imagePath, ContentResolver);

                outputWidth = Poco.CroppedImageWidth;
                outputHeight = Poco.CroppedImageHeight;
                
                if (extras.GetString("outputFormat") != null)
                {
                    outputFormat = Bitmap.CompressFormat.ValueOf(extras.GetString("outputFormat"));
                }
            }

            if (bitmap == null)
            {
                Finish();
                return;
            }

            Window.AddFlags(WindowManagerFlags.Fullscreen);

            saveButton.Click += saveButton_Click;
            cancelButton.Click += (sender, e) => { SetResult(Result.Canceled); Finish(); };

            cropImageView.SetImage(imagePath, outputWidth, outputHeight);
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            Bundle extras = new Bundle();

            var bmp = cropImageView.Crop();
            BackgroundJob.StartBackgroundJob(this, null, "Saving image", () => cropImageView.SaveOutput(bmp, saveUri), mHandler);
            SetResult(Result.Ok, new Intent(saveUri.ToString()).PutExtras(extras));
            Finish();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (bitmap != null && bitmap.IsRecycled)
            {
                bitmap.Recycle();
            }
        }

        #endregion

        #region Private helpers
                
        private static void showStorageToast(Activity activity)
        {
            showStorageToast(activity, calculatePicturesRemaining());
        }

        private static void showStorageToast(Activity activity, int remaining)
        {
            string noStorageText = null;

            if (remaining == NO_STORAGE_ERROR)
            {
                String state = Android.OS.Environment.ExternalStorageState;
                if (state == Android.OS.Environment.MediaChecking)
                {
                    noStorageText = "Preparing card";
                }
                else
                {
                    noStorageText = "No storage card";
                }
            }
            else if (remaining < 1)
            {
                noStorageText = "Not enough space";
            }

            if (noStorageText != null)
            {
                Toast.MakeText(activity, noStorageText, ToastLength.Long).Show();
            }
        }

        private static int calculatePicturesRemaining()
        {
            try
            {
                string storageDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryPictures).ToString();
                StatFs stat = new StatFs(storageDirectory);
                float remaining = ((float)stat.AvailableBlocks
                                   * (float)stat.BlockSize) / 400000F;
                return (int)remaining;
            }
            catch (Exception)
            {
                // if we can't stat the filesystem then we don't know how many
                // pictures are remaining.  it might be zero but just leave it
                // blank since we really don't know.
                return CANNOT_STAT_ERROR;
            }
        }

        #endregion
    }
}


//private void addHighlightView()
//{
//    highlightView = new HighlightView(cropImageView);

//    int width = bitmap.Width;
//    int height = bitmap.Height;

//    Rect imageRect = new Rect(0, 0, width, height);

//    // make the default size about 4/5 of the width or height
//    int cropWidth = width * 4/5; //Math.Min(width, height) * 4 / 5;
//    int cropHeight = height * 4/5;

//    if (outputWidth != 0 && outputHeight != 0)
//    {
//        if (outputWidth > outputHeight)
//        {
//            cropHeight = cropWidth * outputHeight / outputWidth;
//        }
//        else
//        {
//            cropWidth = cropHeight * outputWidth / outputHeight;
//        }
//    }

//    int x = (width - cropWidth) / 2;
//    int y = (height - cropHeight) / 2;

//    RectF cropRect = new RectF(x, y, x + cropWidth, y + cropHeight);
//    highlightView.Setup(cropImageView.ImageMatrix, imageRect, cropRect, outputWidth != 0 && outputHeight != 0);

//    cropImageView.ClearHighlightViews();
//    highlightView.Focused = true;
//    cropImageView.AddHighlightView(highlightView);
//}

//private void onSaveClicked()
//{
//    // TODO this code needs to change to use the decode/crop/encode single
//    // step api so that we don't require that the whole (possibly large)
//    // bitmap doesn't have to be read into memory
//    if (Saving)
//    {
//        return;
//    }

//    Saving = true;

//    var r = highlightView.CropRect;

//    int width = r.Width();
//    int height = r.Height();

//    Bitmap croppedImage = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
//    {
//        Canvas canvas = new Canvas(croppedImage);
//        Rect dstRect = new Rect(0, 0, width, height);
//        canvas.DrawBitmap(bitmap, r, dstRect, null);
//    }

//    // If the output is required to a specific size then scale or fill
//    if (outputWidth != 0 && outputHeight != 0)
//    {
//        if (scale)
//        {
//            // Scale the image to the required dimensions
//            Bitmap old = croppedImage;
//            croppedImage = Util.transform(new Matrix(), croppedImage, outputWidth, outputHeight, scaleUp);
//            if (old != croppedImage)
//            {
//                old.Recycle();
//            }
//        }
//        else
//        {
//            // Don't scale the image crop it to the size requested.
//            // Create an new image with the cropped image in the center and
//            // the extra space filled.              
//            Bitmap b = Bitmap.CreateBitmap(outputWidth, outputHeight, Bitmap.Config.Rgb565);
//            Canvas canvas = new Canvas(b);

//            Rect srcRect = highlightView.CropRect;
//            Rect dstRect = new Rect(0, 0, outputWidth, outputHeight);

//            int dx = (srcRect.Width() - dstRect.Width()) / 2;
//            int dy = (srcRect.Height() - dstRect.Height()) / 2;

//            // If the srcRect is too big, use the center part of it.
//            srcRect.Inset(Math.Max(0, dx), Math.Max(0, dy));

//            // If the dstRect is too big, use the center part of it.
//            dstRect.Inset(Math.Max(0, -dx), Math.Max(0, -dy));

//            // Draw the cropped bitmap in the center
//            canvas.DrawBitmap(bitmap, srcRect, dstRect, null);

//            // Set the cropped bitmap as the new bitmap
//            croppedImage.Recycle();
//            croppedImage = b;
//        }
//    }

//    // Return the cropped image directly or save it to the specified URI.
//    Bundle myExtras = Intent.Extras;

//    if (myExtras != null && (myExtras.GetParcelable("data") != null || myExtras.GetBoolean("return-data")))
//    {
//        Bundle extras = new Bundle();
//        extras.PutParcelable("data", croppedImage);
//        SetResult(Result.Ok, (new Intent()).SetAction("inline-data").PutExtras(extras));
//        Finish();
//    }
//    else
//    {
//        Bitmap b = croppedImage;
//        //saveOutput(b);
//        BackgroundJob.StartBackgroundJob(this, null, "Saving image", () => saveOutput(b), mHandler);
//    }
//}

//private void saveOutput(Bitmap croppedImage)
//{
//    if (saveUri != null)
//    {
//        try
//        {
//            using (var outputStream = ContentResolver.OpenOutputStream(saveUri))
//            {
//                if (outputStream != null)
//                {
//                    croppedImage.Compress(outputFormat, 75, outputStream);
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            Log.Error(this.GetType().Name, ex.Message);
//        }

//        Bundle extras = new Bundle();
//        SetResult(Result.Ok, new Intent(saveUri.ToString()).PutExtras(extras));
//    }
//    else
//    {
//        Log.Error(this.GetType().Name, "not defined image url");
//    }
//    croppedImage.Recycle();
//    Finish();
//}
