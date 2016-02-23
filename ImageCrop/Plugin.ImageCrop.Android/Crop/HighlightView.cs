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

using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;
using System;

namespace Plugin.ImageCrop
{
    internal class HighlightView
    {
        // The View displaying the image.
        private View context;

        internal enum ModifyMode
        {
            None,
            Move,
            Grow
        }

        private ModifyMode mode = ModifyMode.None;

        private RectF imageRect;  // in image space
        RectF cropRect;  // in image space
        internal Matrix matrix;

        private bool maintainAspectRatio = false;
        private float initialAspectRatio;
        private float resizerSize = 80;

        private Paint focusPaint = new Paint();
        private Paint noFocusPaint = new Paint();
        private Paint outlinePaint = new Paint();

        [Flags]
        internal enum HitPosition
        {
            None,
            GrowLeftEdge,
            GrowRightEdge,
            GrowTopEdge,
            GrowBottomEdge,
            Move
        }

        #region Constructor

        internal HighlightView(View ctx)
        {
            context = ctx;
        }

        #endregion

        #region Properties

        internal bool Focused
        {
            get;
            set;
        }

        internal bool Hidden
        {
            get;
            set;
        }

        internal Rect DrawRect  // in screen space
        {
            get;
            private set;
        }

        // Returns the cropping rectangle in image space.
        internal Rect CropRect
        {
            get
            {
                return new Rect((int)cropRect.Left, (int)cropRect.Top,
                                (int)cropRect.Right, (int)cropRect.Bottom);
            }
        }

        //// Returns the cropping rectangle in image space.
        //internal Circle CropRect
        //{
        //    get
        //    {
        //        return new Rect((int)cropRect.Left, (int)cropRect.Top,
        //                        (int)cropRect.Right, (int)cropRect.Bottom);
        //    }
        //}

