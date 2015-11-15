using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ev3dev;

namespace SensorDemo
{
    public class Program
    {
        public void Main(string[] args)
        {
            //Device.SYS_ROOT = "c:/temp/ev3-sensors";
            Console.WriteLine("Reading sensor data");
            Sensor s = new Sensor(string.Empty);

            if (s.modes.Any())
                Console.WriteLine("Avaliable sensor modes: " + s.modes.Aggregate((x, y) => x + " " + y));

            while (true)
            {
                Console.WriteLine("Sensor value: " + s.value());

                Thread.Sleep(2000);
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                        break;
                }
            }

        }
    }
}
