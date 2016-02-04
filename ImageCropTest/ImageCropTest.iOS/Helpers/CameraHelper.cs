using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace ImageCropTest.iOS.Helpers
{
    // Camera by: https://github.com/migueldeicaza/TweetStation/blob/master/TweetStation/UI/Camera.cs
    public static class CameraHelper
    {
        static UIImagePickerController picker;
        static Action<NSDictionary> _callback;

        static void Init()
        {
            if (picker != null)
                return;

            picker = new UIImagePickerController();
            picker.Delegate = new CameraDelegate();
        }

        class CameraDelegate : UIImagePickerControllerDelegate
        {
            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                var cb = _callback;
                _callback = null;

                picker.DismissModalViewController(true);
                cb(info);
            }
        }

        public static void TakePicture(UIViewController parent, Action<NSDictionary> callback)
        {
            Init();
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            _callback = callback;
            parent.PresentModalViewController(picker, true);
        }

        public static void SelectPicture(UIViewController parent, Action<NSDictionary> callback)
        {
            Init();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            _callback = callback;
            parent.PresentModalViewController(picker, true);
        }
    }
}
