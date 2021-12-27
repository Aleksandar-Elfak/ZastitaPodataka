using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    internal class CRC
    {

        public static string Hash(string plainText)
        {

            uint crc = 0xFFFFFFFF;
            char[] chars = plainText.ToCharArray();

            for (int i = 0; i < plainText.Length; i++)
            {
                char c = chars[i];
                for (int j = 0; j < 8; j++)
                {
                    uint b = (c ^ crc) & 1;
                    crc >>= 1;
                    if (b == 1)
                        crc = crc ^ 0xEDB88320;
                    c >>= 1;
                }
            }

            return (~crc).ToString("X");
        }
    }
}
