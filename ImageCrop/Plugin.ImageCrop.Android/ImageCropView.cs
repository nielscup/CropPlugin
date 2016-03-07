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
using Plugin.ImageCrop.Abstractions;
using Android.App;

namespace Plugin.ImageCrop
{
    /// <summary>
    /// The image crop view
    /// </summary>
    [Register("plugin.imagecrop.android.ImageCropView")]
    public class ImageCropView : ImageViewTouchBase, IImageCropView
    {
        List<HighlightView> hightlightViews = new List<HighlightView>();
        HighlightView mMotionHighlightView = null;
        readonly ScaleGestureDetector _scaleDetector;
        float mLastX;
        float mLastY;
        global::Plugin.ImageCrop.HighlightView.HitPosition motionEdge;
        Context context;
        bool saving;
        Bitmap.CompressFormat outputFormat = Bitmap.CompressFormat.Jpeg;
        HighlightView highlightView;
        Bitmap bitmap;
        bool scaleImage = true;
        bool scaleUp = true;
        bool isScaling = false;
        int cropperMinWidth = 100;
        Handler mHandler = new Handler();
        int _width;
        int _height;
        //float scale = 1;
                
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attrs"></param>
        public ImageCropView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetLayerType(Android.Views.LayerType.Software, null);
            this.context = context;
            _scaleDetector = new ScaleGestureDetector(context, new MyScaleListener(this));

            GetScreenResolution(context);

            // make this view available in the PCL
            ImageCropInstance.ImageCropView = this;
        }

        private void GetScreenResolution(Context context)
        {
            var display = ((Activity)context).WindowManager.DefaultDisplay;
            DisplayMetrics metrics = new DisplayMetrics();
            display.GetMetrics(metrics);

            _width = metrics.WidthPixels;
            _height = metrics.HeightPixels;
        }

        #region interface implementation

        string _imagePath;
        int _outputWidth;
        int _outputHeight;
        bool _isRound;
        
        /// <summary>
        /// The local path to the image to be cropped, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto-cropped.jpg"
        /// </summary>
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                //if (_imagePath == value)
                //    return;

