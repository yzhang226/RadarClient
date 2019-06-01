using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common
{
    public class ByteUtils
    {

        /**
         *
         */
        public static readonly int MAGIC_NUMBER = 2140483647;

        // write

        public static void writeLong(byte[] data, int offset, long value)
        {
            data[offset] = (byte)((value >> 56) & 0xFF);
            data[offset + 1] = (byte)((value >> 48) & 0xFF);
            data[offset + 2] = (byte)((value >> 40) & 0xFF);
            data[offset + 3] = (byte)((value >> 32) & 0xFF);
            data[offset + 4] = (byte)((value >> 24) & 0xFF);
            data[offset + 5] = (byte)((value >> 16) & 0xFF);
            data[offset + 6] = (byte)((value >> 8) & 0xFF);
            data[offset + 7] = (byte)((value) & 0xFF);
        }

        public static void writeInt(byte[] data, int offset, int value)
        {
            data[offset] = (byte)((value >> 24) & 0xFF);
            data[offset + 1] = (byte)((value >> 16) & 0xFF);
            data[offset + 2] = (byte)((value >> 8) & 0xFF);
            data[offset + 3] = (byte)((value) & 0xFF);
        }

        public static void writeByte(byte[] data, int offset, byte value)
        {
            data[offset] = value;
        }

        public static void writeBytes(byte[] data, int offset, byte[] bs)
        {
            for (int i = 0; i < bs.Length; i++)
            {
                data[offset + i] = bs[i];
            }
        }

        // read

        public static byte read1Byte(byte[] bs, int offset)
        {
            return bs[offset];
        }

        public static byte[] read4Bytes(byte[] bs, int offset)
        {
            return new byte[] { bs[offset], bs[offset + 1], bs[offset + 2], bs[offset + 3] };
        }

        public static byte[] readFixLength(byte[] bs, int offset, int len)
        {
            byte[] tar = new byte[len];
            arraycopy(bs, offset, tar, 0, len);
            return tar;
        }

        public static int readInt(byte[] bs, int offset)
        {
            byte[] seg = read4Bytes(bs, offset);
            int ret = ((int)seg[0] & 0xFF) << 24 |
                    ((int)seg[1] & 0xFF) << 16 |
                    ((int)seg[2] & 0xFF) << 8 |
                    ((int)seg[3] & 0xFF);
            return ret;
        }

        public static long readLong(byte[] bs, int offset)
        {
            byte[] seg = readFixLength(bs, offset, 8);
            long ret = ((long)seg[0] & 0xFF) << 56 |
                    ((long)seg[1] & 0xFF) << 48 |
                    ((long)seg[2] & 0xFF) << 40 |
                    ((long)seg[3] & 0xFF) << 32 |
                    ((long)seg[4] & 0xFF) << 24 |
                    ((long)seg[5] & 0xFF) << 16 |
                    ((long)seg[6] & 0xFF) << 8 |
                    ((long)seg[7] & 0xFF)
                    ;
            return ret;
        }

        public static void arraycopy(byte[] source, int sourceOffset, byte[] target, int targetOffset, int targetLength)
        {
            int i = 0;
            for (; i < targetLength; i++)
            {
                target[targetOffset + i] = source[sourceOffset + i];
            }

        }

    }
}
