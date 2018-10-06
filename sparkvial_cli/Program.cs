using libsv;
using libsv.InterfaceTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sparkvial_cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"--- SparkVial CLI v1.0.0 / libsv v{SparkVial.Version} ---");
            SparkVial sv = new SparkVial(new List<InterfaceType> {
                new Win32SerialInterfaceType()
            });

            sv.OnInterfaceAdded += (Interface inf) => {
                Console.WriteLine($"- Interface of type {inf.type} added: {inf.id}");
                inf.Enable();
            };
            sv.OnInterfaceRemoved += (Interface inf) => {
                Console.WriteLine($"- Interface of type {inf.type} removed: {inf.id}");
            };
            sv.OnDeviceAdded += (Device dev) => {
                Console.WriteLine($"- Device {dev.name} Added with serial number {dev.serialNum:X8}");
            };
            sv.OnDeviceRemoved += (Device dev) => {
                Console.WriteLine($"- Device {dev.name} / {dev.serialNum:X8} Removed");
            };
            //sv.OnDeviceValue += (Device dev, byte[] value) => {
            //    Console.Write($"- Value from {dev.name} ({dev.serialNum:X8}) of size {value.Count()}:");
            //    foreach (var c in value) {
            //        Console.Write($" {c}");
            //    }
            //    Console.WriteLine();
            //};
            sv.OnDeviceValueFloat += (Device dev, float value) => {
                Console.WriteLine($"- Value from {dev.name} ({dev.serialNum:X8}): {value}");
            };

            sv.AutoScan = true;
            sv.AutoSample = true;
        }
    }
}
