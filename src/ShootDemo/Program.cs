using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ev3dev;

namespace ShootDemo
{
    public class Program
    {
        public void Main(string[] args)
        {
            Motor m = new Motor(string.Empty);

            m.duty_cycle_sp = 50;
            m.stop_command = "hold";

            int min = -50;
            int max = 40;
            m.position_sp = max;

            m.run_to_abs_pos();

            Thread.Sleep(2000);

            m.position_sp = min;

            m.run_to_abs_pos();
            Thread.Sleep(2000);

            m.stop_command = "coast";
            m.stop();
        }
    }
}
