using System;
using System.Diagnostics;
using libsv.devices;
using Xamarin.Forms;

namespace sparkvial_app {
    public partial class DevicesPage : ContentPage {
        public DevicesPage() {
            InitializeComponent();
            //NavigationPage.SetHasNavigationBar(this, false);
        }

        public void Rescan(object sender, EventArgs e) {
            Console.WriteLine("refresh");
        }

        private void DevList_ItemTapped(object sender, ItemTappedEventArgs e) {
            if (e.Item is DeviceWithValue dev) {
                Debug.Print(DeviceLogics.Get(dev.Dev.ProductID) + "\n");
            }
        }
    }
}