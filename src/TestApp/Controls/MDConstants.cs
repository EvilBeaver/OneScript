using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace V8Reader.Core
{

    class NamedIcons
    {
        public NamedIcons(WPFImageArray imgArray)
        {
            m_imgArray = imgArray;
            m_NameIndexMap = new Dictionary<string,int>();
        }

        public AbstractImage this[String Name]
        {
            get
            {
                return new Image<ImageSource>(m_imgArray[NameIndexMap[Name]]);
            }
        }

        public Dictionary<String, int> NameIndexMap
        {
            get
            {
                return m_NameIndexMap;
            }
        }

        WPFImageArray m_imgArray;
        private Dictionary<String, int> m_NameIndexMap;
    }

    abstract class AbstractImage
    {
        public AbstractImage(object Image)
        {
            m_Image = Image;
        }

        public virtual object GetImage()
        {
            return m_Image;
        }

        protected object m_Image;
    }

    class IconSet
    {
        private WPFImageArray _arr;
        private Uri _uri;

        public Uri ImageUri
        {
            get
            {
                return _uri;
            }

            set
            {
                _uri = value;
                _arr = new WPFImageArray(value, 16, 16);
            }
        }

        public ImageSource this[int index]
        {
            get
            {
                return _arr[index];
            }
        }

    }
    
    class IconSetItem
    {

        public IconSet Icons { get; set; }
        
        public int Index { get; set; }

        public ImageSource Item 
        {
            get
            {
                return Icons[Index];
            }
        }

    }

    class Image<TImage> : AbstractImage
    {
        public Image(TImage image) : base(image)
        {

        }

        public TImage GetPlatformImage()
        {
            return (TImage)m_Image;
        }
    }

    [ValueConversion(typeof(AbstractImage), typeof(ImageSource))]
    internal class IconTypeConverter : IValueConverter 
    {

        public IconTypeConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is AbstractImage))
                return null;

            Image<ImageSource> source = (Image<ImageSource>)value;

            return source.GetPlatformImage();

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}