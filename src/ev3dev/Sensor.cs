using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ev3dev
{
    public class Sensor : Device
    {
        //typedef device_type sensor_type;

        const string ev3_touch = "lego-ev3-touch";
        const string ev3_color = "lego-ev3-color";
        const string ev3_ultrasonic = "lego-ev3-us";
        const string ev3_gyro = "lego-ev3-gyro";
        const string ev3_infrared = "lego-ev3-ir";

        const string nxt_touch = "lego-nxt-touch";
        const string nxt_light = "lego-nxt-light";
        const string nxt_sound = "lego-nxt-sound";
        const string nxt_ultrasonic = "lego-nxt-us";
        const string nxt_i2c_sensor = "nxt-i2c-sensor";
        const string nxt_analog = "nxt-analog";

        public Sensor(string port)
        {
            Connect(new Dictionary<string, string[]>
                {
                    { "port_name", new[] { port }
                }
            });
        }

        protected bool Connect(IDictionary<string, string[]> match)
        {
            string classDir = Path.Combine(SYS_ROOT, "class", "lego-sensor");
            string pattern = "sensor";

            return Connect(classDir, pattern, match);
        }

        public Sensor(string port, string[] types)
        {
            Connect(new Dictionary<string, string[]>
            {
                { "port_name", new[] { port } },
                { "driver_name", types }
            });
        }
        
        // Returns the value or values measured by the sensor. Check `num_values` to
        // see how many values there are. Values with index >= num_values will return
        // an error. The values are fixed point numbers, so check `decimals` to see
        // if you need to divide to get the actual value.
        public int value(int index = 0)
        {
            if (index >= num_values)
                throw new ArgumentOutOfRangeException();

            return get_attr_int("value" + index);
        }

        // The value converted to float using `decimals`.
        public double float_value(int index = 0)
        {
            return value(index) * Math.Pow(10, -decimals());
        }

        // Human-readable name of the connected sensor.
        public string type_name
        {
            get
            {
                var type = driver_name();
                if (string.IsNullOrEmpty(type))
                {
                    return "<none>";
                }

                var lookup_table = new Dictionary<string, string>{
                    { ev3_touch,       "EV3 touch" },
                    { ev3_color,       "EV3 color" },
                    { ev3_ultrasonic,  "EV3 ultrasonic" },
                    { ev3_gyro,        "EV3 gyro" },
                    { ev3_infrared,    "EV3 infrared" },
                    { nxt_touch,       "NXT touch" },
                    { nxt_light,       "NXT light" },
                    { nxt_sound,       "NXT sound" },
                    { nxt_ultrasonic,  "NXT ultrasonic" },
                    { nxt_i2c_sensor,  "I2C sensor" },
                  };

                string value;
                if (lookup_table.TryGetValue(type, out value))
                    return value;

                return type;
            }
        }

        // Bin Data Format: read-only
        // Returns the format of the values in `bin_data` for the current mode.
        // Possible values are:
        //
        //    - `u8`: Unsigned 8-bit integer (byte)
        //    - `s8`: Signed 8-bit integer (sbyte)
        //    - `u16`: Unsigned 16-bit integer (ushort)
        //    - `s16`: Signed 16-bit integer (short)
        //    - `s16_be`: Signed 16-bit integer, big endian
        //    - `s32`: Signed 32-bit integer (int)
        //    - `float`: IEEE 754 32-bit floating point (float)
        public string bin_data_format()
        {
            return get_attr_string("bin_data_format");
        }

        // Bin Data: read-only
        // Returns the unscaled raw values in the `value<N>` attributes as raw byte
        // array. Use `bin_data_format`, `num_values` and the individual sensor
        // documentation to determine how to interpret the data.
        public byte[] bin_data()
        {
            if (!Connected)
                throw new NotSupportedException("no device connected");

            if (!_bin_data.Any())
            {
                var lookup_table = new Dictionary<string, int> {
                    { "u8",     1},
                      { "s8",     1},
                      { "u16",    2},
                      { "s16",    2},
                      { "s16_be", 2},
                      { "s32",    4},
                      { "float",  4}
                };

                int value_size = 1;

                int value;
                if (lookup_table.TryGetValue(bin_data_format(), out value))
                    value_size = value;

                Array.Resize(ref _bin_data, value_size);
            }

            string fname = Path.Combine(_path, "bin_data");

            using (Stream s = File.OpenRead(fname))
                if (s.CanRead)
                {
                    s.Read(_bin_data, 0, _bin_data.Length);
                }

            return _bin_data;
        }
        // Bin Data: read-only
        // Writes the unscaled raw values in the `value<N>` attributes into the
        // user-provided struct/buffer.  Use `bin_data_format`, `num_values` and the
        // individual sensor documentation to determine how to interpret the data.
        //template<class T>
        //  void bin_data(T* buf) const {
        //      bin_data(); // fills _bin_data
        //    copy_n(_bin_data.data(), _bin_data.size(), static_cast<char*>(buf));
        //  }

        //~autogen cpp_generic-get-set classes.sensor>currentClass

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

        // Decimals: read-only
        // Returns the number of decimal places for the values in the `value<N>`
        // attributes of the current mode.
        public int decimals()
        {
            return get_attr_int("decimals");
        }

        // Driver Name: read-only
        // Returns the name of the sensor device/driver. See the list of [supported
        // sensors] for a complete list of drivers.
        public string driver_name() { return get_attr_string("driver_name"); }

        // Mode: read/write
        // Returns the current mode. Writing one of the values returned by `modes`
        // sets the sensor to that mode.
        public string mode()
        {
            return get_attr_string("mode");
        }

        //        auto set_mode(string v) -> decltype(*this)
        //{
        //            set_attr_string("mode", v);
        //            return *this;
        //        }

        // Modes: read-only
        // Returns a list of the valid modes for the sensor.
        public string[] modes { get { return get_attr_set("modes"); } }

        // Num Values: read-only
        // Returns the number of `value<N>` attributes that will return a valid value
        // for the current mode.
        public int num_values { get { return get_attr_int("num_values"); } }

        // Port Name: read-only
        // Returns the name of the port that the sensor is connected to, e.g. `ev3:in1`.
        // I2C sensors also include the I2C address (decimal), e.g. `ev3:in1:i2c8`.
        public string port_name { get { return get_attr_string("port_name"); } }

        // Units: read-only
        // Returns the units of the measured value for the current mode. May return
        // empty string
        public string units { get { return get_attr_string("units"); } }



        protected byte[] _bin_data;
    }
}
