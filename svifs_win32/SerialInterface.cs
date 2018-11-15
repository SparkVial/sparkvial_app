using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using libsv;
using System.Threading.Tasks;

namespace svifs.win32 {
    public class Win32SerialPort : ISerialPort {
        SerialPort port;
        readonly string portID;
        public Win32SerialPort(string portID) {
            this.portID = portID;
        }

        public void WriteByte(byte b) {
            Write(new byte[] { b });
        }

        public Task<bool> Open(int baudRate) {
            if (port != null && port.IsOpen) {
                port.Close();
            }
            port = new SerialPort(portID, baudRate);
            try {
                port.Open();
                return Task.FromResult(true);
            } catch (InvalidOperationException) { } catch (System.IO.IOException) { }
            return Task.FromResult(false);
        }

        public void Close() {
            port.Close();
        }

        public void Flush() {
            try {
                port.ReadExisting();
            } catch (Exception) { }
        }

        public void Write(byte[] b) {
            port.Write(b, 0, b.Length);
        }

        public Task<byte[]> Read(int count) {
            var output = new byte[count];
            port.Read(output, 0, count);
            return Task.FromResult(output);
        }

        public async Task<byte> ReadByte() {
            return (await Read(1))[0];
        }
    }

    public class Win32SerialInterfaceType : InterfaceType {
        public override Task<List<Interface>> Scan() {
            List<Interface> interfaces = new List<Interface>();
            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"")) {
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var portInfo = (
                    from p in ports
                    select new string[] {
                        (p["Caption"] as string).Split('(').Last().TrimEnd(')'),
                        p["Name"] + " by " + p["Manufacturer"]
                    }
                ).ToList();
                interfaces = (
                    from p in portInfo
                    select (Interface)new SerialInterface(p[0], p[1], new Win32SerialPort(p[0]))
                ).ToList();
            }
            return Task.FromResult(interfaces);
        }
    }
    /*public class SerialInterface : Interface {
        private SerialPort port;

        public override ulong BacklogSize => throw new NotImplementedException();

        public SerialInterface(string port, string portInfo) : base(port, portInfo, "Serial") { }

        public override void Enable() {
            if (port != null && port.IsOpen) {
                port.Close();
            }
            port = new SerialPort(id, 115200);
            try {
                port.Open();
                Thread.Sleep(2500);
                port.ReadExisting();
            } catch (InvalidOperationException) { } catch (System.IO.IOException) { }
            isEnabled = true;
            Console.WriteLine($"{port.PortName} Enabled");
        }

        public override void Disable() {
            isEnabled = false;
            port.Close();
        }

        public override List<Device> Scan() {
            // TODO: Handle mid-scan disconnects
            var output = new List<Device>();

            //if (timestampSize > 0b11) {
            //    throw new ArgumentException($"timestampSize cannot be higher than {0b11}.");
            //}

            //if (timestampResolution > 0b1111) {
            //    throw new ArgumentException($"timestampResolution cannot be higher than {0b1111}.");
            //}

            try {
                port.ReadExisting(); // Flush previous output
                port.WriteByte((byte)((byte)Command.Scan | timestampSize));
                port.WriteByte((byte)(timestampResolution & 0xF));
                Thread.Sleep(200);
                uint index_counter = 0;
                byte moreDevs;
                do {
                    //Console.WriteLine("> Next device");
                    byte devClass = moreDevs = (byte)port.ReadByte();
                    devClass &= 1;

                    if ((moreDevs & 0x80) != 0) {
                        var productID = new byte[4];
                        var uniqueID = new byte[4];
                        port.Read(productID, 0, 4);
                        Console.WriteLine(BitConverter.ToUInt32(productID, 0));
                        port.Read(uniqueID, 0, 4);

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

            } catch (InvalidOperationException) { };
            return output;
        }

        public override List<Sample> ReadStream(byte readSize) {
            if (readSize == 0) {
                throw new ArgumentException("readSize must not be 0");
            }
            if (readSize >= (1 << 6)) {
                throw new ArgumentException($"readSize must be less than {1 << 6} (given {readSize}).");
            }

            try {
                port.ReadExisting(); // Flush previous output
                port.WriteByte((byte)((byte)Command.ReadStream | readSize));
                Thread.Sleep(200);

                var sampleCount = port.ReadByte();
                List<Sample> samples = new List<Sample>();
                for (int i = 0; i < sampleCount; i++) {
                    ulong devIdx = port.ReadVarint(0, (byte)port.ReadByte());
                    if (devices[(int)devIdx].cls == DeviceClass.Buffer) {
                        // TODO
                    } else if (devices[(int)devIdx].cls == DeviceClass.Peripheral) {
                        var timestamp = port.ReadInt(1 << timestampSize);
                        var values = new List<Field>((devices[(int)devIdx] as PeripheralDevice).fields.Capacity);
                        foreach (var field in (devices[(int)devIdx] as PeripheralDevice).fields) {
                            switch (field) {
                                case FieldType.ByteArray:
                                    throw new NotImplementedException("Bytearray fields are not implemented");
                                case FieldType.Int:
                                    values.Add(new IntField(port.ReadVarint(0, (byte)port.ReadByte())));
                                    break;
                                case FieldType.Float:
                                    values.Add(new FloatField(port.ReadFloat()));
                                    break;
                                case FieldType.Double:
                                    throw new NotImplementedException("Double fields are not implemented");
                            }
                        }
                        samples.Add(new Sample(devIdx, timestamp, values));
                    }
                }
                return samples;
            } catch (InvalidOperationException) {
                return new List<Sample> { };
            }
        }

        // TODO
        public override void Write(ulong idx, byte[] value) {
            throw new NotImplementedException();
        }

        // TODO
        public override bool Heartbeat() {
            throw new NotImplementedException();
        }

        // FIXME
        public override void AdjustInterval(ulong index, uint interval) {
            port.WriteByte((byte)Command.AdjustInterval);
            var bInterval = new byte[4];
            bInterval[0] = (byte)((byte)interval & 0xFF);
            interval >>= 8;
            bInterval[1] = (byte)((byte)interval & 0xFF);
            interval >>= 8;
            bInterval[2] = (byte)((byte)interval & 0xFF);
            interval >>= 8;
            bInterval[3] = (byte)((byte)interval & 0xFF);
            port.Write(bInterval, 0, 4);
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
