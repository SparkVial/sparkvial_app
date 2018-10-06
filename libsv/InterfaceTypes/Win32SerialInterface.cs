using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace libsv.InterfaceTypes {
    public class Win32SerialInterfaceType : libsv.InterfaceType {
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
    public class Win32SerialInterface : libsv.Interface {
        private SerialPort port;

        public Win32SerialInterface(string port, string portInfo) : base(port, portInfo, "Serial") { }

        public override void Enable() {
            if (port != null && port.IsOpen) {
                port.Close();
            }
            port = new SerialPort(id, 9600);
            try {
                port.Open();
                Thread.Sleep(2500);
                port.ReadExisting();
            } catch (InvalidOperationException) { } catch (System.IO.IOException) { }
            isEnabled = true;
        }

        public override void Disable() {
            isEnabled = false;
            port.Close();
        }

        public override List<Device> Scan() {
            var output = new List<Device>();

            try {
                port.ReadExisting(); // Flush previous output
                port.Write(new byte[] { (byte)Command.Scan << 5 }, 0, 1);
                Thread.Sleep(200);
                uint index_counter = 0;
                byte moreDevs;
                do {
                    moreDevs = (byte)port.ReadByte();
                    if (moreDevs != 0) {
                        var identity = new byte[8];
                        var valueTypes = new byte[8];
                        port.Read(identity, 0, 8);
                        port.Read(valueTypes, 0, 8);

                        uint serialNum;
                        uint productID;
                        if (!BitConverter.IsLittleEndian) {
                            Array.Reverse(identity);

                            serialNum = BitConverter.ToUInt32(identity, 4);
                            productID = BitConverter.ToUInt32(identity, 0);
                        } else {
                            serialNum = BitConverter.ToUInt32(identity, 0);
                            productID = BitConverter.ToUInt32(identity, 4);
                        }
                        output.Add(new Device(this, productID, serialNum, index_counter++));
                    }
                } while (moreDevs != 0);

            } catch (InvalidOperationException) { };
            /*Console.WriteLine("Bytes: ");
            for (int i = 0; i < 52 && port.BytesToRead > 0; i++) {
                Console.Write(port.ReadByte());
                Console.Write(" ");
            }*/
            //Console.WriteLine();
            //Console.Write("Left over: ");
            //Console.WriteLine(port.BytesToRead);
            return output;
        }

        public override List<byte[]> GetValue(uint gvID) {
            if (gvID > 0b11111) {
                throw new NotImplementedException($"Register IDs above {0b11111} aren't implemented yet.");
            }
            var output = new List<byte[]>();

            try {
                port.ReadExisting(); // Flush previous output
                port.Write(new byte[] { (byte)((byte)Command.GetValue << 5 | (byte)gvID) }, 0, 1);
                for (int i = 0; i < devices.Count; i++) {
                    var dataLen = port.ReadByte();
                    var data = new byte[dataLen];
                    port.Read(data, 0, dataLen);
                    output.Add(data);
                }

            } catch (InvalidOperationException) { };
            return output;
        }

        public override byte[] GetValue(uint index, uint gvID) {
            throw new NotImplementedException();
        }

        public override void SetValue(uint index, uint svID, byte[] value) {
            throw new NotImplementedException();
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
    }
}
