using System.IO;
using System.Text;

namespace EVCharging.Server.Infrastructure
{
    public static class PathSanitizer
    {
        public static string ToSafeFolderName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "UnknownVehicle";
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder();

            foreach (char c in value.Trim())
            {
                bool isInvalid = false;
                foreach (char bad in invalid)
                {
                    if (c == bad)
                    {
                        isInvalid = true;
                        break;
                    }
                }

                builder.Append(isInvalid ? '_' : c);
            }

            return builder.ToString();
        }
    }
}
