using System;
using System.Collections.Generic;
using System.Text;

namespace libsv {
    public static class DeviceDB {
        private static readonly Dictionary<uint, Tuple<string, string>> map = new Dictionary<uint, Tuple<string, string>> {
            { 0, Tuple.Create("MISSING NAME", "MISSING VENDOR") },
            { 0 << 16 | 2, Tuple.Create("Mock Device", "ACME Inc.") },
            { 1 << 16 | 2, Tuple.Create("Example Device", "SparkVial") }
        };
        public static Tuple<string, string> Get(uint identifier) {
            return map.ContainsKey(identifier) ? map[identifier] : Tuple.Create($"{identifier:X8}?", "?");
        }
    }
}
