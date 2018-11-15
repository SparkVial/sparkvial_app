using TouchTracking;
using Xamarin.Forms;

namespace sparkvial_app {
    public partial class GraphPage : ContentPage {
        public GraphPage() {
            InitializeComponent();
            //NavigationPage.SetHasNavigationBar(this, false);
        }

        public void OnTouch(object sender, TouchActionEventArgs args) {
            this.FindByName<GraphEditor>("GraphEditor").OnInteract(sender, args);
        }
    }
}