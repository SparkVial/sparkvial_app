using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;
using libsv;
using svifs.win32;
using Xamarin.Forms.Platform.WPF.Controls;

namespace sparkvial_app.WPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage {
        public MainWindow() {
            InitializeComponent();

            //var titleBg = base.GetType().GetProperty("TitleBarBackgroundColorProperty", BindingFlags.Public | BindingFlags.Instance);
            //var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            //var brush = new Brus(System.Windows.Media.Color.FromRgb(255, 255, 255));
            //(this as FormsWindow).SetValue(TitleBarBackgroundColorProperty, Brushes.Beige);
            //Console.WriteLine(setMethod.GetGenericArguments());
            //if (setMethod != null) {
            //setMethod.Invoke(titleBg, new object[] { brush });
            //} else {
            //throw new Exception("!!!");
            //}
            //privMethod.Invoke(objInstance, new object[] { methodParameters });

            Forms.Init();
            LoadApplication(new sparkvial_app.App(new SparkVial(new List<InterfaceType> {
                new Win32SerialInterfaceType(),
                new MockInterfaceType()
            })));

            //Console.WriteLine(TitleBarBackgroundColor == null);
            //Console.WriteLine(TitleBarBackgroundColorProperty == null);
            //TitleBarBackgroundColor.SetValue(TitleBarBackgroundColorProperty, brush);
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);
            Border borderWindow = (Border)Template.FindName("BorderWindow", this);
            TextBlock systemTitle = (TextBlock)Template.FindName("PART_System_Title", this);
            FormsAppBar appbar = (FormsAppBar)Template.FindName("PART_TopAppBar", this);
            System.Windows.Controls.Grid windowbar = (System.Windows.Controls.Grid)Template.FindName("PART_CommandsBar", this);
            System.Windows.Controls.Button hamburger = (System.Windows.Controls.Button)Template.FindName("PART_Hamburger", this);
            windowbar.Height = 35;
            appbar.Height = 35;
            hamburger.Height = 35;
            Panel.SetZIndex(hamburger, 11);
            //systemTitle.Opacity = 0;
            //hamburger.Content
            appbar.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 41, 41));
            windowbar.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 41, 41, 41));
            borderWindow.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(120, 120, 120));
            //windowbar.Margin = new System.Windows.Thickness(53, 0, 0, -35);
            appbar.Padding = new System.Windows.Thickness(0, 0, 135, 0);
            Panel.SetZIndex(windowbar, 10);
            //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(appbar); i++) {
            //    Console.WriteLine((VisualTreeHelper.GetChild(appbar, i) as UIElement).GetType());
            //    Console.WriteLine((VisualTreeHelper.GetChild(appbar, i) as UIElement).RenderSize);
            //}
            Console.WriteLine(VisualTreeHelper.GetChildrenCount(appbar));
            /*
            <color name="launcher_background">#FFFFFF</color>
            <color name="colorPrimary">#292929</color>
            <color name="colorPrimaryDark">#292929</color>
            <color name="colorAccent">#FF7B0F</color>
            */
            //System.Windows.Controls.Border border = (System.Windows.Controls.Border)this.Template.FindName("BorderWindow", this);
            //border.BorderThickness = new System.Windows.Thickness(0);
        }
    }
}
