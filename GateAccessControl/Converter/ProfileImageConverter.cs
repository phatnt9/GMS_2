using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GateAccessControl.ViewModels
{
    internal class ProfileImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            long elapsedMs;
            // the code that you want to measure comes here
            if (value != null)
            {
                string imagePath = GlobalConstant.ImagePath + "\\" + value.ToString();
                string defaultImagePath = GlobalConstant.ImagePath + "\\default.png";
                if (targetType == typeof(System.Windows.Media.ImageSource))
                {
                    if (File.Exists(imagePath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.UriSource = new Uri(imagePath);
                        bitmap.EndInit();
                        watch.Stop();
                        elapsedMs = watch.ElapsedMilliseconds;
                        Console.WriteLine("Load Picture time:" + elapsedMs.ToString());
                        return bitmap;
                    }
                    else
                    {
                        //return true;
                        if (File.Exists(defaultImagePath))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bitmap.UriSource = new Uri(defaultImagePath);
                            bitmap.EndInit();
                            watch.Stop();
                            elapsedMs = watch.ElapsedMilliseconds;
                            Console.WriteLine("Load Picture time:" + elapsedMs.ToString());
                            return bitmap;
                        }
                        else
                        {
                            watch.Stop();
                            elapsedMs = watch.ElapsedMilliseconds;
                            Console.WriteLine("Load Picture time:" + elapsedMs.ToString());
                            return true;
                        }
                    }
                }
            }
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Load Picture time:" + elapsedMs.ToString());
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}