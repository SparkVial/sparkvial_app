using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using libsv;
using libsv.devices;
using SkiaSharp;
using sparkvial_app.nodes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace sparkvial_app {
    class DeviceWithStringValue : BindableObject {
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(
            "Value",
            typeof(string),
            typeof(DeviceWithStringValue),
            default(string)
        );

        public string Value {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PeripheralDevice Dev { get; }

        public DeviceWithStringValue(PeripheralDevice dev) {
            this.Dev = dev;
            this.Value = "unk";
        }

    }

    class InterfaceWithStatus : ObservableCollection<DeviceWithStringValue> {
        //public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        //    "Status",
        //    typeof(string),
        //    typeof(InterfaceWithStatus),
        //    default(string)
        //);

        public string Status { get; set; }

        public Interface Inf { get; }
        //public ObservableCollection<DeviceWithStringValue> Devs { get; } = new ObservableCollection<DeviceWithStringValue>();

        public InterfaceWithStatus(Interface inf) {
            this.Inf = inf;
            this.Status = "Disabled";
        }

    }

    public partial class App : Application {
        SparkVial sv;
        ListView devList;
        GraphEditor graphEditor;
        ObservableCollection<InterfaceWithStatus> infs = new ObservableCollection<InterfaceWithStatus>();
        Dictionary<Interface, InterfaceWithStatus> infs_map = new Dictionary<Interface, InterfaceWithStatus>();
        
        public App(SparkVial sv) {
            this.sv = sv;
            Console.WriteLine($"--- SparkVial App v0.1.0 / libsv v{SparkVial.MajorVersion}.{SparkVial.MinorVersion}.{SparkVial.PatchVersion} ---");
            sv.OnInterfaceAdded += (Interface inf) => {
                Console.WriteLine($"- Interface of type {inf.type} added: {inf.id}");
                inf.Enable().ContinueWith((Task<bool> success) => {
                    if (success.Result) {
                        //Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                        //    MainPage.DisplayAlert("Device enabled", $"Interface of type {inf.type} added and enabled: {inf.id}", "Dismiss");
                        //});

                        new Thread(() => {
                            sv.ScanInterface(inf);
                        }).Start();
                    }
                });

                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    var newInf = new InterfaceWithStatus(inf);
                    infs_map[inf] = newInf;
                    infs.Add(newInf);
                    infs_map[inf].Status = "Enabling...";
                });
            };
            sv.OnInterfaceScanning += async (inf, tsk) => {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    infs_map[inf].Status = "Scanning...";
                });
                await tsk;
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    infs_map[inf].Status = "Active";
                });
            };
            sv.OnInterfaceRemoved += (inf) => {
                Console.WriteLine($"- Interface of type {inf.type} removed: {inf.id}");
            };
            sv.OnDeviceAdded += (inf, dev) => {
                Console.WriteLine($"- Device '{dev.name}' added with serial number {dev.uniqueID:X8}");
                if (dev is PeripheralDevice pdev) {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                        infs_map[inf].Add(new DeviceWithStringValue(pdev));
                    });
                }
                dev.AdjustInterval(100);  // in ms
            };
            sv.OnDeviceRemoved += (inf, dev) => {
                Console.WriteLine($"- Device '{dev.name}' with serial number {dev.uniqueID:X8} removed");
                if (dev is PeripheralDevice pdev) {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                        infs_map[inf].Remove(new DeviceWithStringValue(pdev));
                    });
                }
            };
            sv.OnSample += (inf, dev, smp) => {
                //Console.WriteLine($"- Value from {dev.name} ({dev.uniqueID:X8}) with {smp.values.Count} fields at {smp.timestamp}ms:");
                string formattedValue = DeviceLogics.Get(dev.productID).FormatValue(smp);
                
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    infs_map[inf][(int)dev.index].Value = formattedValue;
                });
            };

            InitializeComponent();

            MainPage = new MainPage();
            devList = MainPage.FindByName<DevicesPage>("DevPage").FindByName<ListView>("DevList");
            devList.ItemsSource = infs;
            graphEditor = MainPage.FindByName<GraphPage>("GraphPage").FindByName<GraphEditor>("GraphEditor");

            //var chartNode = new ChartNode() {
            //    data = new Queue<float>(new float[] { 1, 2, 3, 4, 5, 4, 3, 2, 1 })
            //};

            graphEditor.Graph.nodes.Add(new SensorNode("Temperature Sensor", "Number", "C", graphEditor.Graph) {
                pos = new SKPoint(300, 170)
            });

            graphEditor.Graph.nodes.Add(new SensorNode("Light Sensor", "Number", "Lux", graphEditor.Graph) {
                pos = new SKPoint(300, 230)
            });

            graphEditor.Graph.nodes.Add(new AddNode(graphEditor.Graph) {
                pos = new SKPoint(600, 200)
            });

            graphEditor.Graph.nodes.Add(new ChartNode(graphEditor.Graph) {
                pos = new SKPoint(750, 300)
            });

            //graphEditor.Graph.connections.Add(new GraphConnection() {
            //    source = graphEditor.Graph.nodes[0],
            //    sourceRow = 0,
            //    sink = graphEditor.Graph.nodes[2],
            //    sinkRow = 0
            //});

            //graphEditor.Graph.connections.Add(new GraphConnection() {
            //    source = graphEditor.Graph.nodes[1],
            //    sourceHandle = 0,
            //    sink = graphEditor.Graph.nodes[2],
            //    sinkHandle = 1
            //});

            //graphEditor.Graph.connections.Add(new GraphConnection() {
            //    source = graphEditor.Graph.nodes[2],
            //    sourceHandle = 2,
            //    sink = graphEditor.Graph.nodes[3],
            //    sinkHandle = 0
            //});
        }

        protected override void OnStart() {
            // Handle when your app starts
            sv.AutoScan = false;
            sv.AutoSample = true;

            new Thread(() => {
                sv.Scan();
            }).Start();
            //Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
            //    devs.Add(new DeviceWithStringValue(new PeripheralDevice(null, 1 << 16 | 2, 1, 0, null)));
            //});
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
            sv.AutoScan = false;
            sv.AutoSample = false;
        }

        protected override void OnResume() {
            // Handle when your app resumes
            //sv.Scan();

            new Thread(() => {
                sv.Scan();
            }).Start();
            sv.AutoScan = false;
            sv.AutoSample = true;
        }
    }
}
