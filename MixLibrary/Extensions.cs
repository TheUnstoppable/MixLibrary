/*  
    MIX Package/File Parser
    Copyright (c) 2021 Unstoppable
    You can redistribute or modify this code under GNU General Public License v3.0.
    The permission given to run this code in a closed source project modified.
    But, you have to release the source code using this library must be released.
    Or, you have to add original owner's name into your project.
*/


using System.IO;
using System.Text;

namespace MixLibrary
{
    internal static class Extensions
    {
        public static byte[] Read(this MemoryStream Stream, int Count) => Read(Stream, Count, 0);
        public static byte[] Read(this MemoryStream Stream, int Count, int Offset)
        {
            byte[] Data = new byte[Count];
            Stream.Read(Data, Offset, Count);
            return Data;
        }

        public static string ReadString(this MemoryStream Stream, int Count) => ReadString(Stream, Count, 0);
        public static string ReadString(this MemoryStream Stream, int Count, int Offset)
        {
            string Data = string.Empty;
            byte[] arr = Read(Stream, Count, Offset);

            foreach (byte b in arr)
                Data += (char)b;
            return Data;
        }

        public static void Write(this MemoryStream Stream, byte[] Data) => Write(Stream, Data, 0);
        public static void Write(this MemoryStream Stream, byte[] Data, int Offset) => Stream.Write(Data, Offset, Data.Length);

        public static void WriteString(this MemoryStream Stream, string Data) => WriteString(Stream, Data, 0);
        public static void WriteString(this MemoryStream Stream, string Data, int Offset)
        {
            var Bytes = Encoding.Default.GetBytes(Data);
            Stream.Write(Bytes, Offset, Bytes.Length);
        }
    }
}