                _imagePath = value;
                SetCropper();
            }
        }  

        /// <summary>
        /// The width after cropping, set to 0 for any width or height
        /// </summary>        
        public int OutputWidth 
        { 
            get
            {
                return _outputWidth;
            }
            set
            {
                if (_outputWidth == value)
                    return;

                _outputWidth = value;
                SetCropper();
            }
        }

        /// <summary>
        /// The height after cropping, set to 0 for any height or height
        /// </summary>
        public int OutputHeight
        {
            get
            {
                return _outputHeight;
            }
            set
            {
                if (_outputHeight == value)
                    return;

                _outputHeight = value;
                SetCropper();
            }
        }

        /// <summary>
        /// Determines wether the cropper is round. A round cropped image will be saved as a square image, therefor the OuputHeight will be ignored when set to true.
        /// </summary>
        public bool IsRound
        {
            get
            {
                return _isRound;
            }
            set
            {
                if (_isRound == value)
                    return;

                _isRound = value;
                SetCropper();
            }
        }

        /// <summary>
        /// Sets the image to be cropped
        /// </summary>
        /// <param name="imagePath">the image path, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto.jpg"</param>
        /// <param name="outputWidth">The width after cropping, leave empty or set to 0 for any width</param>
        /// <param name="outputHeight">The height after cropping, leave empty or set to 0 for any height</param>
        /// <param name="isRound">Determines wether the cropper is round. A round cropped image will be saved as a square image, therefor the OuputHeight will be ignored when set to true.</param>
        public void SetImage(string imagePath, int outputWidth = 0, int outputHeight = 0, bool isRound = false)
        {
            IsRound = isRound;
            OutputWidth = outputWidth;
            OutputHeight = outputHeight;

            if (isRound)
            {
                if (OutputWidth == 0)
                    _outputWidth = cropperMinWidth;

                OutputHeight = OutputWidth;
            }

            ImagePath = imagePath;
        }

        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="destinationPath">the image destination path after cropping, fi: "/storage/emulated/0/Pictures/TempPictures/myPhoto-cropped.jpg"</param>
        public void CropAndSave(string destinationPath)
        {
            var saveUri = Util.GetImageUri(destinationPath);
            CropAndSave(saveUri);
        }

        #endregion

        private void SetCropper()
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
                return;

            bitmap = Util.GetBitmap(_imagePath, context.ContentResolver);
            SetImageBitmapResetBase(bitmap, true);
            AddHighlightView(bitmap);
        }
  
        #region Public methods
                                
        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="destinationUri">the image destination uri after cropping</param>
        private void CropAndSave(Android.Net.Uri destinationUri)
        {
            var bmp = Crop();
            SaveOutput(bmp, destinationUri);
        }

        /// <summary>
        /// Crops the image
        /// </summary>
        internal Bitmap Crop()
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
                if (OutputWidth != 0 && OutputHeight != 0)
                {
                    if (scaleImage)
                    {
                        // Scale the image to the required dimensions
                        Bitmap old = croppedImage;
                        croppedImage = Util.transform(new Matrix(), croppedImage, OutputWidth, OutputHeight, scaleUp);
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
                        Bitmap b = Bitmap.CreateBitmap(OutputWidth, OutputHeight, Bitmap.Config.Rgb565);
                        Canvas canvas = new Canvas(b);

                        Rect srcRect = highlightView.CropRect;
                        Rect dstRect = new Rect(0, 0, OutputWidth, OutputHeight);

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

        /// <summary>
        /// Saves the cropped image
        /// </summary>
        /// <param name="croppedImage">the cropped image</param>
        /// <param name="saveUri"> the uri to save the cropped image to</param>
        internal void SaveOutput(Bitmap croppedImage, Android.Net.Uri saveUri)
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
                hightlightViews[i].Draw(canvas, IsRound);
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
                return false;

            _scaleDetector.OnTouchEvent(ev);

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
                    isScaling = false;
                    break;

                case MotionEventActions.Move:
                    if (!isScaling && mMotionHighlightView != null)
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

        public void ScaleCropper(float scaleFactor)
        {
            isScaling = true;
            highlightView.GrowBy(
                scaleFactor * highlightView.CropRect.Width() - highlightView.CropRect.Width(), 
                scaleFactor * highlightView.CropRect.Height() - highlightView.CropRect.Height());
        }
                
        #endregion

        #region Private helpers

        private void ClearHighlightViews()
        {
            this.hightlightViews.Clear();
        }

        private void AddHighlightView(HighlightView hv)
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

            thisWidth = _width;
            thisHeight = _height;

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

            if (OutputWidth != 0 && OutputHeight != 0)
            {
                if(OutputWidth < OutputHeight)
                {
                    var aspectRatio =  (float)OutputWidth / (float)OutputHeight;
                    cropWidth = (int)(cropHeight * aspectRatio);
                }
                else
                {
                    var aspectRatio = (float)OutputHeight / (float)OutputWidth;
                    cropHeight = (int)(cropWidth * aspectRatio);
                }
            }

            int x = (width - cropWidth) / 2;
            int y = (height - cropHeight) / 2;

            RectF cropRect = new RectF(x, y, x + cropWidth, y + cropHeight);

            //int x = (width - OutputWidth) / 2;
            //int y = (height - OutputHeight) / 2;
            //RectF cropRect = new RectF(x, y, OutputWidth, OutputHeight);
            highlightView.Setup(this.ImageMatrix, imageRect, cropRect, OutputWidth != 0 && OutputHeight != 0);

            this.ClearHighlightViews();
            highlightView.Focused = true;
            this.AddHighlightView(highlightView);

            Center(true, true);
        }
                
        #endregion
    }

    public  class MyScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
    {
        private readonly ImageCropView _view;

        public MyScaleListener(ImageCropView view)
        {
            _view = view;
        }

        public override bool OnScale(ScaleGestureDetector detector)
        {
            _view.ScaleCropper(detector.ScaleFactor);
            return true;
        }
    }
}
