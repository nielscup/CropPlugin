using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using System;

namespace ImageCropTest.Droid
{
    [Register("ImageCropTest.Droid.RoundImageView")]
    public class RoundImageView : ImageView
    {
        protected RoundImageView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        public RoundImageView(Context context) : this(context, null) { }
        public RoundImageView(Context context, IAttributeSet attrs) : this(context, attrs, 0) { }
        public RoundImageView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) { }

        protected override void OnDraw(Canvas canvas)
        {
            int mBorderWidth;
            int mCanvasSize;
            Bitmap mImage;
            Paint mPaint;
            Paint mPaintBorder;

            mPaint = new Paint();
            mPaint.AntiAlias = true;
            mPaintBorder = new Paint();
            mPaintBorder.Color = BorderColor;
            mPaint.AntiAlias = true;
            mImage = DrawableToBitmap(Drawable);
            mBorderWidth = 2;

            if (mImage != null)
            {
                mCanvasSize = canvas.Width;

                if (canvas.Height < mCanvasSize)
                {
                    mCanvasSize = canvas.Height;
                }

                Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(mImage, mCanvasSize, mCanvasSize, true); //true --> apply the given scaleType.
                BitmapShader shader = new BitmapShader(scaledBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
                mPaint.SetShader(shader);

                int circleCenter = (mCanvasSize - (mBorderWidth * 2)) / 2;

                if (ImageBorderVisibility)
                    canvas.DrawCircle(circleCenter + mBorderWidth, circleCenter + mBorderWidth, ((mCanvasSize - (mBorderWidth * 2)) / 2) + mBorderWidth - 4.0f, mPaintBorder);

                canvas.DrawCircle(circleCenter + mBorderWidth, circleCenter + mBorderWidth, ((mCanvasSize - (mBorderWidth * 2)) / 2) - 4.0f, mPaint);
            }
        }

        private static Bitmap DrawableToBitmap(Drawable drawable)
        {
            if (drawable is BitmapDrawable)
            {
                BitmapDrawable bitmapDrawable = (BitmapDrawable)drawable;
                if (bitmapDrawable.Bitmap != null)
                {
                    return bitmapDrawable.Bitmap;
                }
            }

            return null;
        }

        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }

        public bool _imageBorderVisibility = false;
        public bool ImageBorderVisibility
        {
            get
            {
                return _imageBorderVisibility;
            }
            set
            {
                _imageBorderVisibility = value;
            }
        }

        private Color _borderColor = Color.White;
        public Color BorderColor
        {
            get
            {
                return _borderColor;
            }
            set
            {
                _borderColor = value;
            }
        }
    }
}
