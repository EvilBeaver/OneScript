/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace V8Reader.Core
{
    class ImageArray
    {
        Bitmap m_CompleteImage;
        
        public int Count { get; private set; }
        public int IconWidth { get; private set; }
        public int IconHeight { get; private set; }

        public ImageArray(Bitmap Source, int ItemWidth, int ItemHeight)
        {
            if (ItemWidth <= 0 || ItemHeight <= 0)
                throw new ArgumentException();

            m_CompleteImage = (Bitmap)Source.Clone();
            Count = m_CompleteImage.Width / ItemWidth;

            IconWidth  = ItemWidth;
            IconHeight = ItemHeight;

        }

        public Bitmap this[int index]
        {
            get
            {
                return GetItem(index);
            }
        }

        private Bitmap GetItem(int index)
        {

            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            int startPoint = IconWidth * index;

            Bitmap ItemBitmap = new Bitmap(IconWidth, IconHeight, m_CompleteImage.PixelFormat);
            Graphics.FromImage(ItemBitmap).DrawImage(m_CompleteImage,
                new Rectangle(0, 0, IconWidth, IconHeight),
                new Rectangle(startPoint, 0, IconWidth, IconHeight),
                GraphicsUnit.Pixel);

            return ItemBitmap;

        }

    }

    class WPFImageArray
    {

        public WPFImageArray(BitmapImage Source, int ItemWidth, int ItemHeight)
        {
            m_CompleteImage = Source.Clone();
            m_CompleteImage.Freeze();
            Load(ItemWidth, ItemHeight);
        }

        public WPFImageArray(Uri ResourceURI, int ItemWidth, int ItemHeight)
        {
            m_CompleteImage = new BitmapImage(ResourceURI);
            Load(ItemWidth, ItemHeight);
        }

        public ImageSource this[int index]
        {
            get
            {
                return GetItem(index);
            }
        }

        private ImageSource GetItem(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            if(m_Cache.ContainsKey(index))
            {
                return m_Cache[index];
            }

            int startPoint = IconWidth * index;
            var Frame = new System.Windows.Int32Rect(startPoint, 0, IconWidth, IconHeight);
            byte[] bits = new byte[m_CompleteImage.PixelHeight * stride];
            m_CompleteImage.CopyPixels(Frame, bits, stride, 0);

            WriteableBitmap target = new WriteableBitmap(
              IconWidth,
              IconHeight,
              m_CompleteImage.DpiX, m_CompleteImage.DpiY,
              m_CompleteImage.Format, null);

            // Write the pixel data to the WriteableBitmap.
            target.WritePixels(
                new System.Windows.Int32Rect(0, 0, IconWidth, IconHeight),
                bits, stride, 0);

            target.Freeze();
            m_Cache.Add(index, target);

            return target;

        }

        private void Load(int ItemWidth, int ItemHeight)
        {
            //int bytesPerPixel = (width * m_CompleteImage.Format.BitsPerPixel + 7) / 8;
            //stride = width * bytesPerPixel;
            stride = m_CompleteImage.PixelWidth * (m_CompleteImage.Format.BitsPerPixel / 8);

            Count = (int)m_CompleteImage.Width / ItemWidth;

            IconWidth = ItemWidth;
            IconHeight = ItemHeight;

        }

        public int Count { get; private set; }
        public int IconWidth { get; private set; }
        public int IconHeight { get; private set; }

        private BitmapImage m_CompleteImage;
        private int stride;
        private Dictionary<int, ImageSource> m_Cache = new Dictionary<int,ImageSource>();
    }

}
