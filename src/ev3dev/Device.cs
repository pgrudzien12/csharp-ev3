using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ev3dev
{
    public class Device
    {
        public static string SYS_ROOT = "/sys/";
        protected string _path;
        protected int _deviceIndex = -1;

        public bool Connect(string classDir,
               string pattern,
               IDictionary<string, string[]> match)
        {

            int pattern_length = pattern.Length;

            if (!Directory.Exists(classDir))
            {
                return false;
            }

            var dirs = Directory.EnumerateDirectories(classDir);
            foreach (var currentFullDirPath in dirs)
            {
                var dirName = Path.GetFileName(currentFullDirPath);
                if (dirName.StartsWith(pattern))
                {
                    _path = Path.Combine(classDir, dirName);

                    bool bMatch = true;
                    foreach (var m in match)
                    {
                        var attribute = m.Key;
                        var matches = m.Value;
                        var strValue = get_attr_string(attribute);

                        if (matches.Any() && !string.IsNullOrEmpty(matches.First()) 
                            && !matches.Any(x=>x == strValue))
                        {
                            bMatch = false;
                            break;
                        }
                    }

                    if (bMatch)
                    {
                        return true;
                    }

                    _path = null;
                }
            }
            return false;

        }

        public bool Connected
        {
            get { return !string.IsNullOrEmpty(_path); }
        }

        public int DeviceIndex
        {
            get
            {
                if (!Connected)
                    throw new NotSupportedException("no device connected");

                if (_deviceIndex < 0)
                {
                    int f = 1;
                    _deviceIndex = 0;
                    foreach (char c in _path.Where(char.IsDigit))
                    {
                        _deviceIndex += (int)char.GetNumericValue(c) * f;
                        f *= 10;
                    }
                }

                return _deviceIndex;
            }
        }


        //int get_attr_int(const std::string &name) const;

        public int get_attr_int(string name)
        {
            if (!Connected)
                throw new NotSupportedException("no device connected");

            using (StreamReader os = OpenStreamReader(name))
            {
                return int.Parse(os.ReadToEnd());
            }
        }

        public void set_attr_int(string name, int value)
        {
            if (!Connected)
                throw new NotSupportedException("no device connected");

            using (StreamWriter os = OpenStreamWriter(name))
            {
                os.Write(value);
            }
        }

        public string get_attr_string(string name)
        {
            if (!Connected)
                throw new NotSupportedException("no device connected");

            using (StreamReader os = OpenStreamReader(name))
            {
                return os.ReadToEnd();
            }
        }

        public void set_attr_string(string name,
                              string value)
        {
            if (!Connected)
                throw new NotSupportedException("no device connected");

            using (StreamWriter os = OpenStreamWriter(name))
            {
                os.Write(value);
            }
        }

        public string get_attr_line(string name)
        {
            if (!Connected)
                throw new NotSupportedException("no device connected");

            using (StreamReader os = OpenStreamReader(name))
            {
                return os.ReadLine();
            }
        }

        public string[] get_attr_set(string name)
        {
            string s = get_attr_line(name);
            return s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] get_attr_set(string name, out string pCur)
        {
            string[] result = get_attr_set(name);
            var bracketedValue = result.FirstOrDefault(s => s.StartsWith("["));
            pCur = bracketedValue.Substring(1, bracketedValue.Length - 2);
            return result;
        }

        private StreamReader OpenStreamReader(string name)
        {
            return new StreamReader(Path.Combine(_path, name));
        }

        private StreamWriter OpenStreamWriter(string name)
        {
            return new StreamWriter(Path.Combine(_path, name));
        }
    }
}
