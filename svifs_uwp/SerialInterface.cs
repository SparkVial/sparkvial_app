using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using libsv;
using Windows.Devices.SerialCommunication;


namespace svifs.uwp {
    /*public static class MyExtensions {
        public static void WriteByte(this SerialPort port, byte b) {
            port.Write(new byte[] { b }, 0, 1);
        }

        public static ulong ReadInt(this SerialPort port, int size) {
            var output = new byte[8];
            port.Read(output, 0, size);
            return BitConverter.ToUInt64(output, 0);
        }

        public static float ReadFloat(this SerialPort port) {
            var output = new byte[4];
            port.Read(output, 0, 4);
            return BitConverter.ToSingle(output, 0);
        }

        public static ulong ReadVarint(this SerialPort port, byte skipBits, byte initialByte) {
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

    public class SerialInterfaceType : InterfaceType {
        public override List<Interface> Scan() {
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
                    select (Interface)new Win32SerialInterface(p[0], p[1])
                ).ToList();
            }
            return interfaces;
        }
    }
    public class Win32SerialInterface : Interface {
        private SerialPort port;

        public override ulong BacklogSize => throw new NotImplementedException();

        public Win32SerialInterface(string port, string portInfo) : base(port, portInfo, "Serial") { }

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
            if (!(obj is Win32SerialInterface item)) {
                return false;
            }

            return id.Equals(item.id);
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }
    }*/

    /*public class SerialInterfaceType : InterfaceType {

        public override List<Interface> Scan() {
            throw new NotImplementedException();
        }
    }

    public class SerialInterface : Interface {
        public override ulong BacklogSize => throw new NotImplementedException();

        public SerialInterface(string port, string portInfo) : base(port, portInfo, "Serial") { }

        public override void AdjustInterval(ulong index, uint interval) {
            throw new NotImplementedException();
        }

        public override void Disable() {
            throw new NotImplementedException();
        }

        public override bool Enable() {
            throw new NotImplementedException();
        }

        public override bool Heartbeat() {
            throw new NotImplementedException();
        }

        public override List<Sample> ReadStream(byte readSize) {
            throw new NotImplementedException();
        }

        public override List<Device> Scan() {
            throw new NotImplementedException();
        }

        public override void Write(ulong idx, byte[] value) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            throw new NotImplementedException();
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }*/
}
