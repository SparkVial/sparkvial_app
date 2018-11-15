using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Hardware.Usb;
using Android.App;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using libsv;
using System.Threading.Tasks;
using Android.OS;
using Android.Widget;
using System.Collections.Concurrent;

namespace svifs.android {
    public class AndroidSerialPort : ISerialPort {
        private readonly UsbManager usbManager;
        private readonly UsbSerialPort port;
        private UsbDeviceConnection conn;
        private readonly Context ctx;

        public AndroidSerialPort(UsbManager usbManager, UsbSerialPort port, Context ctx) {
            this.usbManager = usbManager;
            this.port = port;
            this.ctx = ctx;
        }

        public void Close() {
            port.Close();
        }

        public void Flush() {
            try {
                port.PurgeHwBuffers(true, true);
            } catch (Exception) { }
            //new Handler(Looper.MainLooper).Post(() => {
            //    var output = "";
            //    foreach (var b in ba) {
            //        output += $"{b} ";
            //    }
            //    Toast.MakeText(ctx, output, ToastLength.Short).Show();
            //});
        }

        private async void EchoTest() {
            Thread.Sleep(3000);
            while (true) {
                for (int i = 0; i < 256; i++) {
                    WriteByte((byte)i);
                    try {
                        var res = await ReadByte();
                        if (res == i) {
                            continue;
                        } else {
                            new Handler(Looper.MainLooper).Post(() => {
                                Toast.MakeText(ctx, $"Expected {i} but got {res}", ToastLength.Short).Show();
                            });
                            Flush();
                        }

                    } catch (Exception) {
                        new Handler(Looper.MainLooper).Post(() => {
                            Toast.MakeText(ctx, $"Read failed", ToastLength.Short).Show();
                        });
                    }
                }
            }
        }

        public async Task<bool> Open(int baudRate) {
            Console.WriteLine("Getting permission");
            var perm = await usbManager.RequestPermissionAsync(port.Driver.Device, ctx);
            if (perm) {
                try {
                    conn = usbManager.OpenDevice(port.Driver.Device);
                    port.Open(conn);
                    port.SetParameters(baudRate, 8, StopBits.One, Parity.None);
                    port.SetDTR(true);

                    new Handler(Looper.MainLooper).Post(() => {
                        Toast.MakeText(ctx, "Serial connected", ToastLength.Short).Show();
                    });
                } catch (Java.IO.IOException) {
                    // TODO: Handle this
                    throw;
                }

                return true;
            } else {
                return false;
            }
        }

        public Task<byte[]> Read(int count) {
            var output = new byte[count];
            int i = 0;
            while (i < count) {
                var chunk = new byte[count];
                var read = port.Read(chunk, 200);
                chunk.CopyTo(output, i);
                i += read;
            }
            return Task.FromResult(output);
        }

        public async Task<byte> ReadByte() {
            return (await Read(1))[0];
        }

        public void Write(byte[] b) {
            port.Write(b, 100);
        }

        public void WriteByte(byte b) {
            port.Write(new byte[] { b }, 100);
        }
    }

    public class AndroidSerialInterfaceType : InterfaceType {
        Context ctx;
        public AndroidSerialInterfaceType(Context ctx) {
            this.ctx = ctx;
        }

        public override async Task<List<Interface>> Scan() {
            UsbManager usbManager = ctx.GetSystemService(Context.UsbService) as UsbManager;
            var table = UsbSerialProber.DefaultProbeTable;
            table.AddProduct(0x1a86, 0x7523, typeof(Ch34xSerialDriver));

            var prober = new UsbSerialProber(table);
            var drivers = await prober.FindAllDriversAsync(usbManager);
            //var drivers = prober.FindAllDrivers(usbManager);
            var output = new List<Interface>();
            foreach (var driver in drivers) {
                foreach (var port in driver.Ports) {
                    output.Add(new SerialInterface(
                        port.Driver.Device.DeviceName, port.Driver.Device.ProductName + " by " + port.Driver.Device.ManufacturerName,
                        new AndroidSerialPort(usbManager, port, ctx)
                    ));
                }
            }

            return output;
        }
    }

