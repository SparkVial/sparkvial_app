using System.Collections.Generic;
using System.Threading.Tasks;

namespace libsv {
    public abstract class InterfaceType {
        public List<Interface> interfaces = new List<Interface>();
        public abstract Task<List<Interface>> Scan();
    }

    /// <summary>
    /// A single instance of an <see cref="InterfaceType"/>, ex. a single serial port, bluetooth connection, etc.
    /// </summary>
    public abstract class Interface {

        // Fields for xaml bindings
        public string Id => id;
        public string Type => type;
        public string Info => info;

        /// <summary>
        /// A unique identifier within the <see cref="InterfaceType"/>, ex. COM1, the bluetooth mac address, etc.
        /// </summary>
        public readonly string id;

        /// <summary>
        /// Human readable string describing the <see cref="Interface"/>.
        /// </summary>
        public readonly string info;

        /// <summary>
        /// Human readable type, ex. Serial, Bluetooth, etc.
        /// </summary>
        public readonly string type;

        public object scanLock = new object();

        public byte timestampSize = 2;
        public byte timestampResolution = 1;

        public Interface(string id, string info, string type) {
            this.id = id;
            this.info = info;
            this.type = type;
        }

        /// <summary>
        /// The <see cref="Device"/>s this <see cref="Interface"/> manages.
        /// </summary>
        public List<Device> devices = new List<Device>();

        /// <summary>
        /// Is there an active connection / session with the <see cref="Interface"/>?
        /// </summary>
        public bool isEnabled = false;

        /// <summary>
        /// Initiate a connection / session with the <see cref="Interface"/>.
        /// </summary>
        public abstract Task<bool> Enable();

        /// <summary>
        /// Drop the connection / session with the <see cref="Interface"/>.
        /// </summary>
        public abstract Task Disable();

        /// <summary>
        /// Checks that rescan is not required.
        /// </summary>
        /// <returns>Is rescan not required</returns>
        public abstract Task<bool> Heartbeat();

        /// <summary>
        /// Perform a scan to find all sub<see cref="Device"/>s.
        /// Caller must update the <see cref="devices"/> list.
        /// </summary>
        /// <returns>Scan results</returns>
        public abstract Task<List<Device>> Scan();
        
        public abstract Task<List<Sample>> ReadStream(byte readSize);

        public abstract Task<ulong> GetBacklogSize();

        public abstract void AdjustInterval(uint index, uint interval);

        public abstract void Write(uint idx, byte[] value);

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();
    }
}
