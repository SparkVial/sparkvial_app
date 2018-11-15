using System;
using System.Collections.Generic;
using libsv;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using svifs.android;
using Android.Hardware.Usb;

[assembly: UsesFeature("android.hardware.usb.host")]
namespace sparkvial_app.Droid
{
    [Activity(Label = "SparkVial", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            
            LoadApplication(new App(new SparkVial(new List<InterfaceType> {
                new AndroidSerialInterfaceType(this),
                new MockInterfaceType()
            })));
        }
    }
}