    /*public static class MyExtensions {
        public static void WriteByte(this UsbSerialPort port, byte b) {
            port.Write(new byte[] { b }, 1000);
        }

        public static byte ReadByte(this UsbSerialPort port) {
            var output = new byte[1] { 0xFF };
            port.Read(output, 1000);
            return output[0];
        }

        public static ulong ReadInt(this UsbSerialPort port, int size) {
            var output = new byte[size];
            var realOutput = new byte[8];
            port.Read(output, 1000);
            output.CopyTo(realOutput, 0);
            return BitConverter.ToUInt64(realOutput, 0);
        }

        public static float ReadFloat(this UsbSerialPort port) {
            var output = new byte[4];
            port.Read(output, 1000);
            return BitConverter.ToSingle(output, 0);
        }

        public static ulong ReadVarint(this UsbSerialPort port, byte skipBits, byte initialByte) {
            byte offset = (byte)(7 - skipBits);
            ulong output = (initialByte & ~(0xFFu << offset));

            bool more = (initialByte & (0x80 >> skipBits)) != 0;
            for (int i = 0; i < 9 && more; i++) {
                var next = port.ReadByte();
                output |= ((ulong)next & 0x7F) << offset;
                more = (next & 0x80) != 0;
                offset += 7;
            }

            return output;
        }
    }


    public class SerialInterface : Interface {
        private static string ACTION_USB_PERMISSION = "io.github.sparkvial.sparkvial_app.USB_PERMISSION";
        
        public UsbManager usbManager;
        public Context ctx;
        public UsbSerialPort port;

        //class UsbReciever : BroadcastReceiver {
        //    public override void OnReceive(Context context, Intent intent) {
        //        String action = intent.Action;
        //        if (ACTION_USB_PERMISSION.Equals(action)) {
        //            lock (this) {
        //                UsbDevice device = (UsbDevice)intent
        //                        .GetParcelableExtra(UsbManager.ExtraDevice);

        //                if (intent.GetBooleanExtra(
        //                        UsbManager.ExtraPermissionGranted, false)) {
        //                    if (device != null) {
        //                        var connection = usbManager.OpenDevice(port.Driver.Device);
        //                        port.Open(connection);
        //                        Thread.Sleep(2500);
        //                        port.PurgeHwBuffers(true, true);
                                
        //                        // call method to set up device communication
        //                    }
        //                } else {

        //                }
        //            }
        //        }
        //    }
        //}
        //private SerialInputOutputManager ser;
        public override ulong BacklogSize => throw new NotImplementedException();

        public SerialInterface(string portName, string portInfo, UsbManager usbManager, UsbSerialPort port, Context ctx) : base(portName, portInfo, "Serial") {
            this.usbManager = usbManager;
            this.port = port;
            this.ctx = ctx;
        }

        public override void AdjustInterval(ulong index, uint interval) {
            //throw new NotImplementedException();
        }

        public override void Disable() {
            //throw new NotImplementedException();
        }

        public override void Enable() {
            // TODO: Check if port is already open

            //var connection = usbManager.OpenDevice(port.Driver.Device);
            //port.Open(connection);
            //Thread.Sleep(2500);
            //port.PurgeHwBuffers(true, true);
            //isEnabled = true;
            //Console.WriteLine($"{this.id} Enabled");
            
            Console.WriteLine("Getting permission");
            //UsbReciever usbReciever = new UsbReciever();
            //PendingIntent mPermissionIntent = PendingIntent.GetBroadcast(ctx, 0, new Intent(
            //    ACTION_USB_PERMISSION), 0);
            //IntentFilter filter = new IntentFilter(ACTION_USB_PERMISSION);
            //ctx.RegisterReceiver(usbReciever, filter);

            //usbManager.RequestPermission(port.Driver.Device, mPermissionIntent);

            var req = usbManager.RequestPermissionAsync(port.Driver.Device, ctx);
            req.Wait();
            var connection = usbManager.OpenDevice(port.Driver.Device);
            port.Open(connection);
            port.SetParameters(115200, 8, StopBits.One, Parity.None);
            Thread.Sleep(2500);
            port.PurgeHwBuffers(true, true);
            isEnabled = true;
            new Handler(Looper.MainLooper).Post(() => {
                Toast.MakeText(ctx, "Serial Enabled", ToastLength.Short).Show();
            });

            //Thread.Sleep(5000);


            //var permissionGranted = await usbManager.RequestPermissionAsync(selectedPort.Driver.Device, this);
            //if (permissionGranted) {
            //    // start the SerialConsoleActivity for this device
            //    var newIntent = new Intent(this, typeof(SerialConsoleActivity));
            //    newIntent.PutExtra(SerialConsoleActivity.EXTRA_TAG, new UsbSerialPortInfo(selectedPort));
            //    StartActivity(newIntent);
            //}

            //port = new SerialPort(id, 115200);
            //try {
            //    port.Open();
            //    Thread.Sleep(2500);
            //    port.ReadExisting();
            //} catch (InvalidOperationException) { } catch (System.IO.IOException) { }
            //isEnabled = true;
            //Console.WriteLine($"{port.PortName} Enabled");
        }

        public override bool Heartbeat() {
            throw new NotImplementedException();
        }

        public override List<Sample> ReadStream(byte readSize) {
            //throw new NotImplementedException();
            return new List<Sample> { };
        }

        public override List<Device> Scan() {
            if (isEnabled) {
                var output = new List<Device>();

                //if (timestampSize > 0b11) {
                //    throw new ArgumentException($"timestampSize cannot be higher than {0b11}.");
                //}

                //if (timestampResolution > 0b1111) {
                //    throw new ArgumentException($"timestampResolution cannot be higher than {0b1111}.");
                //}

                //try {
                    port.PurgeHwBuffers(true, true); // Flush previous output
                    //port.WriteByte((byte)((byte)Command.Scan | timestampSize));
                    //port.WriteByte((byte)(timestampResolution & 0xF));
                    port.Write(new byte[] { 1 << 5 | 2 }, 1000);
                    port.Write(new byte[] { 1 }, 1000);
                    Thread.Sleep(200);
                    uint index_counter = 0;
                    byte moreDevs;
                    do {
                        //Console.WriteLine("> Next device");
                        byte devClass = moreDevs = port.ReadByte();

                        devClass &= 1;

                        if ((moreDevs & 0x80) != 0) {
                            new Handler(Looper.MainLooper).Post(() => {
                                Toast.MakeText(ctx, $"Moredevs: {moreDevs}", ToastLength.Short).Show();
                            });
                            var productID = new byte[4];
                            var uniqueID = new byte[4];
                            port.Read(productID, 1000);
                            Console.WriteLine(BitConverter.ToUInt32(productID, 0));
                            port.Read(uniqueID, 1000);

                            //Console.WriteLine("> Read idents");

                            if (devClass == (byte)DeviceClass.Buffer) {
                                var shlAmount = port.ReadByte();
                                var bufferSize = port.ReadByte() | (port.ReadByte() << 8);

                                output.Add(new BufferDevice(
                                    this,
                                    BitConverter.ToUInt32(productID, 0),
                                    BitConverter.ToUInt32(uniqueID, 0),
                                    index_counter++,
                                    (ulong)bufferSize << shlAmount
                                ));

                                throw new NotImplementedException("Buffer devices are not implemented yet");
                            } else {
                                var fieldCount = port.ReadByte();
                                //Console.WriteLine($"> There are {fieldCount} fields");
                                var fields = new List<FieldType>();
                                for (int i = 0; i < fieldCount; i += 4) {
                                    var currentFieldTypes = port.ReadByte();
                                    for (int j = i; j < Math.Min(i + 4, fieldCount); j++) {
                                        //Console.WriteLine($"> New field {currentFieldTypes & 0b11}");
                                        fields.Add((FieldType)(currentFieldTypes & 0b11));
                                        currentFieldTypes >>= 2;
                                    }
                                }

                                output.Add(new PeripheralDevice(
                                    this,
                                    BitConverter.ToUInt32(productID, 0),
                                    BitConverter.ToUInt32(uniqueID, 0),
                                    index_counter++,
                                    fields
                                ));
                            }

                        }
                    } while (moreDevs != 0);

                //} catch (InvalidOperationException) { };
                return output;
            } else {
                return new List<Device> { };
            }
        }

        public override void Write(ulong idx, byte[] value) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            if (!(obj is SerialInterface item)) {
                return false;
            }

            return id.Equals(item.id);
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }
    }*/
}
