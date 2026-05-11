using System;
using System.IO;
using System.Text;

namespace EVCharging.Client.Infrastructure
{
    public static class CsvMemoryBuffer
    {
        public static MemoryStream CreateReadableBuffer(string line)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(line ?? string.Empty);
            return new MemoryStream(bytes, false);
        }

        public static string ReadFromBuffer(MemoryStream memoryStream)
        {
            if (memoryStream == null)
            {
                throw new ArgumentNullException("memoryStream");
            }

            memoryStream.Position = 0;
            using (var reader = new StreamReader(memoryStream, Encoding.UTF8, true, 1024, true))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
