using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using libsv;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace svifs.uwp {
    public class UWPSerialPort : ISerialPort {
        private readonly string portID;
        private SerialDevice port;
        private DataReader reader;
        private DataWriter writer;

        public UWPSerialPort(string id) {
            portID = id;
        }

        public void Close() {
            port.Dispose();
            reader.DetachStream();
            reader.Dispose();
            writer.DetachStream();
            writer.Dispose();
        }

        public void Flush() {
            Debug.Print("Flushing\n");
            //port.ReadTimeout = new TimeSpan(5000);
            try {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500))) {
                    reader.LoadAsync(1024).AsTask(cts.Token).Wait();
                }
                //Task.Run(async () => {
                //    while (true) {
                //        await ReadByte();
                //        Debug.Print("Flushed 1 byte\n");
                //    }
                //});
            } catch (AggregateException) { }
            catch (AccessViolationException) {
                throw new PortClosedException();
            }
            reader.ReadBuffer(reader.UnconsumedBufferLength);
        }

        public async Task<bool> Open(int baudRate) {
            try {
                port = await SerialDevice.FromIdAsync(portID);
                port.BaudRate = (uint) baudRate;
                //port.IsRequestToSendEnabled = true;
                port.IsDataTerminalReadyEnabled = true;
                reader = new DataReader(port.InputStream);
                writer = new DataWriter(port.OutputStream);
                return true;
            } catch (Exception) { // FIXME: Make exception specific
                return false;
            }
        }

        public async Task<byte[]> Read(int count) {
            var output = new byte[count];
            try {
                await reader.LoadAsync((uint)count);
            } catch (Exception) { }
            reader.ReadBytes(output);

            //Debug.Print("\nRead: ");
            //foreach (var b in output) {
            //    Debug.Print($"{b:x2} ");
            //}

            return output;
        }

        public async Task<byte> ReadByte() {
            byte output;
            try {
                await reader.LoadAsync(1);
            } catch (Exception) { }
            output = reader.ReadByte();

            //Debug.Print("\nRead byte: ");
            //Debug.Print($"{output:x2} ");

            return output;
        }

        public void Write(byte[] bytes) {
            writer.ByteOrder = ByteOrder.LittleEndian;
            //Debug.Print("\nWrite: ");
            //foreach (var b in bytes) {
            //    Debug.Print($"{b:x2} ");
            //}

            writer.WriteBytes(bytes);
            writer.StoreAsync().AsTask().Wait();
            //writer.
            //writer.FlushAsync().AsTask().Wait();
        }

        public void WriteByte(byte b) {
            // FIXME
            Write(new byte[] { b });
        }
    }

    public class UWPSerialInterfaceType : InterfaceType {
        public override async Task<List<Interface>> Scan() {
            var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            List<Interface> interfaces = devices.Select(dev => {
                var portID = (dev.Name as string).Split('(').Last().TrimEnd(')');
                return (Interface)new SerialInterface(portID, dev.Name, new UWPSerialPort(dev.Id));
            }).ToList();
            return interfaces;
        }
    }
}
