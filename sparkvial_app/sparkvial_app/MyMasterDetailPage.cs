//using System;
//using System.Collections.Generic;
//using System.Text;
//using Xamarin.Forms;

//[assembly: Xamarin.Forms.ExportRenderer(typeof(Xamarin.Forms.MasterDetailPage), typeof(MyMasterDetailRenderer))]
//namespace sparkvial_app
//{
//    class MyMasterDetailPage : MasterDetailPage {
//        public static readonly BindableProperty DrawerWidthProperty = BindableProperty.Create<MyMasterDetailPage, int>(p => p.DrawerWidth, default(int));

//        public int DrawerWidth {
//            get { return (int)GetValue(DrawerWidthProperty); }
//            set { SetValue(DrawerWidthProperty, value); }
//        }
//    }

//    class MyMasterDetailRenderer : MasterDetailRenderer {
//        bool firstDone;

//        public override void AddView(View child) {
//            if (firstDone) {
//                MyMasterDetailPage page = (MyMasterDetailPage)this.Element;
//                LayoutParams p = (LayoutParams)child.LayoutParameters;
//                p.Width = page.DrawerWidth;
//                base.AddView(child, p);
//            } else {
//                firstDone = true;
//                base.AddView(child);
//            }
//        }

//    }
//}
