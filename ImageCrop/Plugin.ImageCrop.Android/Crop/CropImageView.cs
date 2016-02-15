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

using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.OS;
using Android.Widget;

namespace Plugin.ImageCrop
{
    public class CropImageView : ImageViewTouchBase
    {
        #region Private members

        private List<HighlightView> hightlightViews = new List<HighlightView>();
        private HighlightView mMotionHighlightView = null;
        private float mLastX;
        private float mLastY;
        private global::Plugin.ImageCrop.HighlightView.HitPosition motionEdge;
        private Context context;
        private bool saving;
        private Bitmap.CompressFormat outputFormat = Bitmap.CompressFormat.Jpeg;
        HighlightView highlightView;
        Bitmap bitmap;
        int outputWidth, outputHeight;
        private bool scale = true;
        private bool scaleUp = true;
        private Handler mHandler = new Handler();

        #endregion

        #region Constructor

        public CropImageView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            SetLayerType(Android.Views.LayerType.Software, null);
            this.context = context;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sets the image to be cropped
        /// </summary>
        /// <param name="imagePath">the image path</param>
        /// <param name="contentResolver">the content resolver</param>
        /// <param name="croppedImageWidth"></param>
        /// <param name="croppedImageHeight"></param>
        public void SetImage(string imagePath, int croppedImageWidth = 0, int croppedImageHeight = 0)
        {
            outputWidth = croppedImageWidth;
            outputHeight = croppedImageHeight;
            bitmap = Util.GetBitmap(imagePath, context.ContentResolver);
            SetImageBitmapResetBase(bitmap, true);
            AddHighlightView(bitmap);
        }

        public void CropAndSave(string destinationFile)
        { 
            var saveUri = Util.GetImageUri(destinationFile);
            CropAndSave(saveUri);
        }

        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="contentResolver"></param>
        /// <param name="destinationFile"></param>
        public void CropAndSave(Android.Net.Uri destinationUri)
        {
            var bmp = Crop();
            SaveOutput(bmp, destinationUri);
        }

        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="contentResolver"></param>
        /// <param name="destinationFile"></param>
        public Bitmap Crop()
        {
            try
            {
                // TODO this code needs to change to use the decode/crop/encode single
                // step api so that we don't require that the whole (possibly large)
                // bitmap doesn't have to be read into memory
                if (saving)
                {
                    return null;
                }

                saving = true;

                var r = highlightView.CropRect;

                int width = r.Width();
                int height = r.Height();

                Bitmap croppedImage = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
                {
                    Canvas canvas = new Canvas(croppedImage);
                    Rect dstRect = new Rect(0, 0, width, height);
                    canvas.DrawBitmap(bitmap, r, dstRect, null);
                }

                // If the output is required to a specific size then scale or fill
                if (outputWidth != 0 && outputHeight != 0)
                {
                    if (scale)
                    {
                        // Scale the image to the required dimensions
                        Bitmap old = croppedImage;
                        croppedImage = Util.transform(new Matrix(), croppedImage, outputWidth, outputHeight, scaleUp);
                        if (old != croppedImage)
                        {
                            old.Recycle();
                        }
                    }
                    else
                    {
                        // Don't scale the image crop it to the size requested.
                        // Create an new image with the cropped image in the center and
                        // the extra space filled.              
                        Bitmap b = Bitmap.CreateBitmap(outputWidth, outputHeight, Bitmap.Config.Rgb565);
                        Canvas canvas = new Canvas(b);

                        Rect srcRect = highlightView.CropRect;
                        Rect dstRect = new Rect(0, 0, outputWidth, outputHeight);

                        int dx = (srcRect.Width() - dstRect.Width()) / 2;
                        int dy = (srcRect.Height() - dstRect.Height()) / 2;

                        // If the srcRect is too big, use the center part of it.
                        srcRect.Inset(Math.Max(0, dx), Math.Max(0, dy));

                        // If the dstRect is too big, use the center part of it.
                        dstRect.Inset(Math.Max(0, -dx), Math.Max(0, -dy));

                        // Draw the cropped bitmap in the center
                        canvas.DrawBitmap(bitmap, srcRect, dstRect, null);

                        // Set the cropped bitmap as the new bitmap
                        croppedImage.Recycle();
                        croppedImage = b;                                                
                    }
                }

                return croppedImage;

                //SaveOutput(croppedImage, destinationUri);
            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().Name, ex.Message);
            }
            finally
            {
                saving = false;
            }

            return null;
        }

        public void SaveOutput(Bitmap croppedImage, Android.Net.Uri saveUri)
        {
            if (saveUri == null)
            {
                Log.Error(this.GetType().Name, "invalid image url");
                return;
            }

            using (var outputStream = context.ContentResolver.OpenOutputStream(saveUri))
            {
                if (outputStream != null)
                {
                    croppedImage.Compress(outputFormat, 75, outputStream);
                }
            }

            croppedImage.Recycle();
        }

        #endregion

