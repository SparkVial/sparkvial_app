using libsv;
using System;
using System.Collections.Generic;

namespace sparkvial_cli {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine($"--- SparkVial CLI v0.1.0 / libsv v{SparkVial.MajorVersion}.{SparkVial.MinorVersion}.{SparkVial.PatchVersion} ---");
            SparkVial sv = new SparkVial(new List<InterfaceType> {
                new svifs.win32.Win32SerialInterfaceType()
            });

            sv.OnInterfaceAdded += (Interface inf) => {
                Console.WriteLine($"- Interface of type {inf.type} added: {inf.id}");
                inf.Enable();
            };
            sv.OnInterfaceRemoved += (Interface inf) => {
                Console.WriteLine($"- Interface of type {inf.type} removed: {inf.id}");
            };
            sv.OnDeviceAdded += (Device dev) => {
                Console.WriteLine($"- Device '{dev.name}' added with serial number {dev.uniqueID:X8}");
                dev.AdjustInterval(25);  // in ms
            };
            sv.OnDeviceRemoved += (Device dev) => {
                Console.WriteLine($"- Device '{dev.name}' with serial number {dev.uniqueID:X8} removed");
            };
            sv.OnSample += (dev, smp) => {
                Console.WriteLine($"- Value from {dev.name} ({dev.uniqueID:X8}) with {smp.values.Count} fields at {smp.timestamp}ms:");
                int i = 0;
                foreach (var val in smp.values) {
                    switch (dev.fields[i]) {
                        case FieldType.ByteArray:
                            Console.Write("  [");
                            foreach (var b in (val as ByteArrayField).value) {
                                Console.Write(b);
                                Console.Write(", ");
                            }
                            Console.WriteLine("]");
                            break;
                        case FieldType.Int:
                            Console.WriteLine($"  {Convert.ToInt64((val as IntField).value)}");
                            break;
                        case FieldType.Float:
                            Console.WriteLine($"  {Convert.ToSingle((val as FloatField).value)}f");
                            break;
                        case FieldType.Double:
                            Console.WriteLine($"  {Convert.ToDouble((val as DoubleField).value)}");
                            break;
                    }
                    i++;
                }
            };

            sv.AutoScan = false;
            sv.AutoSample = true;

            sv.Scan();
        }
    }
}
