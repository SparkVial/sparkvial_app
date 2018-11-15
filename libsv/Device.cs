using System;
using System.Collections.Generic;

namespace libsv {
    public class Device {
        public readonly uint productID;
        public readonly uint uniqueID;
        public readonly uint index;
        public readonly string name;
        public readonly DeviceClass cls;
        private readonly Interface inf;

        // Fields for xaml bindings
        public string Name => name;
        public uint Index => index;
        public uint UniqueID => uniqueID;
        public uint ProductID => productID;

        public Device(Interface inf, uint productID, uint uniqueID, uint index, DeviceClass cls) {
            this.inf = inf;
            this.productID = productID;
            this.uniqueID = uniqueID;
            this.index = index;
            this.cls = cls;
            name = SparkVial.productDB.ContainsKey(productID) ? SparkVial.productDB[productID] : $"{productID:X8}";
        }

        public void AdjustInterval(uint interval) {
            inf.AdjustInterval(index, interval);
        }

        public void Write(byte[] value) {
            inf.Write(index, value);
        }

        public override bool Equals(object obj) {
            if (!(obj is Device item)) {
                return false;
            }

            return productID == item.productID && uniqueID == item.uniqueID && index == item.index;
        }

        public override int GetHashCode() {
            int hashcode = 23;
            hashcode = (hashcode * 37) + productID.GetHashCode();
            hashcode = (hashcode * 37) + uniqueID.GetHashCode();
            hashcode = (hashcode * 37) + index.GetHashCode();
            return hashcode;
        }
    }

    public class BufferDevice : Device {
        public readonly ulong storageSize;

        public BufferDevice(Interface inf, uint productID, uint uniqueID, uint index, ulong storageSize)
            : base(inf, productID, uniqueID, index, DeviceClass.Buffer) {

            this.storageSize = storageSize;
        }
    }

    public class PeripheralDevice : Device {
        public readonly List<FieldType> fields;

        public PeripheralDevice(Interface inf, uint productID, uint uniqueID, uint index, List<FieldType> fields)
            : base(inf, productID, uniqueID, index, DeviceClass.Peripheral) {

            this.fields = fields;
        }
    }
}
