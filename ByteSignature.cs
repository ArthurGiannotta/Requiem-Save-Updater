using System;
using System.Collections.Generic;
using System.Text;

namespace Requiem_Save_Updater
{
    internal class ByteSignature
    {
        public byte[] Bytes { get; private set; }

        public string Mask { get; private set; }

        public int Padding { get; private set; } = 0x0;

        public string Pattern { get; private set; }

        private static readonly uint[] Lookup32 = CreateLookup32();

        public ByteSignature(string pattern, int padding = 0x0)
        {
            Padding = padding;
            Pattern = pattern.Trim();

            if (string.IsNullOrEmpty(Pattern))
            {
                Bytes = Array.Empty<byte>();
                Mask = "";
            }
            else
            {
                List<byte> bytes = new();
                StringBuilder mask = new();

                foreach (string hex in Pattern.Split(' '))
                {
                    if (hex == "?" || hex == "??")
                    {
                        bytes.Add(0);
                        mask.Append('?');
                    }
                    else
                    {
                        bytes.Add((byte)Convert.ToInt32(hex, 16));
                        mask.Append('x');
                    }
                }

                Bytes = bytes.ToArray();
                Mask = mask.ToString();
            }
        }

        public int FindPattern(byte[] memory, int skip = 0)
        {
            for (int index = 0; index < memory.Length - Mask.Length + 1; ++index)
            {
                bool found = true;

                for (int mask = 0; mask < Mask.Length; ++mask)
                {
                    found = Mask[mask] == '?' || memory[mask + index] == Bytes[mask];

                    if (!found)
                    {
                        break;
                    }
                }

                if (found)
                {
                    if (skip <= 0)
                    {
                        return index + Padding;
                    }

                    --skip;
                }
            }

            return -1;
        }

        public static implicit operator bool(ByteSignature signature)
        {
            return signature.Bytes.Length > 0;
        }

        public static implicit operator ByteSignature(string str)
        {
            return new ByteSignature(str);
        }

        private static uint[] CreateLookup32()
        {
            var lookup = new uint[256];

            for (int index = 0; index < 256; ++index)
            {
                string hex = index.ToString("X2");
                lookup[index] = hex[0] + ((uint)hex[1] << 0x10);
            }

            return lookup;
        }
    }
}
