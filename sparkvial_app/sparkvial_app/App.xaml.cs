using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using libsv;
using libsv.devices;
using SkiaSharp;
using sparkvial.rows;
using sparkvial.nodes;
using sparkvial.tapes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using System.Diagnostics;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace sparkvial_app {
    /// <summary>
    /// Binds a string value to a device. Only one should exist per device.
    /// </summary>
    public class DeviceWithValue : BindableObject {
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(
            "Value",
            typeof(string),
            typeof(DeviceWithValue),
            default(string)
        );

        public string Value {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public PeripheralDevice Dev { get; }
        public SourceTape Tape { get; }

        public DeviceWithValue(PeripheralDevice dev) {
            Dev = dev;
            Value = "?";
            Tape = new SourceTape();
        }

        public override bool Equals(object obj) {
            return obj is DeviceWithValue value &&
                   Dev == value.Dev;
        }

        public override int GetHashCode() {
            return Dev.GetHashCode();
        }
    }

    /// <summary>
    /// Binds a string status to an interface. Only one should exist per interface.
    /// </summary>
    class InterfaceWithStatus : ObservableCollection<DeviceWithValue> {
        public string Status { get; set; }

        public Interface Inf { get; }

        public InterfaceWithStatus(Interface inf) {
            Inf = inf;
            Status = "Disabled";
        }

    }

    public partial class App : Application {
        SparkVial sv;
        ListView devList;
        GraphEditor graphEditor;
        ObservableCollection<InterfaceWithStatus> infs = new ObservableCollection<InterfaceWithStatus>();
        Dictionary<Interface, InterfaceWithStatus> infMap = new Dictionary<Interface, InterfaceWithStatus>();
        Dictionary<PeripheralDevice, DeviceWithValue> devMap = new Dictionary<PeripheralDevice, DeviceWithValue>();
        Dictionary<PeripheralDevice, SensorNode> devNodesMap = new Dictionary<PeripheralDevice, SensorNode>();
        private Random pseudoRandomGen = new Random();
        
        public App(SparkVial sv) {
            this.sv = sv;
            Console.WriteLine($"--- SparkVial App v0.1.0 / libsv v{SparkVial.MajorVersion}.{SparkVial.MinorVersion}.{SparkVial.PatchVersion} ---");

            // When an interface is connected enable it, scan it, and create an entry for it in the UI.
            sv.OnInterfaceAdded += (Interface inf) => {
                Console.WriteLine($"- Interface of type {inf.type} added: {inf.id}");

                inf.Enable().ContinueWith((Task<bool> success) => {
                    if (success.Result) {
                        new Thread(() => {
                            sv.ScanInterface(inf);
                        }).Start();
                    }
                });
                
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    var newInf = new InterfaceWithStatus(inf);
                    infMap[inf] = newInf;
                    infMap[inf].Status = "Enabling...";
                    infs.Add(newInf);
                });
            };
            
            // When an interface starts/stops scanning update it on the UI.
            sv.OnInterfaceScanning += async (inf, tsk) => {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    infMap[inf].Status = "Scanning...";
                });
                await tsk;
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                    infMap[inf].Status = "Active";
                });
            };

            // When an interface gets removed update it on the UI.
            sv.OnInterfaceRemoved += (inf) => {
                Console.WriteLine($"- Interface of type {inf.type} removed: {inf.id}");
                infs.Remove(infMap[inf]);
                infMap.Remove(inf);
            };

            // When a device gets added spawn a graph node for it and start sampling it at 10hz.
            sv.OnDeviceAdded += (inf, dev) => {
                Console.WriteLine($"- Device '{dev.name}' added with serial number {dev.uniqueID:X8}");
                if (dev is PeripheralDevice pdev) {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                        devMap[pdev] = new DeviceWithValue(pdev);
                        infMap[inf].Add(devMap[pdev]);
                        var randX = pseudoRandomGen.Next(-200, 200);
                        var randY = pseudoRandomGen.Next(-200, 200);
                        devNodesMap[pdev] = new SensorNode(pdev.Name, "Number", "Units", devMap[pdev].Tape, graphEditor.Graph) {
                            pos = new SKPoint(300 + randX, 200 + randY)
                        };

                        graphEditor.Graph.nodes.Add(devNodesMap[pdev]);
                    });
                }
                dev.AdjustInterval(100 /*ms*/);
            };

            // When a device gets removed remove it's graph node.
            sv.OnDeviceRemoved += (inf, dev) => {
                Console.WriteLine($"- Device '{dev.name}' with serial number {dev.uniqueID:X8} removed");
                if (dev is PeripheralDevice pdev) {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                        graphEditor.Graph.connections.RemoveAll(c => c.sink == devNodesMap[pdev] || c.source == devNodesMap[pdev]);
                        graphEditor.Graph.nodes.Remove(devNodesMap[pdev]);
                        devNodesMap.Remove(pdev);
                        infMap[inf].Remove(devMap[pdev]);
                        devMap.Remove(pdev);
                    });
                }
            };

            // When a sample occurs update it's value on the UI and in the sensor node.
            sv.OnSample += (inf, dev, smp) => {
                //Console.WriteLine($"- Value from {dev.name} ({dev.uniqueID:X8}) with {smp.values.Count} fields at {smp.timestamp}ms:");
                string formattedValue = DeviceLogics.Get(dev.productID).FormatValue(smp);
                
                if (devMap.ContainsKey(dev)) {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                        devMap[dev].Value = formattedValue;
                    });
                    devMap[dev].Tape.Add(smp);
                }
            };

            InitializeComponent();

            MainPage = new MainPage();
            devList = MainPage.FindByName<DevicesPage>("DevPage").FindByName<ListView>("DevList");
            devList.ItemsSource = infs;
            graphEditor = MainPage.FindByName<GraphPage>("GraphPage").FindByName<GraphEditor>("GraphEditor");

            graphEditor.Graph.nodes.Add(new AddNode(graphEditor.Graph) {
                pos = new SKPoint(600, 200)
            });

            graphEditor.Graph.nodes.Add(new ChartNode(graphEditor.Graph) {
                pos = new SKPoint(750, 300)
            });

            graphEditor.Graph.connections.CollectionChanged += ConnectionsChanged;

            new Thread(UIRefresherThread).Start();
        }

        // Orders surface invalidations 
        private void UIRefresherThread() {
            while (true) {
                graphEditor.InvalidateSurface();
                try {
                    var interval = devMap.Keys.Min(d => d.Interval == 0 ? uint.MaxValue : d.Interval);
                    Thread.Sleep((int)interval / 2);
                } catch (InvalidOperationException) {
                    Thread.Sleep(1000);
                }
            };
        }

        // Handle tape connections and disconnections when connections change
        private void ConnectionsChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (var item in e.NewItems) {
                    if (item is GraphConnection con) {
                        if (con.sink is BaseNode sink && sink.Rows[con.sinkRow] is ConnectorRow sinkRow &&
                            con.source is BaseNode source && source.Rows[con.sourceRow] is ConnectorRow sourceRow) {
                            sinkRow.inputTape.sourceTape = sourceRow.outputTape;
                        }
                    }
                }
            } else if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (var item in e.OldItems) {
                    if (item is GraphConnection con) {
                        if (con.sink is BaseNode sink && sink.Rows[con.sinkRow] is ConnectorRow sinkRow &&
                            con.source is BaseNode source && source.Rows[con.sourceRow] is ConnectorRow sourceRow) {
                            sinkRow.inputTape.sourceTape = null;
                        }
                    }
                }
            }
        }

        protected override void OnStart() {
            // Handle when your app starts
            sv.AutoScan = false;
            sv.AutoSample = true;

            new Thread(() => {
                sv.Scan();
            }).Start();
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
            sv.AutoScan = false;
            sv.AutoSample = false;
        }

        protected override void OnResume() {
            // Handle when your app resumes
            new Thread(() => {
                sv.Scan();
            }).Start();
            sv.AutoScan = false;
            sv.AutoSample = true;
        }
    }
}
