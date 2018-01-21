using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Utils
{
    internal static class ByteArrayUtils
    {
        public static bool Equals(byte[] first, byte[] second)
        {
            if (first == second) // reference equals
            {
                return true;
            }
            if (first == null || second == null) // one is null
            {
                return false;
            }
            if (first.Length != second.Length) // lengths don't match
            {
                return false;
            }
            for (int i = 0; i < first.Length; i++) // O(n) contents check
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int GetHashCode(byte[] array)
        {
            if (array == null)
            {
                return 0;
            }
            int hash = 17;
            foreach (byte element in array)
            {
                hash = hash * 31 + element.GetHashCode();
            }
            return hash;
        }
    }
}
