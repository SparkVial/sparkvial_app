using sparkvial_app;
using Xamarin.Forms;

[assembly: Dependency(typeof(IBaseUrl))]
namespace sparkvial_app.Droid {
    public class BaseUrl_Android : IBaseUrl {
        public string Get() {
            return "file:///android_asset/";
        }
    }
}