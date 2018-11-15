using System;
using System.Collections.Generic;
using System.Text;

namespace libsv.devices {
    public abstract class IDeviceLogic {
        public string name = "MISSING NAME";
        public string vendor = "MISSING VENDOR";

        public abstract string FormatValue(Sample smp);
    }

    public class FallbackDevice : IDeviceLogic {
        public override string FormatValue(Sample sample) {
            string output = "";
            foreach (var val in sample.values) {
                if (val is ByteArrayField bv) {
                    Console.Write("  [");
                    foreach (var b in bv.value) {
                        Console.Write(b);
                        Console.Write(", ");
                    }
                    Console.WriteLine("]");
                } else if (val is IntField iv) {
                    Console.WriteLine($"  {Convert.ToInt64(iv.value)}");
                } else if (val is FloatField fv) {
                    if (output.Length != 0) {
                        output += ", ";
                    }
                    output += $"{Convert.ToSingle(fv.value):0.02}";
                } else if (val is DoubleField dv) {
                    Console.WriteLine($"  {Convert.ToDouble(dv.value)}");
                }
            }
            return output;
        }
    }
}
