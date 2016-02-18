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
    internal class ImageCropActivity : MonitoredActivity
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

        private ImageCropView imageCropView;
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
            
            imageCropView = new ImageCropView(this, null);
            var imageViewLayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            imageCropView.LayoutParameters = imageViewLayoutParameters;
            
            var saveButton = new Button(this) { Text = "Save" };
            var cancelButton = new Button(this) { Text = "Cancel" };
                        
            buttonLayout.AddView(cancelButton);
            buttonLayout.AddView(saveButton);

            mainLayout.AddView(imageCropView);
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

            imageCropView.SetImage(imagePath, outputWidth, outputHeight);
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            Bundle extras = new Bundle();

            var bmp = imageCropView.Crop();
            BackgroundJob.StartBackgroundJob(this, null, "Saving image", () => imageCropView.SaveOutput(bmp, saveUri), mHandler);
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