using System;
using System.Collections.Generic;
using System.Text;

namespace libsv.devices {
    class ExampleDevice : IDeviceLogic {
        public ExampleDevice() {
            name = "Analog Input";
            vendor = "SparkVial";
        }

        //public override string FormatValue(Sample smp) {
        //    if (smp.values.Count != 2) {
        //        throw new FormatException("Invalid sample recieved from device");
        //    }

        //    if (smp.values[0] is FloatField v1 && smp.values[1] is FloatField v2) {
        //        return $"{Convert.ToSingle(v1.value):0.02}v, {Convert.ToSingle(v2.value):0.02}v";
        //    } else {
        //        throw new FormatException("Invalid sample recieved from device");
        //    }
        //}

        public override string FormatValue(Sample smp) {
            if (smp.values.Count != 1) {
                throw new FormatException("Invalid sample recieved from device");
            }

            if (smp.values[0] is FloatField v1) {
                return $"{Convert.ToSingle(v1.value):0.02}v";
            } else {
                throw new FormatException("Invalid sample recieved from device");
            }
        }
    }
}