        #region Overrides

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            for (int i = 0; i < hightlightViews.Count; i++)
            {
                hightlightViews[i].Draw(canvas);
            }
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);

            if (bitmapDisplayed.Bitmap != null)
            {
                foreach (var hv in hightlightViews)
                {
                    hv.matrix.Set(ImageMatrix);
                    hv.Invalidate();

                    if (hv.Focused)
                    {
                        centerBasedOnHighlightView(hv);
                    }
                }
            }
        }

        protected override void PostTranslate(float deltaX, float deltaY)
        {
            base.PostTranslate(deltaX, deltaY);
            for (int i = 0; i < hightlightViews.Count; i++)
            {
                HighlightView hv = hightlightViews[i];
                hv.matrix.PostTranslate(deltaX, deltaY);
                hv.Invalidate();
            }
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (saving)
            {
                return false;
            }

            switch (ev.Action)
            {
                case MotionEventActions.Down:

                    for (int i = 0; i < hightlightViews.Count; i++)
                    {
                        HighlightView hv = hightlightViews[i];
                        var edge = hv.GetHit(ev.GetX(), ev.GetY());
                        if (edge != global::Plugin.ImageCrop.HighlightView.HitPosition.None)
                        {
                            motionEdge = edge;
                            mMotionHighlightView = hv;
                            mLastX = ev.GetX();
                            mLastY = ev.GetY();
                            mMotionHighlightView.Mode =
                            (edge == global::Plugin.ImageCrop.HighlightView.HitPosition.Move)
                                ? HighlightView.ModifyMode.Move
                                : HighlightView.ModifyMode.Grow;
                            break;
                        }
                    }
                    break;

                case MotionEventActions.Up:
                    if (mMotionHighlightView != null)
                    {
                        centerBasedOnHighlightView(mMotionHighlightView);
                        mMotionHighlightView.Mode = HighlightView.ModifyMode.None;
                    }

                    mMotionHighlightView = null;
                    break;

                case MotionEventActions.Move:
                    if (mMotionHighlightView != null)
                    {
                        mMotionHighlightView.HandleMotion(motionEdge,
                                                          ev.GetX() - mLastX,
                                                          ev.GetY() - mLastY);
                        mLastX = ev.GetX();
                        mLastY = ev.GetY();

                        if (true)
                        {
                            // This section of code is optional. It has some user
                            // benefit in that moving the crop rectangle against
                            // the edge of the screen causes scrolling but it means
                            // that the crop rectangle is no longer fixed under
                            // the user's finger.
                            ensureVisible(mMotionHighlightView);
                        }
                    }
                    break;
            }

            switch (ev.Action)
            {
                case MotionEventActions.Up:
                    Center(true, true);
                    break;
                case MotionEventActions.Move:
                    // if we're not zoomed then there's no point in even allowing
                    // the user to move the image around.  This call to center puts
                    // it back to the normalized location (with false meaning don't
                    // animate).
                    if (GetScale() == 1F)
                    {
                        Center(true, true);
                    }
                    break;
            }

            return true;
        }

        #endregion

        #region Private helpers

        public void ClearHighlightViews()
        {
            this.hightlightViews.Clear();
        }

        public void AddHighlightView(HighlightView hv)
        {
            hightlightViews.Add(hv);
            Invalidate();
        }

        // Pan the displayed image to make sure the cropping rectangle is visible.
        private void ensureVisible(HighlightView hv)
        {
            Rect r = hv.DrawRect;

            int panDeltaX1 = Math.Max(0, IvLeft - r.Left);
            int panDeltaX2 = Math.Min(0, IvRight - r.Right);

            int panDeltaY1 = Math.Max(0, IvTop - r.Top);
            int panDeltaY2 = Math.Min(0, IvBottom - r.Bottom);

            int panDeltaX = panDeltaX1 != 0 ? panDeltaX1 : panDeltaX2;
            int panDeltaY = panDeltaY1 != 0 ? panDeltaY1 : panDeltaY2;

            if (panDeltaX != 0 || panDeltaY != 0)
            {
                PanBy(panDeltaX, panDeltaY);
            }
        }

        // If the cropping rectangle's size changed significantly, change the
        // view's center and scale according to the cropping rectangle.
        private void centerBasedOnHighlightView(HighlightView hv)
        {
            Rect drawRect = hv.DrawRect;

            float width = drawRect.Width();
            float height = drawRect.Height();

            float thisWidth = Width;
            float thisHeight = Height;

            float z1 = thisWidth / width * .6F;
            float z2 = thisHeight / height * .6F;

            float zoom = Math.Min(z1, z2);
            zoom = zoom * this.GetScale();
            zoom = Math.Max(1F, zoom);
            if ((Math.Abs(zoom - GetScale()) / zoom) > .1)
            {
                float[] coordinates = new float[]
                {
                    hv.CropRect.CenterX(),
					hv.CropRect.CenterY()
				};

                ImageMatrix.MapPoints(coordinates);
            }

            ensureVisible(hv);
        }

        private void AddHighlightView(Bitmap bitmap)
        {
            if (bitmap == null)
                return;

            highlightView = new HighlightView(this);

            int width = bitmap.Width;
            int height = bitmap.Height;

            Rect imageRect = new Rect(0, 0, width, height);

            // make the default size about 4/5 of the width or height
            int cropWidth = width * 4 / 5; //Math.Min(width, height) * 4 / 5;
            int cropHeight = height * 4 / 5;

            if (outputWidth != 0 && outputHeight != 0)
            {
                if (outputWidth > outputHeight)
                {
                    cropHeight = cropWidth * outputHeight / outputWidth;
                }
                else
                {
                    cropWidth = cropHeight * outputWidth / outputHeight;
                }
            }

            int x = (width - cropWidth) / 2;
            int y = (height - cropHeight) / 2;

            RectF cropRect = new RectF(x, y, x + cropWidth, y + cropHeight);
            highlightView.Setup(this.ImageMatrix, imageRect, cropRect, outputWidth != 0 && outputHeight != 0);

            this.ClearHighlightViews();
            highlightView.Focused = true;
            this.AddHighlightView(highlightView);
        }
                
        #endregion
    }
}
