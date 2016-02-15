using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.ImageCrop.Abstractions
{
    public class PictureData
    {
        public string OriginalImagePath { get; set; }
        public int OriginalImageWidth { get; set; }
        public int OriginalImageHeight { get; set; }
        public string CroppedImagePath { get; set; }        
        public int CroppedImageWidth { get; set; }
        public int CroppedImageHeight { get; set; }
    }
}
