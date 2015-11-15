using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ev3dev
{
    public class Motor : Device
    {
        public const string motor_large = "lego-ev3-l-motor";
        public const string motor_medium = "lego-ev3-m-motor";

        const string command_run_forever = "run-forever";
        const string command_run_to_abs_pos = "run-to-abs-pos";
        const string command_run_to_rel_pos = "run-to-rel-pos";
        const string command_run_timed = "run-timed";
        const string command_run_direct = "run-direct";
        const string command_stop = "stop";
        const string command_reset = "reset";
        const string encoder_polarity_normal = "normal";
        const string encoder_polarity_inversed = "inversed";
        const string polarity_normal = "normal";
        const string polarity_inversed = "inversed";
        const string speed_regulation_on = "on";
        const string speed_regulation_off = "off";
        const string stop_command_coast = "coast";
        const string stop_command_brake = "brake";
        const string stop_command_hold = "hold";

        public static Motor CreateMediumMotor(string port)
        {
            return new Motor(port, motor_medium);
        }

        public static Motor CreateLargeMotor(string port)
        {
            return new Motor(port, motor_large);
        }

        protected bool Connect(IDictionary<string, string[]> match)
        {
            string classDir = Path.Combine(SYS_ROOT, "class", "tacho-motor");
            string pattern = "motor";

            return Connect(classDir, pattern, match);
        }

        public Motor(string port)
        {
            Connect(new Dictionary<string, string[]>
                {
                    { "port_name", new[] { port }
                }
            });
        }

        public Motor(string port, string motorType)
        {
            Connect(new Dictionary<string, string[]>
            {
                { "port_name", new[] { port } },
                { "driver_name", new[] { motorType } }
            });
        }


        public int count_per_rot { get { return get_attr_int("count_per_rot"); } }
        public int duty_cycle {
            get { return get_attr_int("duty_cycle"); }
            set { set_attr_int("duty_cycle_sp", value); }
        }

        public int position
        {
            get { return get_attr_int("position"); }
            set { set_attr_int("position", value); }
        }

        // Position P: read/write
        // The proportional constant for the position PID.
        public int position_p
        {
            get { return get_attr_int("hold_pid/Kp"); }
            set { set_attr_int("hold_pid/Kp", value); }
        }

        // Position I: read/write
        // The integral constant for the position PID.
        public int position_i
        {
            get { return get_attr_int("hold_pid/Ki"); }
            set { set_attr_int("hold_pid/Ki", value); }
        }

        public int duty_cycle_sp
        {
            get { return get_attr_int("duty_cycle_sp"); }
            set { set_attr_int("duty_cycle_sp", value); }
        }

        // Position SP: read/write
        // Writing specifies the target position for the `run-to-abs-pos` and `run-to-rel-pos`
        // commands. Reading returns the current value. Units are in tacho counts. You
        // can use the value returned by `counts_per_rot` to convert tacho counts to/from
        // rotations or degrees.
        public int position_sp
        {
            get { return get_attr_int("position_sp"); }
            set { set_attr_int("position_sp", value); }
        }

        // Speed: read-only
        // Returns the current motor speed in tacho counts per second. Not, this is
        // not necessarily degrees (although it is for LEGO motors). Use the `count_per_rot`
        // attribute to convert this value to RPM or deg/sec.
        public int speed
        {
            get { return get_attr_int("speed"); }
        } 
        
        // State: read-only
        // Reading returns a list of state flags. Possible flags are
        // `running`, `ramping` `holding` and `stalled`.
        public string[] state
        {
            get { return get_attr_set("state"); }
        }
        // Speed SP: read/write
        // Writing sets the target speed in tacho counts per second used when `speed_regulation`
        // is on. Reading returns the current value.  Use the `count_per_rot` attribute
        // to convert RPM or deg/sec to tacho counts per second.
        public int speed_sp
        {
            get { return get_attr_int("speed_sp"); }
            set { set_attr_int("speed_sp", value); }
        }

        // Stop Command: read/write
        // Reading returns the current stop command. Writing sets the stop command.
        // The value determines the motors behavior when `command` is set to `stop`.
        // Also, it determines the motors behavior when a run command completes. See
        // `stop_commands` for a list of possible values.
        public string stop_command
        {
            get { return get_attr_string("stop_command"); }
            set { set_attr_string("stop_command", value); }
        }

        // Stop Commands: read-only
        // Returns a list of stop modes supported by the motor controller.
        // Possible values are `coast`, `brake` and `hold`. `coast` means that power will
        // be removed from the motor and it will freely coast to a stop. `brake` means
        // that power will be removed from the motor and a passive electrical load will
        // be placed on the motor. This is usually done by shorting the motor terminals
        // together. This load will absorb the energy from the rotation of the motors and
        // cause the motor to stop more quickly than coasting. `hold` does not remove
        // power from the motor. Instead it actively try to hold the motor at the current
        // position. If an external force tries to turn the motor, the motor will 'push
        // back' to maintain its position.
        public string[] stop_commands
        {
            get { return get_attr_set("stop_commands"); }
        }

        public int time_sp
        {
            get { return get_attr_int("time_sp"); }
            set { set_attr_int("time_sp", value); }
        }

        // Driver Name: read-only
        // Returns the name of the sensor device/driver. See the list of [supported
        // sensors] for a complete list of drivers.
        public string driver_name() { return get_attr_string("driver_name"); }


        // Command: write-only
        // Sends a command to the sensor.
        public void set_command(string v)
        {
            set_attr_string("command", v);
        }

        // Commands: read-only
        // Returns a list of the valid commands for the sensor.
        // Returns -EOPNOTSUPP if no commands are supported.
        public string[] commands() { return get_attr_set("commands"); }

        // Run the motor until another command is sent.
        public void run_forever() { set_command("run-forever"); }

        // Run to an absolute position specified by `position_sp` and then
        // stop using the command specified in `stop_command`.
        public void run_to_abs_pos() { set_command("run-to-abs-pos"); }

        // Run to a position relative to the current `position` value.
        // The new position will be current `position` + `position_sp`.
        // When the new position is reached, the motor will stop using
        // the command specified by `stop_command`.
        public void run_to_rel_pos() { set_command("run-to-rel-pos"); }

        // Run the motor for the amount of time specified in `time_sp`
        // and then stop the motor using the command specified by `stop_command`.
        public void run_timed() { set_command("run-timed"); }

        // Run the motor at the duty cycle specified by `duty_cycle_sp`.
        // Unlike other run commands, changing `duty_cycle_sp` while running *will*
        // take effect immediately.
        public void run_direct() { set_command("run-direct"); }

        // Stop any of the run commands before they are complete using the
        // command specified by `stop_command`.
        public void stop() { set_command("stop"); }

        // Reset all of the motor parameter attributes to their default value.
        // This will also have the effect of stopping the motor.
        public void reset() { set_command("reset"); }
    }
}

