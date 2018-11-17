using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libsv {
    public abstract class Field { }

    public class ByteArrayField : Field {
        public byte[] value;

        public ByteArrayField(byte[] value) => this.value = value;
    }

    public class IntField : Field {
        public ulong value;

        public IntField(ulong value) => this.value = value;
    }

    public class FloatField : Field {
        public float value;

        public FloatField(float value) => this.value = value;
    }

    public class DoubleField : Field {
        public double value;

        public DoubleField(double value) => this.value = value;
    }

    public class Sample {
        public Interface device;
        public ulong idx;
        public ulong timestamp;
        public List<Field> values;

        public Sample(Interface device, uint idx, ulong timestamp, List<Field> values) {
            this.device = device;
            this.idx = idx;
            this.timestamp = timestamp;
            this.values = values;
        }
    }
}
