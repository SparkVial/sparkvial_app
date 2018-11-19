using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using libsv;
using Windows.UI.ViewManagement;
using Xamarin.Forms.Platform.UWP;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Xamarin.Forms;
using svifs.uwp;

namespace sparkvial_app.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            // FIXME
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            //var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            //titleBar.BackgroundColor = Colors.Transparent;
            //titleBar.InactiveBackgroundColor = Colors.Transparent;
            //titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            //titleBar.ButtonBackgroundColor = Colors.Transparent;

            //titleBar.BackgroundColor = Windows.UI.Color.FromArgb(255, 41, 41, 41);
            //titleBar.InactiveBackgroundColor = Windows.UI.Color.FromArgb(255, 65, 65, 65);
            //titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(255, 41, 41, 41);
            //titleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(255, 65, 65, 65);
            //titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(255, 65, 65, 65);
            //titleBar.ButtonHoverForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            //titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(255, 80, 80, 80);
            //titleBar.ButtonPressedForegroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);

            var app = new sparkvial_app.App(new SparkVial(new List<InterfaceType> {
                new UWPSerialInterfaceType(),
                new MockInterfaceType()
            }));

            LoadApplication(app);
            //var masterDetailPage = app.MainPage as Xamarin.Forms.MasterDetailPage;
            //var masterPage = masterDetailPage.Master;
            //var renderer = Platform.GetRenderer(masterPage) as PageRenderer;
            //Console.WriteLine(renderer);
            //var acrylicBrush = new Windows.UI.Xaml.Media.AcrylicBrush() {
            //    BackgroundSource = AcrylicBackgroundSource.Backdrop,
            //    TintColor = Windows.UI.Color.FromArgb(255, 40, 40, 40),
            //    FallbackColor = Windows.UI.Color.FromArgb(255, 40, 40, 40),
            //    TintOpacity = 0.6
            //};

            //renderer.Background = acrylicBrush;

            //var cmdbar = app.MainPage.FindByName<FormsCommandBar>("CommandBar");
            //cmdbar.Margin = new Windows.UI.Xaml.Thickness(10, 10, 10, 10);

            //app.PageAppearing += (object sender, Xamarin.Forms.Page e) => {
            //    var cmdBar1 = VisualTreeHelper.GetChild(this, 0);
            //    var cmdBar2 = VisualTreeHelper.GetChild(cmdBar1, 0) as MasterDetailControl;
            //    var cmdBar3 = cmdBar2.Detail.Parent
            //    ;
            //};

            //var cmdBar1 = VisualTreeHelper.GetChild(this, 0) as Canvas;
            //var cmdBar2 = VisualTreeHelper.GetChild(cmdBar1, 0) as MasterDetailControl;
            //var styl = new Windows.UI.Xaml.Style(typeof(MasterDetailControl));

            //styl.BasedOn = Xamarin.Forms.Platform.UWP.MasterDetailControl;
            //styl.Setters.Add(new Windows.UI.Xaml.Setter(MasterDetailControl.ToolbarBackgroundProperty, new SolidColorBrush(Windows.UI.Color.FromArgb(255, 127, 0, 64))));
            //cmdBar2.Style = styl;

            //cmdBar2.MasterTitleVisibility = true;
            //var cmdBar3 = cmdBar2.FindName("SplitView");
            //var cmdBar4 = VisualTreeHelper.GetChild(cmdBar2, 0);
            //cmdBar = VisualTreeHelper.GetChild(cmdBar, 0);
            //cmdBar = VisualTreeHelper.GetChild(cmdBar, 1);
            //cmdBar = VisualTreeHelper.GetChild(cmdBar, 0);
            //cmdBar = VisualTreeHelper.GetChild(cmdBar, 0);
            //cmdBar = VisualTreeHelper.GetChild(cmdBar, 1);
            //cmdBar = VisualTreeHelper.GetChild(cmdBar, 0);

            //app.MainPage.BackgroundColor = Xamarin.Forms.Color.Tomato;
        }
    }
}
