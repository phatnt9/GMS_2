﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace GateAccessControl.ViewModels
{
    internal class ProfileListDeviceConverter : IValueConverter
    {
        private ObservableCollection<ProfileDevice> _profileDevices = new ObservableCollection<ProfileDevice>();
        public ObservableCollection<ProfileDevice> ProfileDevices => _profileDevices;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProfileDevices.Clear();
            string ListDevice = value as String;
            List<int> listDeviceId = new List<int>();
            if (!String.IsNullOrEmpty(ListDevice))
            {
                string[] listVar = ListDevice.Split(',');
                foreach (string var in listVar)
                {
                    int temp;
                    Int32.TryParse(var, out temp);
                    if (temp != 0)
                    {
                        listDeviceId.Add(temp);
                    }
                }
                //List<Device> devices = SqliteDataAccess.LoadDevices(0);
                foreach (int id in listDeviceId)
                {
                    ProfileDevice temp = new ProfileDevice(id);
                    ProfileDevices.Add(temp);
                }
            }
            return ProfileDevices;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}