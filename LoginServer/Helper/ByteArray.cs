using System;
using System.Collections.Generic;

namespace LoginServer.Helper
{
    public static class ByteArray
    {
        public static List<byte[]> SplitByteArray(byte[] array, int length)
        {
            int arrayLength = array.Length;
            List<byte[]> splitted = new List<byte[]>();

            for (int i = 0; i < arrayLength; i = i + length)
            {
                byte[] val = new byte[length];

                if (arrayLength < i + length)
                {
                    length = arrayLength - i;
                }
                Array.Copy(array, i, val, 0, length);
                splitted.Add(val);
            }

            return splitted;
        }
    }
}
