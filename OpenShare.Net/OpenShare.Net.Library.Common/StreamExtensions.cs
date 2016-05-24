using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenShare.Net.Library.Common
{
    public static class StreamExtensions
    {
        public static byte[] GetStreamBytes(this Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, Convert.ToInt32(stream.Length));
            return buffer;
        }

        public static async Task<byte[]> GetStreamBytesAsync(this Stream stream)
        {
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, Convert.ToInt32(stream.Length));
            return buffer;
        }

        public static byte[] GetStreamBytes(this Stream stream, int length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        public static async Task<byte[]> GetStreamBytesAsync(this Stream stream, int length)
        {
            var buffer = new byte[length];
            await stream.ReadAsync(buffer, 0, length);
            return buffer;
        }
    }
}
