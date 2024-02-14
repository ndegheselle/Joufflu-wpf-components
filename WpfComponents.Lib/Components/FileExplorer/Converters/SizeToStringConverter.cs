using System;
using System.IO;
using System.Windows.Data;

namespace WpfComponents.Lib.Components.FileExplorer.Converters
{
    public class SizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is FileInfo))
                return null;

            FileInfo fileInfo = (FileInfo)value;
            if (!fileInfo.Exists)
                return null;

            long fileSize = fileInfo.Length;
            string sizeString = $"{fileSize} B";
            if (fileSize >= (1 << 30))
                sizeString = string.Format("{0} GB", fileSize >> 30);
            else if (fileSize >= (1 << 20))
                sizeString = string.Format("{0} MB", fileSize >> 20);
            else if (fileSize >= (1 << 10))
                sizeString = string.Format("{0} KB", fileSize >> 10);
            return sizeString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
