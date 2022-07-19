using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;

namespace ZeroKey.UI.ViewModel.Services
{
    public class NfcwmqService
    {
        public static ObservableCollection<NfcDevice> GetConnectedDevices()
        {
            ObservableCollection<NfcDevice> devices = new ObservableCollection<NfcDevice>();

            IList<ManagementBaseObject> smartCardDevices = GetSmartCardReaders();

            foreach (ManagementBaseObject smartCardDevice in smartCardDevices)
            {
                devices.Add(new NfcDevice()
                {
                    Id = devices.Count + 1,
                    DeviceName = smartCardDevice.GetPropertyValue("Caption").ToString(),
                    DeviceAddress = smartCardDevice.GetPropertyValue("PNPDeviceID").ToString(),
                    DeviceManufac = smartCardDevice.GetPropertyValue("Manufacturer").ToString(),
                });
            }


            return devices;
        }
        /// <summary>
        /// method which filters our all the devices with class of SmartCardReader
        /// </summary>
        /// <returns>Returns a list  of Devices with SmartCardReader class</returns>
        private static IList<ManagementBaseObject> GetSmartCardReaders()
        {
            IList<string> usbDeviceAddresses = LookUpUsbDeviceAddresses();

            List<ManagementBaseObject> smartCardDevices = new List<ManagementBaseObject>();

            foreach (string usbDeviceAddress in usbDeviceAddresses)
            {
                // query MI for the PNP device info
                // address must be escaped to be used in the query; luckily, the form we extracted previously is already escaped
                ManagementObjectCollection curMoc = QueryMi("Select * from Win32_PnPEntity where PNPDeviceID = " + usbDeviceAddress + " and PNPClass='SmartCardReader'");
                foreach (ManagementBaseObject device in curMoc)
                {
                    smartCardDevices.Add(device);
                }
            }

            return smartCardDevices;
        }

        /// <summary>
        /// method to get all devices which are connected to USB ports of machine
        /// </summary>
        /// <returns>List of string containing pnpaddress of devices</returns>
        private static IList<string> LookUpUsbDeviceAddresses()
        {
            // this query gets the addressing information for connected USB devices
            ManagementObjectCollection usbDeviceAddressInfo = QueryMi(@"Select * from Win32_USBControllerDevice");

            List<string> usbDeviceAddresses = new List<string>();

            foreach (var device in usbDeviceAddressInfo)
            {
                string curPnpAddress = (string)device.GetPropertyValue("Dependent");
                // split out the address portion of the data; note that this includes escaped backslashes and quotes
                curPnpAddress = curPnpAddress.Split(new String[] { "DeviceID=" }, 2, StringSplitOptions.None)[1];

                usbDeviceAddresses.Add(curPnpAddress);
            }

            return usbDeviceAddresses;
        }

        /// <summary>
        /// run a query against Windows Management Infrastructure (MI) and return the resulting collection
        /// </summary>
        /// <param name="query">WMI Query</param>
        /// <returns>returns objects of devices using management queries</returns>
        private static ManagementObjectCollection QueryMi(string query)
        {
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection result = managementObjectSearcher.Get();

            managementObjectSearcher.Dispose();
            return result;
        }
    }
    public class NfcDevice : ViewModelBase
    {
        private int _id;
        private string _deviceName;
        private string _deviceAddress;
        private string _deviceManufac;


        public NfcDevice()
        {

        }
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }
        public string DeviceName
        {
            get
            {
                return _deviceName;
            }

            set
            {
                _deviceName = value;
                RaisePropertyChanged();
            }
        }
        public string DeviceAddress
        {
            get
            {
                return _deviceAddress;
            }

            set
            {
                _deviceAddress = value;
                RaisePropertyChanged();
            }
        }
        public string DeviceManufac
        {
            get
            {
                return _deviceManufac;
            }

            set
            {
                _deviceManufac = value;
                RaisePropertyChanged();
            }
        }
    }
}
