using System;

namespace libsv {
    public class Device {
        public readonly uint productID;
        public readonly uint serialNum;
        public readonly uint index;
        public readonly string name;
        private readonly Interface inf;

        public Device(Interface inf, uint productID, uint serialNum, uint index) {
            this.inf = inf;
            this.productID = productID;
            this.serialNum = serialNum;
            this.index = index;
            name = SparkVial.productDB.ContainsKey(productID) ? SparkVial.productDB[productID] : String.Format("{0:X8}", productID);
        }
        public float[] GetDefaultValue() {
            var result = GetValue(1);
            var output = new float[result.Length / 4];
            Buffer.BlockCopy(result, 0, output, 0, result.Length);
            return output;
        }
        public byte[] GetValue(uint gvID) {
            return inf.GetValue(index, gvID);
        }
        public void SetValue(uint svID, byte[] value) {
            inf.SetValue(index, svID, value);
        }

        public override bool Equals(object obj) {
            if (!(obj is Device item)) {
                return false;
            }

            return productID == item.productID && serialNum == item.serialNum && index == item.index;
        }

        public override int GetHashCode() {
            int hashcode = 23;
            hashcode = (hashcode * 37) + productID.GetHashCode();
            hashcode = (hashcode * 37) + serialNum.GetHashCode();
            hashcode = (hashcode * 37) + index.GetHashCode();
            return hashcode;
        }
    }
}
