using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace sparkvial_app {
    public partial class GraphPage : ContentPage {
        public GraphPage() {
            InitializeComponent();
            //NavigationPage.SetHasNavigationBar(this, false);
        }

        public void OnTouch(object sender, SKTouchEventArgs e) {
            this.FindByName<GraphEditor>("GraphEditor").OnInteract(sender, e);
        }

        private void OnPan(object sender, PanUpdatedEventArgs e) {
            this.FindByName<GraphEditor>("GraphEditor").OnPan(sender, e);
        }

        private void OnPinch(object sender, PinchGestureUpdatedEventArgs e) {
            this.FindByName<GraphEditor>("GraphEditor").OnPinch(sender, e);
        }
    }
}