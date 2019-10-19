using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;

namespace GateAccessControl.ViewModels
{
    class ProfileImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = GlobalConstant.ImagePath +"\\"+ value.ToString();
            string defaultImagePath = GlobalConstant.ImagePath + "\\default.png";
            if (targetType == typeof(System.Windows.Media.ImageSource))
            {
                if(File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.EndInit();
                    //Console.WriteLine("Tra hinh moi");
                    return bitmap;
                }
                else
                {
                    if (File.Exists(defaultImagePath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(defaultImagePath);
                        bitmap.EndInit();
                        return bitmap;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
