using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Tools.FileOperations
{
    public static class TextFileReader
    {
        public static bool TryReadFromFile(string? file, out JObject? result, out Exception? error)
        {
            result = null;

            if (!TryReadFromFile(file, out string? text, out error))
            {
                return false;
            }

            try
            {
                result = JObject.Parse(text);
                return result is not null;
            }
            catch (Exception e)
            {
                error = e;
                return false;
            }
        }
        public static bool TryReadFromFile(string? file, out string? result, out Exception? error)
        {
            result = null;
            error = null;

            try
            {
                result = File.ReadAllText(file);
                return result is not null;
            }
            catch (Exception e)
            {
                error = e;
                return false;
            }
        }
    }
}
