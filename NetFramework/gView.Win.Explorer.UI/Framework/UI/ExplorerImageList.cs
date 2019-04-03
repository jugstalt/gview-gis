using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace gView.Framework.UI
{
    public class ExplorerImageList : gView.Framework.system.UI.IUIImageList
    {
        static ImageList _imageList;
        private static object lockThis = new object();

        public static ExplorerImageList List;

        static ExplorerImageList()
        {
            List = new ExplorerImageList();
            List.AddImage((new gView.Framework.UI.Dialogs.Icons()).imageList1.Images[0]);
        }

        private ExplorerImageList()
        {
            _imageList = new ImageList();
            _imageList.ImageSize = new Size(16, 16);
            _imageList.ColorDepth = ColorDepth.Depth32Bit;
        }

        public int CountImages
        {
            get
            {
                lock (lockThis)
                    return _imageList.Images.Count;
            }
        }

        public void AddImage(Image image)
        {
            if (image == null) return;
            lock (lockThis)
            {
                Bitmap bm = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(bm))
                {

                    gr.DrawImage(image, new Rectangle(0, 0, 16, 16), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    bm.MakeTransparent(Color.White);

                    _imageList.Images.Add(bm);
                }
            }
        }

        //public void AddImages(ImageList imageList)
        //{
        //    foreach (Image image in imageList.Images)
        //        AddImage(image);
        //}

        public ImageList ImageList
        {
            get { return _imageList; }
        }

        public Image Image(int index)
        {
            lock (lockThis)
            {
                return _imageList.Images[index];
            }
        }

        public Image this[int index]
        {
            get
            {
                lock (lockThis)
                {
                    return _imageList.Images[index];
                }
            }
        }
    }
}
