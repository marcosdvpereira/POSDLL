using System;
using System.Collections.Generic;
using System.Text;

namespace POSDLL
{
    class DataUtils
    {
        /// <summary>
        /// 将多个字节数组按顺序合并
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] byteArraysToBytes(byte[][] data)
        {

            int length = 0;
            for (int i = 0; i < data.Length; i++)
                length += data[i].Length;
            byte[] send = new byte[length];
            int k = 0;
            for (int i = 0; i < data.Length; i++)
                for (int j = 0; j < data[i].Length; j++)
                    send[k++] = data[i][j];
            return send;
        }

        public static byte[] cloneBytes(byte[] data)
        {
            byte[] ret = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                ret[i] = data[i];
            return ret;
        }

        public static byte bytesToXor(byte[] data, int start, int length)
        {
            if (length == 0)
                return 0;
            else if (length == 1)
                return data[start];
            else
            {
                int result = data[start] ^ data[start + 1];
                for (int i = start + 2; i < start + length; i++)
                    result ^= data[i];
                return (byte)result;
            }
        }

        public static void copyBytes(byte[] orgdata, int orgstart, byte[] desdata,
                int desstart, int copylen)
        {
            for (int i = 0; i < copylen; i++)
            {
                desdata[desstart + i] = orgdata[orgstart + i];
            }
        }
    }
}