        internal ModifyMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (value != mode)
                {
                    mode = value;
                    context.Invalidate();
                }
            }
        }

        #endregion

        #region Public methods

        // Handles motion (dx, dy) in screen space.
        // The "edge" parameter specifies which edges the user is dragging.
        internal void HandleMotion(HitPosition edge, float dx, float dy)
        {
            Rect r = computeLayout();
            if (edge == HitPosition.None)
            {
                return;
            }
            else if (edge == HitPosition.Move)
            {
                // Convert to image space before sending to moveBy().
                moveBy(dx * (cropRect.Width() / r.Width()),
                       dy * (cropRect.Height() / r.Height()));
            }
            else
            {
                if (!edge.HasFlag(HitPosition.GrowLeftEdge) && !edge.HasFlag(HitPosition.GrowRightEdge))
                {
                    dx = 0;
                }

                if (!edge.HasFlag(HitPosition.GrowTopEdge) && !edge.HasFlag(HitPosition.GrowBottomEdge))
                {
                    dy = 0;
                }

                // Convert to image space before sending to growBy().
                float xDelta = dx * (cropRect.Width() / r.Width());
                float yDelta = dy * (cropRect.Height() / r.Height());

                GrowBy((edge.HasFlag(HitPosition.GrowLeftEdge) ? -1 : 1) * xDelta,
                       (edge.HasFlag(HitPosition.GrowTopEdge) ? -1 : 1) * yDelta);
            }
        }

        internal void Draw(Canvas canvas, bool isRound)
        {
            if (Hidden)
            {
                return;
            }

            canvas.Save();
            var resizerPaint = new Paint { Color = new Color(255, 255, 255, 200) };

            //if (!Focused)
            //{
            //    outlinePaint.Color = Color.White;
            //    canvas.DrawRect(DrawRect, outlinePaint);
            //    //canvas.DrawCircle(DrawRect.CenterX(), DrawRect.CenterY(), DrawRect.Width(), resizerPaint);
            //}
            //else
            //{
            Rect viewDrawingRect = new Rect();
            context.GetDrawingRect(viewDrawingRect);

            outlinePaint.Color = Color.White;// new Color(0XFF, 0xFF, 0x8A, 0x00);
            focusPaint.Color = new Color(50, 50, 50, 125);

            Path path = new Path();

            if (isRound)
            {
                path.AddCircle(DrawRect.CenterX(), DrawRect.CenterY(), DrawRect.Width() / 2, Path.Direction.Cw);
                canvas.ClipPath(path, Region.Op.Difference);
                canvas.DrawCircle(viewDrawingRect.CenterX(), viewDrawingRect.CenterY(), viewDrawingRect.Width(), focusPaint);
            }
            else
            {
                path.AddRect(new RectF(DrawRect), Path.Direction.Cw);
                canvas.ClipPath(path, Region.Op.Difference);
                canvas.DrawRect(viewDrawingRect, focusPaint);

            }

            //canvas.ClipPath(path, Region.Op.Difference);

            //if (isRound)
            //    canvas.DrawCircle(viewDrawingRect.CenterX(), viewDrawingRect.CenterY(), viewDrawingRect.Width(), focusPaint);
            //else
            //    canvas.DrawRect(viewDrawingRect, focusPaint);

            canvas.Restore();
            canvas.DrawPath(path, outlinePaint);

            var resizerTriangle = new Path();
            var resizerOffset = 4;
            var triangleSize = resizerSize + resizerOffset;
            resizerTriangle.MoveTo(DrawRect.Right - resizerOffset, DrawRect.Bottom - triangleSize);
            resizerTriangle.LineTo(DrawRect.Right - resizerOffset, DrawRect.Bottom - resizerOffset);
            resizerTriangle.LineTo(DrawRect.Right - triangleSize, DrawRect.Bottom - resizerOffset);
            resizerPaint.SetStyle(Paint.Style.Fill);
            canvas.DrawPath(resizerTriangle, resizerPaint);

            if (isRound)
            {
                // Draw the rectangle around the round cropper                
                var roundCropperRectangle = new Path();
                roundCropperRectangle.MoveTo(DrawRect.Left, DrawRect.Top);
                roundCropperRectangle.LineTo(DrawRect.Right, DrawRect.Top);
                roundCropperRectangle.LineTo(DrawRect.Right, DrawRect.Bottom);
                roundCropperRectangle.LineTo(DrawRect.Left, DrawRect.Bottom);
                roundCropperRectangle.LineTo(DrawRect.Left, DrawRect.Top);

                resizerPaint.SetStyle(Paint.Style.Stroke);
                canvas.DrawPath(roundCropperRectangle, resizerPaint);
            }
        }

        // Determines which edges are hit by touching at (x, y).
        internal HitPosition GetHit(float x, float y)
        {
            Rect r = computeLayout();
            float hysteresis = 20F;
            var retval = HitPosition.None;

            // verticalCheck makes sure the position is between the top and
            // the bottom edge (with some tolerance). Similar for horizCheck.
            bool verticalCheck = (y >= r.Top - hysteresis) && (y < r.Bottom + hysteresis);
            bool horizCheck = (x >= r.Left - hysteresis) && (x < r.Right + hysteresis);

            if (x > DrawRect.Right - resizerSize && x < DrawRect.Right && y > DrawRect.Bottom - resizerSize && y < DrawRect.Bottom)
            {
                retval |= HitPosition.GrowRightEdge;
                retval |= HitPosition.GrowBottomEdge;
            }

            // Not near any edge but inside the rectangle: move.
            if (retval == HitPosition.None && r.Contains((int)x, (int)y))
            {
                retval = HitPosition.Move;
            }

            return retval;
        }

        internal void Invalidate()
        {
            DrawRect = computeLayout();
        }

        internal void Setup(Matrix m, Rect imageRect, RectF cropRect, bool maintainAspectRatio)
        {
            matrix = new Matrix(m);

            this.cropRect = cropRect;
            this.imageRect = new RectF(imageRect);
            this.maintainAspectRatio = maintainAspectRatio;

            initialAspectRatio = cropRect.Width() / cropRect.Height();
            DrawRect = computeLayout();

            focusPaint.SetARGB(125, 50, 50, 50);
            noFocusPaint.SetARGB(125, 50, 50, 50);
            outlinePaint.StrokeWidth = 3;
            outlinePaint.SetStyle(Paint.Style.Stroke);
            outlinePaint.AntiAlias = true;

            mode = ModifyMode.None;
            //init();
        }

        #endregion

        #region Private helpers

        // Grows the cropping rectange by (dx, dy) in image space.
        private void moveBy(float dx, float dy)
        {
            Rect invalRect = new Rect(DrawRect);

            cropRect.Offset(dx, dy);

            // Put the cropping rectangle inside image rectangle.
            cropRect.Offset(
                Math.Max(0, imageRect.Left - cropRect.Left),
                Math.Max(0, imageRect.Top - cropRect.Top));

            cropRect.Offset(
                Math.Min(0, imageRect.Right - cropRect.Right),
                Math.Min(0, imageRect.Bottom - cropRect.Bottom));

            DrawRect = computeLayout();
            invalRect.Union(DrawRect);
            invalRect.Inset(-10, -10);
            context.Invalidate(invalRect);
        }

        // Grows the cropping rectange by (dx, dy) in image space.
        public void GrowBy(float dx, float dy)
        {
            if (maintainAspectRatio)
            {
                if (dx != 0)
                {
                    dy = dx / initialAspectRatio;
                }
                else if (dy != 0)
                {
                    dx = dy * initialAspectRatio;
                }
            }

            // Don't let the cropping rectangle grow too fast.
            // Grow at most half of the difference between the image rectangle and
            // the cropping rectangle.
            RectF r = new RectF(cropRect);
            if (dx > 0F && r.Width() + 2 * dx > imageRect.Width())
            {
                float adjustment = (imageRect.Width() - r.Width()) / 2F;
                dx = adjustment;
                if (maintainAspectRatio)
                {
                    dy = dx / initialAspectRatio;
                }
            }
            if (dy > 0F && r.Height() + 2 * dy > imageRect.Height())
            {
                float adjustment = (imageRect.Height() - r.Height()) / 2F;
                dy = adjustment;
                if (maintainAspectRatio)
                {
                    dx = dy * initialAspectRatio;
                }
            }

            r.Inset(-dx, -dy);

            // Don't let the cropping rectangle shrink too fast.
            float widthCap = Math.Max(Poco.CroppedImageWidth, 100);
            if (r.Width() < widthCap)
            {
                r.Inset(-(widthCap - r.Width()) / 2F, 0F);
            }
            float heightCap = maintainAspectRatio
                ? (widthCap / initialAspectRatio)
                    : widthCap;
            if (r.Height() < heightCap)
            {
                r.Inset(0F, -(heightCap - r.Height()) / 2F);
            }

            // Put the cropping rectangle inside the image rectangle.
            if (r.Left < imageRect.Left)
            {
                r.Offset(imageRect.Left - r.Left, 0F);
            }
            else if (r.Right > imageRect.Right)
            {
                r.Offset(-(r.Right - imageRect.Right), 0);
            }
            if (r.Top < imageRect.Top)
            {
                r.Offset(0F, imageRect.Top - r.Top);
            }
            else if (r.Bottom > imageRect.Bottom)
            {
                r.Offset(0F, -(r.Bottom - imageRect.Bottom));
            }

            cropRect.Set(r);
            DrawRect = computeLayout();
            context.Invalidate();
        }

        // Maps the cropping rectangle from image space to screen space.
        private Rect computeLayout()
        {
            RectF r = new RectF(cropRect.Left, cropRect.Top,
                                cropRect.Right, cropRect.Bottom);
            matrix.MapRect(r);
            return new Rect((int)Math.Round(r.Left), (int)Math.Round(r.Top),
                            (int)Math.Round(r.Right), (int)Math.Round(r.Bottom));
        }

        #endregion
    }
}