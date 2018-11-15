using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

namespace libsv {
    public class SparkVial {
        public const uint MajorVersion = 0;
        public const uint MinorVersion = 1;
        public const uint PatchVersion = 0;
        public const byte ProtocolVersion = 0;

        public static readonly Dictionary<uint, string> productDB = new Dictionary<uint, string>() {
            { 0, "MISSING PRODUCT ID" },
            { 0 << 16 | 1, "Mock controller" },
            { 0 << 16 | 2, "Mock device" },
            { 1 << 16 | 1, "Example controller" },
            { 1 << 16 | 2, "Example device" },
        };

        public Thread scanningThread;
        public Thread sampleThread;

        public SparkVial(List<InterfaceType> ifTypes) {
            interfaceTypes = ifTypes;
        }

        private async void HeartbeatInterface(Interface inf) {
            if (inf.isEnabled) {
                await inf.Heartbeat();
            }
        }

        public void ScanInterface(Interface inf) {
            if (inf.isEnabled) {
                var delegateScanTask = new TaskCompletionSource<bool>();
                OnInterfaceScanning(inf, delegateScanTask.Task);
                Console.WriteLine($"Scanning {inf.info}");

                var scanTask = inf.Scan();
                scanTask.Wait(3000);

                delegateScanTask.SetResult(scanTask.IsCompleted);
                if (scanTask.IsCompleted) {
                    var devScan = scanTask.Result;
                    Console.WriteLine($"Scanning {inf.info} done");
                    var newDevs = devScan.Where(d => !inf.devices.Contains(d));
                    var removedDevs = inf.devices.Where(d => !devScan.Contains(d)).ToList();
                    foreach (var d in newDevs) {
                        inf.devices.Add(d);
                        OnDeviceAdded(inf, d);
                    }
                    foreach (var d in removedDevs) {
                        inf.devices.Remove(d);
                        OnDeviceRemoved(inf, d);
                    }
                } else {
                    Console.WriteLine($"Scanning {inf.info} timed out, retrying");
#pragma warning disable CS4014
                    Task.Run(async () => {
                        await inf.Disable();
                        await inf.Enable();
                    });
#pragma warning restore CS4014
                }
            }
        }

        public async void Scan() {
            foreach (var interfaceType in interfaceTypes) {
                Console.WriteLine($"Scanning {interfaceType.ToString()}");
                var ifScan = await interfaceType.Scan();
                var newIfs = ifScan.Where(i => !interfaceType.interfaces.Contains(i));
                var removedIfs = interfaceType.interfaces.Where(i => !ifScan.Contains(i)).ToList();
                var keptIfs = ifScan.Where(i => interfaceType.interfaces.Contains(i));

                foreach (var i in newIfs) {
                    interfaceType.interfaces.Add(i);
                    OnInterfaceAdded(i);
                }
                foreach (var i in removedIfs) {
                    interfaceType.interfaces.Remove(i);
                    OnInterfaceRemoved(i);
                }

                foreach (var inf in interfaceType.interfaces) {
                    ScanInterface(inf);
                }
            }
        }
        
        void ScanningThreadFunc() {
            while (autoScan) {
                Scan();
                Thread.Sleep(3000);
            }
        }

        async void SampleThreadFunc() {
            while (autoSample) {
                foreach (var interfaceType in interfaceTypes) {
                    foreach (var inf in interfaceType.interfaces) {
                        if (inf.isEnabled && inf.devices.Count() > 0) {
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            var samples = await inf.ReadStream(31);
                            sw.Stop();
                            //Console.WriteLine(sw.Elapsed);
                            foreach (var sample in samples) {
                                OnSample(inf, inf.devices[(int)sample.idx] as PeripheralDevice, sample);
                            }
                        }
                    }
                }
                Thread.Sleep(30);
            }
        }

        public delegate void OnInterfaceAddedDelegate(Interface inf);
        public OnInterfaceAddedDelegate OnInterfaceAdded = (i) => { };
        public delegate void OnInterfaceScanningDelegate(Interface inf, Task scanEnded);
        public OnInterfaceScanningDelegate OnInterfaceScanning = (i, t) => { };
        public delegate void OnInterfaceRemovedDelegate(Interface inf);
        public OnInterfaceRemovedDelegate OnInterfaceRemoved = (i) => { };
        public delegate void OnDeviceAddedDelegate(Interface inf, Device dev);
        public OnDeviceAddedDelegate OnDeviceAdded = (i, d) => { };
        public delegate void OnDeviceRemovedDelegate(Interface inf, Device dev);
        public OnDeviceRemovedDelegate OnDeviceRemoved = (i, d) => { };
        public delegate void OnSampleDelegate(Interface inf, PeripheralDevice dev, Sample value);
        public OnSampleDelegate OnSample = (i, d, v) => { };

        public List<InterfaceType> interfaceTypes;

        private readonly object autoLock = new object();
        private bool autoScan = false;

        public bool AutoScan {
            get { lock (autoLock) { return autoScan; } }
            set {
                lock (autoLock) {
                    if (!autoScan && value) {
                        if (scanningThread != null) {
                            scanningThread.Abort();
                        }
                        scanningThread = new Thread(ScanningThreadFunc);
                        scanningThread.Start();
                    }
                    autoScan = value;
                }
            }
        }

        private bool autoSample = false;
        public bool AutoSample {
            get { lock (autoLock) { return autoSample; } }
            set {
                lock (autoLock) {
                    if (!autoSample && value) {
                        if (sampleThread != null) {
                            sampleThread.Abort();
                        }
                        sampleThread = new Thread(SampleThreadFunc);
                        sampleThread.Start();
                    }
                    autoSample = value;
                }
            }
        }
    }
}
