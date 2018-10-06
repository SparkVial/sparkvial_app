using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace libsv {
    public class SparkVial {
        public static readonly uint Version = 0; // even version = unstable

        public static readonly Dictionary<uint, string> productDB = new Dictionary<uint, string>() {
            { 0, "MISSING PRODUCT ID" },
            { 1, "Example controller" },
            { 2, "Example device" },
        };

        public SparkVial(List<InterfaceType> ifTypes) {
            interfaceTypes = ifTypes;
            new Thread(ScanningThread).Start();
            new Thread(SampleThread).Start();
        }

        public void Scan() {
            throw new NotImplementedException();
        }

        public void GetValue() {
            throw new NotImplementedException();
        }
        
        void ScanningThread() {
            while (true) {
                lock (autoLock) {
                    if (autoScan) {
                        foreach (var interfaceType in interfaceTypes) {
                            var ifScan = interfaceType.Scan();
                            var newIfs = ifScan.Where(i => !interfaceType.interfaces.Contains(i));
                            var removedIfs = interfaceType.interfaces.Where(i => !ifScan.Contains(i)).ToList();
                            foreach (var i in newIfs) {
                                interfaceType.interfaces.Add(i);
                                OnInterfaceAdded(i);
                            }
                            foreach (var i in removedIfs) {
                                interfaceType.interfaces.Remove(i);
                                OnInterfaceRemoved(i);
                            }

                            foreach (var inf in interfaceType.interfaces) {
                                if (inf.isEnabled) {
                                    var devScan = inf.Scan();
                                    var newDevs = devScan.Where(d => !inf.devices.Contains(d));
                                    var removedDevs = inf.devices.Where(d => !devScan.Contains(d)).ToList();
                                    foreach (var d in newDevs) {
                                        inf.devices.Add(d);
                                        OnDeviceAdded(d);
                                    }
                                    foreach (var d in removedDevs) {
                                        inf.devices.Remove(d);
                                        OnDeviceRemoved(d);
                                    }
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(3000);
            }
        }

        void SampleThread() {
            while (true) {
                lock (autoLock) {
                    if (autoSample) {
                        foreach (var interfaceType in interfaceTypes) {
                            foreach (var inf in interfaceType.interfaces) {
                                if (inf.isEnabled && inf.devices.Count() > 0) {
                                    //foreach (var dev in inf.devices) {
                                    //    dev.GetValue();
                                    //}
                                    var devVals = inf.devices.Zip(inf.GetValue(1), (dev, val) => new { dev, val });
                                    foreach (var it in devVals) {
                                        if (it.val.Length > 0)
                                            OnDeviceValue(it.dev, it.val);

                                        if (it.val.Length == 4)
                                            OnDeviceValueFloat(it.dev, BitConverter.ToSingle(it.val, 0));
                                    }
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }

        public delegate void OnInterfaceAddedDelegate(Interface inf);
        public OnInterfaceAddedDelegate OnInterfaceAdded = (i) => { };
        public delegate void OnInterfaceRemovedDelegate(Interface inf);
        public OnInterfaceRemovedDelegate OnInterfaceRemoved = (i) => { };
        public delegate void OnDeviceAddedDelegate(Device dev);
        public OnDeviceAddedDelegate OnDeviceAdded = (d) => { };
        public delegate void OnDeviceRemovedDelegate(Device dev);
        public OnDeviceRemovedDelegate OnDeviceRemoved = (d) => { };
        public delegate void OnDeviceValueDelegate(Device dev, byte[] value);
        public OnDeviceValueDelegate OnDeviceValue = (d, v) => { };
        public delegate void OnDeviceValueFloatDelegate(Device dev, float value);
        public OnDeviceValueFloatDelegate OnDeviceValueFloat = (d, v) => { };

        public List<InterfaceType> interfaceTypes;

        private readonly object autoLock = new object();
        private bool autoScan = false;

        public bool AutoScan {
            get { lock (autoLock) { return autoScan; } }
            set { lock (autoLock) { autoScan = value; } }
        }

        private bool autoSample = false;
        public bool AutoSample {
            get { lock (autoLock) { return autoSample; } }
            set { lock (autoLock) { autoSample = value; } }
        }
    }
}
