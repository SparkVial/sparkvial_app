using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace libsv {

    public class PortClosedException : Exception { }

    public class SerialInterface : Interface {
        private ISerialPort port;

        public override Task<ulong> GetBacklogSize() {
            throw new NotImplementedException();
        }

        public SerialInterface(string portID, string portInfo, ISerialPort backend) : base(portID, portInfo, "Serial") {
            port = backend;
        }

        public override async Task<bool> Enable() {
            if (!await port.Open(115200)) {
                return false;
            }
            await Task.Delay(3000);
            port.Flush();
            isEnabled = true;

            Console.WriteLine($"{id} Enabled");
            return true;
        }

        public override Task Disable() {
            isEnabled = false;
            port.Close();
            return Task.CompletedTask;
        }

        public override async Task<List<Device>> Scan() {
            // TODO: Handle mid-scan disconnects
            var output = new List<Device>();

            //if (timestampSize > 0b11) {
            //    throw new ArgumentException($"timestampSize cannot be higher than {0b11}.");
            //}

            //if (timestampResolution > 0b1111) {
            //    throw new ArgumentException($"timestampResolution cannot be higher than {0b1111}.");
            //}

            Console.Write("  Scan init");
            try {
                port.Flush(); // Flush previous output
                port.WriteByte((byte)((byte)Command.Scan | timestampSize));
                port.WriteByte((byte)(timestampResolution & 0xF));
                //await Task.Delay(200);
                uint index_counter = 0;
                byte moreDevs;
                do {
                    Console.Write("  1111");
                    byte devClass = moreDevs = await port.ReadByte();
                    devClass &= 1;

                    if ((moreDevs & 0x80) != 0) {
                        var productID = await port.Read(4);
                        var uniqueID = await port.Read(4);
                        
                        Console.WriteLine(BitConverter.ToUInt32(productID, 0));

                        if (devClass == (byte)DeviceClass.Buffer) {
                            var shlAmount = await port.ReadByte();
                            var bufferSize = await port.ReadByte() | (await port.ReadByte() << 8);

                            output.Add(new BufferDevice(
                                this,
                                BitConverter.ToUInt32(productID, 0),
                                BitConverter.ToUInt32(uniqueID, 0),
                                index_counter++,
                                (ulong)bufferSize << shlAmount
                            ));

                            throw new NotImplementedException("Buffer devices are not implemented yet");
                        } else {
                            Console.Write("  2222");
                            var fieldCount = await port.ReadByte();
                            //Console.WriteLine($"> There are {fieldCount} fields");
                            var fields = new List<FieldType>();
                            for (int i = 0; i < fieldCount; i += 4) {
                                var currentFieldTypes = await port.ReadByte();
                                for (int j = i; j < Math.Min(i + 4, fieldCount); j++) {
                                    //Console.WriteLine($"> New field {currentFieldTypes & 0b11}");
                                    fields.Add((FieldType)(currentFieldTypes & 0b11));
                                    currentFieldTypes >>= 2;
                                }
                            }
                            Console.Write("  3333");

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

            } catch (InvalidOperationException) {
                throw new PortClosedException();
            };
            return output;
        }

        public override async Task<List<Sample>> ReadStream(byte readSize) {
            if (readSize == 0) {
                throw new ArgumentException("readSize must not be 0");
            }
            if (readSize >= (1 << 6)) {
                throw new ArgumentException($"readSize must be less than {1 << 6} (given {readSize}).");
            }

            try {
                port.Flush(); // Flush previous output
                port.WriteByte((byte)((byte)Command.ReadStream | readSize));

                var sampleCount = await port.ReadByte();
                var samples = new List<Sample>();
                for (int i = 0; i < sampleCount; i++) {
                    var devIdx = (uint) await ReadVarint(0, await port.ReadByte());

                    if ((int)devIdx <= devices.Count) {
                        if (devices[(int)devIdx].cls == DeviceClass.Buffer) {
                            throw new NotImplementedException("Buffer devices not implemented");
                        } else if (devices[(int)devIdx].cls == DeviceClass.Peripheral) {

                            var timestamp = await ReadInt(1 << timestampSize);
                            var values = new List<Field>((devices[(int)devIdx] as PeripheralDevice).fields.Capacity);
                            foreach (var field in (devices[(int)devIdx] as PeripheralDevice).fields) {
                                switch (field) {
                                    case FieldType.ByteArray:
                                        throw new NotImplementedException("Bytearray fields are not implemented");
                                    case FieldType.Int:
                                        values.Add(new IntField(await ReadVarint(0, await port.ReadByte())));
                                        break;
                                    case FieldType.Float:
                                        values.Add(new FloatField(await ReadFloat()));
                                        break;
                                    case FieldType.Double:
                                        throw new NotImplementedException("Double fields are not implemented");
                                }
                            }
                            samples.Add(new Sample(this, devIdx, timestamp, values));
                        }
                    }
                }
                return samples;
            } catch (Exception) {
                port.Flush();
                throw new PortClosedException();
            }
        }

        // TODO
        public override void Write(uint idx, byte[] value) {
            throw new NotImplementedException();
        }

        // TODO
        public override async Task<bool> Heartbeat() {
            try {
                /*
                 a: 1 bit = challenge >> 0
    b: 1 bit = challenge >> 1
    c: 1 bit = challenge >> 2
    d: 1 bit = challenge >> 3
    e: 1 bit = challenge >> 4

    r1: 1 bit = (1 ⊕ a ⊕ b ⊕ c)
    r2: 1 bit = (0 ⊕ b ⊕ c ⊕ d)
    r3: 1 bit = (1 ⊕ c ⊕ d ⊕ e)

    response: 8 bits = challenge | r1 << 7 | r2 << 6 | r3 << 5
                 */
                byte challenge = 0;
                if (challenge >= 1 << 4) {
                    throw new ArgumentException($"Challenge must be less than {1 << 4}. (Given {challenge})");
                }
                port.WriteByte((byte)Command.Heartbeat);

                var a = (challenge >> 4) & 1;
                var b = (challenge >> 3) & 1;
                var c = (challenge >> 2) & 1;
                var d = (challenge >> 1) & 1;
                var e = (challenge >> 0) & 1;

                var r1 = 1 ^ a ^ b ^ c;
                var r2 = 0 ^ b ^ c ^ d;
                var r3 = 1 ^ c ^ d ^ e;

                challenge |= (byte) (r1 << 7);
                challenge |= (byte) (r2 << 6);
                challenge |= (byte) (r3 << 5);

                var response = await port.ReadByte();

            } catch(PortClosedException) {

            }
            throw new NotImplementedException();
            // throw new PortClosedException();
        }

        // FIXME
        public override void AdjustInterval(uint index, uint interval) {
            port.WriteByte((byte)Command.AdjustInterval);
            var bInterval = new byte[4];
            bInterval[0] = (byte)((byte)interval & 0xFF);
            interval >>= 8;
            bInterval[1] = (byte)((byte)interval & 0xFF);
            interval >>= 8;
            bInterval[2] = (byte)((byte)interval & 0xFF);
            interval >>= 8;
            bInterval[3] = (byte)((byte)interval & 0xFF);
            port.Write(bInterval);
        }


        public async Task<ulong> ReadInt(int size) {
            var ba = new byte[8];
            (await port.Read(size)).CopyTo(ba, 0);
            return BitConverter.ToUInt64(ba, 0);
        }

        public async Task<float> ReadFloat() {
            return BitConverter.ToSingle(await port.Read(4), 0);
        }

        public async Task<ulong> ReadVarint(byte skipBits, byte initialByte) {
            byte offset = (byte)(7 - skipBits);
            ulong output = (initialByte & ~(0xFFu << offset));

            bool more = (initialByte & (0x80 >> skipBits)) != 0;
            for (int i = 0; i < 9 && more; i++) {
                var next = await port.ReadByte();
                output |= ((ulong)next & 0x7F) << offset;
                more = (next & 0x80) != 0;
                offset += 7;
            }

            return output;
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
    }

    public interface ISerialPort {
        Task<bool> Open(int baudRate);

        void Close();

        void Flush();

        void Write(byte[] b);

        void WriteByte(byte b);

        Task<byte[]> Read(int count);

        Task<byte> ReadByte();
    }
}