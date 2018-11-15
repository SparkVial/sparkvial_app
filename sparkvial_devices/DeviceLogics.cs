using System;
using System.Collections.Generic;

namespace libsv.devices {
    public static class DeviceLogics {
        private static readonly Dictionary<uint, IDeviceLogic> map = new Dictionary<uint, IDeviceLogic> {
            { 0, new FallbackDevice() },
            { 0 << 16 | 2, new MockDevice() },
            { 1 << 16 | 2, new ExampleDevice() }
        };
        public static IDeviceLogic Get(uint identifier) {
            return map.ContainsKey(identifier) ? map[identifier] : map[0];
        }
    }
}
