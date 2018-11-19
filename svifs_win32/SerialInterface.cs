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
}
