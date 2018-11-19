using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using libsv.devices;

namespace libsv {
    public class MockDevice : IDeviceLogic {
        public override string FormatValue(Sample smp) {
            return $"{(smp.values[0] as FloatField).value:0.02} units";
        }
    }

    class MockInterface : Interface {
        uint interval = 0;
        Random rand = new Random();

        public MockInterface() : base("Mock1", "Mock Interface 1", "Mock") {

        }

        public override void AdjustInterval(uint index, uint interval) {
            this.interval = interval;
        }

        public override Task Disable() {
            interval = 0;
            isEnabled = false;
            return Task.CompletedTask;
        }

        public override Task<bool> Enable() {
            //throw new NotImplementedException();
            isEnabled = true;
            return Task.FromResult(true);
        }

        public override Task<ulong> GetBacklogSize() {
            throw new NotImplementedException();
        }

        public override Task<bool> Heartbeat() {
            throw new NotImplementedException();
        }

        private ulong i = 0;
        public override Task<List<Sample>> ReadStream(byte readSize) {
            return Task.FromResult(new List<Sample>() {
                new Sample(this, 0, i++, new List<Field>() { new FloatField((float)rand.NextDouble() / 5 + (float)Math.Sin(DateTime.Now.Ticks / 50000) + 1) })
            });
        }

        public override Task<List<Device>> Scan() {
            return Task.FromResult(new List<Device>() {
                new PeripheralDevice(this, 0 << 16 | 2, 0, 0, new List<FieldType>(){
                    FieldType.Float
                })
            });
        }

        public override void Write(uint idx, byte[] value) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            if (!(obj is MockInterface item)) {
                return false;
            }

            return id.Equals(item.id);
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }
    }

    public class MockInterfaceType : InterfaceType {
        public MockInterfaceType() {

        }

        public override Task<List<Interface>> Scan() {
            return Task.FromResult(new List<Interface> { new MockInterface() });
        }
    }
}
