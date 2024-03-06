namespace VideoCapture.Controls
{
    using VideoCapture.Imaging;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;

    public class PictureBox : System.Windows.Forms.PictureBox
    {
        private System.Drawing.Image convertedImage;
        private System.Drawing.Image sourceImage;

        public System.Drawing.Image Image
        {
            get
            {
                return this.sourceImage;
            }
            set
            {
                if (((value != null) && (value is Bitmap)) && (((value.PixelFormat == PixelFormat.Format16bppGrayScale) || (value.PixelFormat == PixelFormat.Format48bppRgb)) || (value.PixelFormat == PixelFormat.Format64bppArgb)))
                {
                    System.Drawing.Image image = Boka.VideoCapture.Imaging.Image.Convert16bppTo8bpp((Bitmap) value);
                    base.Image = image;
                    if (this.convertedImage != null)
                    {
                        this.convertedImage.Dispose();
                    }
                    this.convertedImage = image;
                }
                else
                {
                    base.Image = value;
                }
                this.sourceImage = value;
            }
        }
    }
}

