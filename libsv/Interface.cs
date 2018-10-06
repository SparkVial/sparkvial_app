using System.Collections.Generic;

namespace libsv {
    public abstract class InterfaceType {
        public List<Interface> interfaces = new List<Interface>();
        public abstract List<Interface> Scan();
    }

    /// <summary>
    /// A single instance of an <see cref="InterfaceType"/>, ex. a single serial port, bluetooth connection, etc.
    /// </summary>
    public abstract class Interface {

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
        public abstract void Enable();

        /// <summary>
        /// Drop the connection / session with the <see cref="Interface"/>.
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Perform a scan to find all child <see cref="Device"/>s.
        /// Caller must update the <see cref="devices"/> list.
        /// </summary>
        /// <returns>Scan results</returns>
        public abstract List<Device> Scan();

        /// <summary>
        /// Performs a global GetValue request, 
        /// </summary>
        /// <param name="gvID"></param>
        /// <returns></returns>
        public abstract List<byte[]> GetValue(uint gvID);
        public abstract byte[] GetValue(uint index, uint gvID);
        public abstract void SetValue(uint index, uint svID, byte[] value);
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
    }
}
