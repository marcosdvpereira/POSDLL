using System.Text;

namespace POSDLL
{
    /// <summary>
    /// 热敏打印机二次开发包
    /// 支持串口打印
    /// 支持页模式打印机和非页模式打印机
    /// </summary>
    public class Pos
    {
        //打印机模式标记
        public const int POS_PRINT_MODE_STANDARD = 0x00;
        public const int POS_PRINT_MODE_PAGE = 0x01;
        public const int POS_PRINT_MODE_BLACK_MARK_LABEL = 0x02;
        public const int POS_PRINT_MODE_WHITE_MARK_LABEL = 0x03;

        /// <summary>
        /// 复位打印机
        /// </summary>
        public static byte[] POS_Reset()
        {
            byte[] data = Cmd.ESC_ALT;
            return data;
        }

        /// <summary>
        /// 选择打印模式，该函数只对有页模式的打印机有效
        /// 对非页模式的打印机发送该命令可能会导致数据混乱
        /// </summary>
        /// <param name="nPrintMode"></param>
        /// <returns></returns>
        public static byte[] POS_SetMode(int nPrintMode)
        {
            byte[] data;
            switch (nPrintMode)
            {
                case POS_PRINT_MODE_STANDARD:
                    data = Cmd.ESC_S;
                    break;
                case POS_PRINT_MODE_PAGE:
                    data = Cmd.ESC_L;
                    break;
                default:
                    return new byte[0];
            }
            return data;
        }

        /// <summary>
        /// 设置打印机移动单位
        /// </summary>
        /// <param name="nHorizontalMU">把水平方向上的移动单位设置为 25.4 / nHorizontalMU 毫米。可以为0到255。</param>
        /// <param name="nVerticalMu">把竖直方向上的移动单位设置为 25.4 / nVerticalMU 毫米。可以为0到255。</param>
        public static byte[] POS_SetMotionUnit(int nHorizontalMU, int nVerticalMu)
        {
            if (nHorizontalMU < 0 | nHorizontalMU > 255 | nVerticalMu < 0 | nVerticalMu > 255)
                return new byte[0];

            byte[] data = Cmd.GS_P_x_y;
            data[2] = (byte)nHorizontalMU;
            data[3] = (byte)nVerticalMu;
            return data;
        }

        /// <summary>
        /// 设置字符集和代码页
        /// </summary>
        /// <param name="nCharSet"></param>
        /// <param name="nCodePage"></param>
        public static byte[] POS_SetCharSetAndCodePage(int nCharSet, int nCodePage)
        {
            if (nCharSet < 0 | nCharSet > 15 | nCodePage < 0 | nCodePage > 19 | (nCodePage > 10 & nCodePage < 16))
                return new byte[0];

            byte[] data = new byte[Cmd.ESC_R_n.Length + Cmd.ESC_t_n.Length];
            Cmd.ESC_R_n[2] = (byte)nCharSet;
            Cmd.ESC_R_n.CopyTo(data, 0);
            Cmd.ESC_t_n[2] = (byte)nCodePage;
            Cmd.ESC_t_n.CopyTo(data, Cmd.ESC_R_n.Length);
            return data;
        }

        /// <summary>
        /// 换行
        /// </summary>
        public static byte[] POS_FeedLine()
        {
            byte[] data = Cmd.CRLF;
            return data;
        }

        /// <summary>
        /// 设置行高
        /// </summary>
        /// <param name="nDistance">可以为0-255，与移动单位有关</param>
        public static byte[] POS_SetLineSpacing(int nDistance)
        {
            if (nDistance < 0 | nDistance > 255)
                return new byte[0];
            byte[] data = Cmd.ESC_3_n;
            data[2] = (byte)nDistance;
            return data;
        }

        /// <summary>
        /// 设置字符右间距
        /// </summary>
        /// <param name="nDistance">可以为0-255，与移动单位有关</param>
        public static byte[] POS_SetRightSpacing(int nDistance)
        {
            if (nDistance < 0 | nDistance > 255)
                return new byte[0];
            byte[] data = Cmd.ESC_SP_n;
            data[2] = (byte)nDistance;
            return data;
        }

        /// <summary>
        /// 产生钱箱开启脉冲
        /// </summary>
        /// <param name="nID">nID指示引脚，0为引脚2,1为引脚5。</param>
        /// <param name="nOnTimes">可以为1-8</param>
        /// <param name="nOffTimes">暂不支持nOffTimes，此参数可以为0。</param>
        public static byte[] POS_KickOutDrawer(int nID, int nOnTimes, int nOffTimes)
        {
            if (nID > 1 | nID < 0 | nOnTimes < 1 | nOnTimes > 8)
                return new byte[0];
            byte[] data = Cmd.DLE_DC4_n_m_t;
            data[3] = (byte)nID;
            data[4] = (byte)nOnTimes;
            return data;
        }

        /// <summary>
        /// 指定切纸模式并切纸，如果指定为全切，则参数 nDistance 忽略，如果指定为半切，则打印机走纸 nDistance 点，然后切纸。
        /// </summary>
        /// <param name="nMode">切纸模式，0为全切，1为半切</param>
        /// <param name="nDistance">走纸距离</param>
        public static byte[] POS_CutPaper(int nMode, int nDistance)
        {
            if (nMode < 0 | nMode > 2 | (nMode == 2 & (nDistance > 255 | nDistance < 0)))
                return new byte[0];

            byte[] data;
            switch (nMode)
            {
                case 0:
                    data = Cmd.GS_V_m;
                    data[2] = (byte)nMode;
                    break;
                case 1:
                    data = Cmd.GS_V_m_n;
                    data[3] = (byte)nDistance;
                    break;
                default:
                    return new byte[0];
            }
            return data;
        }

        public static byte[] POS_FullCutPaper()
        {
            byte[] data = Cmd.ESC_i;
            return data;
        }

        /// <summary>
        /// 设置标准模式下打印区域宽度
        /// </summary>
        /// <param name="nWidth"></param>
        public static byte[] POS_S_SetAreaWidth(int nWidth)
        {
            if (nWidth < 0 | nWidth > 65535)
                return new byte[0];

            byte nL = (byte)(nWidth % 0x100);
            byte nH = (byte)(nWidth / 0x100);
            byte[] data = Cmd.GS_W_nL_nH;
            data[2] = nL;
            data[3] = nH;
            return data;
        }

        /// <summary>
        /// 把将要打印的字符串数据发送到打印缓冲区中，并指定X 方向（水平）上的绝对起始点位置，指定每个字符宽度和高度方向上的放大倍数、类型和风格。
        /// </summary>
        /// <param name="pszString"></param>
        /// <param name="nOrgx"></param>
        /// <param name="nWidthTimes"></param>
        /// <param name="nHeightTimes"></param>
        /// <param name="nFontType"></param>
        /// <param name="nFontStyle"></param>
        public static byte[] POS_S_TextOut(string pszString, int nOrgx, int nWidthTimes, int nHeightTimes, int nFontType, int nFontStyle)
        {
            if (nOrgx > 65535 | nOrgx < 0 |
                nWidthTimes > 7 | nWidthTimes < 0 | nHeightTimes > 7 | nHeightTimes < 0 |
                nFontType < 0 | nFontType > 4)
                return new byte[0];

            Cmd.ESC_dollors_nL_nH[2] = (byte)(nOrgx % 0x100);
            Cmd.ESC_dollors_nL_nH[3] = (byte)(nOrgx / 0x100);

            byte[] intToWidth = { 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70 };
            byte[] intToHeight = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            Cmd.GS_exclamationmark_n[2] = (byte)(intToWidth[nWidthTimes] + intToHeight[nHeightTimes]);

            byte[] tmp_ESC_M_n = Cmd.ESC_M_n;
            if ((nFontType == 0) || (nFontType == 1))
                tmp_ESC_M_n[2] = (byte)nFontType;
            else
                tmp_ESC_M_n = new byte[0];

            // 字体风格
            // 暂不支持平滑处理
            Cmd.GS_E_n[2] = (byte)((nFontStyle >> 3) & 0x01);

            Cmd.ESC_line_n[2] = (byte)((nFontStyle >> 7) & 0x03);
            Cmd.FS_line_n[2] = (byte)((nFontStyle >> 7) & 0x03);

            Cmd.ESC_lbracket_n[2] = (byte)((nFontStyle >> 9) & 0x01);

            Cmd.GS_B_n[2] = (byte)((nFontStyle >> 10) & 0x01);

            Cmd.ESC_V_n[2] = (byte)((nFontStyle >> 12) & 0x01);

            byte[] pbString = Encoding.ASCII.GetBytes(pszString);

            byte[] data = DataUtils.byteArraysToBytes(new byte[][] {
                Cmd.ESC_dollors_nL_nH, Cmd.GS_exclamationmark_n,
                tmp_ESC_M_n, Cmd.GS_E_n, Cmd.ESC_line_n,
                Cmd.FS_line_n, Cmd.ESC_lbracket_n,
                Cmd.GS_B_n, Cmd.ESC_V_n, pbString });

            return data;
        }

        /// <summary>
        /// 设置并打印条码。
        /// </summary>
        /// <param name="pszInfoBuffer"></param>
        /// <param name="nOrgx"></param>
        /// <param name="nType"></param>
        /// <param name="nWidthX"></param>
        /// <param name="nHeight"></param>
        /// <param name="nHriFontType"></param>
        /// <param name="nHriFontPosition"></param>
        public static byte[] POS_S_SetBarcode(string pszInfoBuffer, int nOrgx, int nType, int nWidthX, int nHeight, int nHriFontType, int nHriFontPosition)
        {
            if (nOrgx < 0 | nOrgx > 65535 | nType < 0x41 | nType > 0x49 | nWidthX < 2 | nWidthX > 6 | nHeight < 1 | nHeight > 255)
                return new byte[0];

            byte[] pbString = Encoding.ASCII.GetBytes(pszInfoBuffer);
            int dataLength = Cmd.ESC_dollors_nL_nH.Length + Cmd.GS_w_n.Length +
                Cmd.GS_h_n.Length + Cmd.GS_f_n.Length +
                Cmd.GS_H_n.Length + Cmd.GS_k_m_n_.Length + pbString.Length;

            byte[] data = new byte[dataLength];
            int offset = 0;
            Cmd.ESC_dollors_nL_nH[2] = (byte)(nOrgx % 0x100);
            Cmd.ESC_dollors_nL_nH[3] = (byte)(nOrgx / 0x100);
            Cmd.ESC_dollors_nL_nH.CopyTo(data, offset);
            offset += Cmd.ESC_dollors_nL_nH.Length;
            Cmd.GS_w_n[2] = (byte)nWidthX;
            Cmd.GS_w_n.CopyTo(data, offset);
            offset += Cmd.GS_w_n.Length;
            Cmd.GS_h_n[2] = (byte)nHeight;
            Cmd.GS_h_n.CopyTo(data, offset);
            offset += Cmd.GS_h_n.Length;
            Cmd.GS_f_n[2] = (byte)(nHriFontType & 0x01);
            Cmd.GS_f_n.CopyTo(data, offset);
            offset += Cmd.GS_f_n.Length;
            Cmd.GS_H_n[2] = (byte)(nHriFontPosition & 0x03);
            Cmd.GS_H_n.CopyTo(data, offset);
            offset += Cmd.GS_H_n.Length;
            Cmd.GS_k_m_n_[2] = (byte)nType;
            Cmd.GS_k_m_n_[3] = (byte)pbString.Length;
            Cmd.GS_k_m_n_.CopyTo(data, offset);
            offset += Cmd.GS_k_m_n_.Length;
            pbString.CopyTo(data, offset);

            return data;
        }

        public static string lasterror = "no error";

        //public static Bitmap POS_ResizeBitmap(Bitmap orgBitmap, int newWidth, int newHeight)
        //{
        //    try
        //    {
        //        Bitmap b = new Bitmap(newWidth, newHeight);
        //        Graphics g = Graphics.FromImage(b);

        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        g.DrawImage(orgBitmap, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, orgBitmap.Width, orgBitmap.Height), GraphicsUnit.Pixel);
        //        g.Dispose();
        //        return b;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        /* 将位图转换为256色光栅位图流数据 */
        /* byte[height][width] */
        //public static byte[][] POS_BitmapToStream(Bitmap orgBitmap)
        //{
        //    try
        //    {
        //        int widthPix = orgBitmap.Width;
        //        int heightPix = orgBitmap.Height;
        //        Rectangle rct = new Rectangle(0, 0, widthPix, heightPix);
        //        System.Drawing.Imaging.BitmapData bmpData = orgBitmap.LockBits(rct, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        //        //先将像素复制出来
        //        byte[][] bmpBitData = new byte[heightPix][];
        //        unsafe
        //        {
        //            byte* pBmpData = (byte*)bmpData.Scan0;
        //            double gray = 0;

        //            for (int i = 0; i < heightPix; i++)
        //            {
        //                bmpBitData[i] = new byte[widthPix];
        //                for (int j = 0; j < widthPix; j++)
        //                {
        //                    gray = *(pBmpData + 1) * 0.3 + *(pBmpData + 2) * 0.59 + *(pBmpData + 3) * 0.11;
        //                    bmpBitData[i][j] = (byte)gray;
        //                    pBmpData += 4;
        //                }
        //                pBmpData += bmpData.Stride - bmpData.Width * 4;
        //            }
        //        }
        //        orgBitmap.UnlockBits(bmpData);
        //        return bmpBitData;
        //    }
        //    catch (Exception Mistake)
        //    {
        //        lasterror = Mistake.ToString();
        //        return null;
        //    }
        //}

        // 16*16
        private static int[,] Floyd16x16 = /* Traditional Floyd ordered dither */
	{
            { 0, 128, 32, 160, 8, 136, 40, 168, 2, 130, 34, 162, 10, 138, 42,
                    170 },
            { 192, 64, 224, 96, 200, 72, 232, 104, 194, 66, 226, 98, 202, 74,
                    234, 106 },
            { 48, 176, 16, 144, 56, 184, 24, 152, 50, 178, 18, 146, 58, 186,
                    26, 154 },
            { 240, 112, 208, 80, 248, 120, 216, 88, 242, 114, 210, 82, 250,
                    122, 218, 90 },
            { 12, 140, 44, 172, 4, 132, 36, 164, 14, 142, 46, 174, 6, 134, 38,
                    166 },
            { 204, 76, 236, 108, 196, 68, 228, 100, 206, 78, 238, 110, 198, 70,
                    230, 102 },
            { 60, 188, 28, 156, 52, 180, 20, 148, 62, 190, 30, 158, 54, 182,
                    22, 150 },
            { 252, 124, 220, 92, 244, 116, 212, 84, 254, 126, 222, 94, 246,
                    118, 214, 86 },
            { 3, 131, 35, 163, 11, 139, 43, 171, 1, 129, 33, 161, 9, 137, 41,
                    169 },
            { 195, 67, 227, 99, 203, 75, 235, 107, 193, 65, 225, 97, 201, 73,
                    233, 105 },
            { 51, 179, 19, 147, 59, 187, 27, 155, 49, 177, 17, 145, 57, 185,
                    25, 153 },
            { 243, 115, 211, 83, 251, 123, 219, 91, 241, 113, 209, 81, 249,
                    121, 217, 89 },
            { 15, 143, 47, 175, 7, 135, 39, 167, 13, 141, 45, 173, 5, 133, 37,
                    165 },
            { 207, 79, 239, 111, 199, 71, 231, 103, 205, 77, 237, 109, 197, 69,
                    229, 101 },
            { 63, 191, 31, 159, 55, 183, 23, 151, 61, 189, 29, 157, 53, 181,
                    21, 149 },
            { 254, 127, 223, 95, 247, 119, 215, 87, 253, 125, 221, 93, 245,
                    117, 213, 85 } };


        public static void format_K_dither16x16(byte[][] orgpixels, byte[][] despixels)
        {
            int ysize = orgpixels.Length;
            int xsize = orgpixels[0].Length;
            for (int y = 0; y < ysize; y++)
            {
                for (int x = 0; x < xsize; x++)
                {

                    if ((orgpixels[y][x] & 0xff) > Floyd16x16[x & 15, y & 15])
                        despixels[y][x] = 0;// black
                    else
                        despixels[y][x] = 1;
                }
            }
        }

        //将bit点阵数据转成水平排列的光栅数据，返回字节。每水平8个位一个字节，不足则补齐
        public static byte[] TurnBitStreamToByte(byte[][] orgData)
        {
            int orgHeight = orgData.Length;
            int orgWidth = orgData[0].Length;
            int desHeight = orgHeight;//转成8的倍数之后的高和宽，为了速度，这里暂时不新建二维数组，直接用一维
            int desWidth = ((orgWidth + 7) / 8) * 8;//

            int nDataSum = desHeight * desWidth / 8;//
            byte[] dataToSend = new byte[nDataSum];

            //这个算法比较精简
            byte[] bitToByte = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            for (int i = 0, k = 0; i < desHeight; i++)
            {
                for (int j = 0; j < desWidth; j = j + 8)
                {
                    if (j + 8 < orgWidth)
                    {
                        for (int m = 0; m < 8; m++)
                            if (orgData[i][j + m] != 0)
                                dataToSend[k] |= bitToByte[m];
                    }
                    else
                    {
                        for (int m = 0; m < 8 - (desWidth - orgWidth); m++)
                            if (orgData[i][j + m] != 0)
                                dataToSend[k] |= bitToByte[m];
                    }
                    k++;
                }
            }
            return dataToSend;
        }

        /* 外部调用，直接打印位图 */
        /* 该函数不会改变位图宽度，只会将位图的数据转为打印机可打印的流数据 */
        //public static void POS_PrintBitmap(Bitmap orgBitmap)
        //{
        //    byte[][] data = POS_BitmapToStream(orgBitmap);
        //    if (null == data)
        //        return;
        //    format_K_dither16x16(data, data);
        //    byte[] combineddata = TurnBitStreamToByte(data);
        //    byte[] headdata = new byte[8];
        //    int widthbytes = (data[0].Length + 7) / 8;
        //    int heightbits = data.Length;
        //    headdata[0] = 0x1d;
        //    headdata[1] = 0x76;
        //    headdata[2] = 0x30;
        //    headdata[3] = 0x00;
        //    headdata[4] = (byte)(widthbytes % 256);
        //    headdata[5] = (byte)(widthbytes / 256);
        //    headdata[6] = (byte)(heightbits % 256);
        //    headdata[7] = (byte)(heightbits / 256);
        //    POS_Write(headdata);
        //    POS_Write(combineddata);
        //}

        public static byte[] POS_PL_SetArea(int nOrgx, int nOrgy, int nWidth, int nHeight, int nDirection)
        {
            if (nOrgx < 0 | nOrgx > 65535 | nOrgy < 0 | nOrgy > 65535 | nWidth < 0 | nWidth > 65535 | nHeight < 0 | nHeight > 65535)
                return new byte[0];

            int dataLength = Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH.Length + Cmd.ESC_T_n.Length;

            byte[] data = new byte[dataLength];
            int offset = 0;
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[2] = (byte)(nOrgx % 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[3] = (byte)(nOrgx / 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[4] = (byte)(nOrgy % 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[5] = (byte)(nOrgy / 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[6] = (byte)(nWidth % 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[7] = (byte)(nWidth / 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[8] = (byte)(nHeight % 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH[9] = (byte)(nHeight / 0x100);
            Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH.CopyTo(data, offset);
            offset += Cmd.ESC_W_xL_xH_yL_yH_dxL_dxH_dyL_dyH.Length;
            Cmd.ESC_T_n[2] = (byte)(nDirection & 0x03);
            Cmd.ESC_T_n.CopyTo(data, offset);

            return data;
        }

        public static byte[] POS_PL_TextOut(string pszString, int nOrgx, int nOrgy, int nWidthTimes, int nHeightTimes, int nFontType, int nFontStyle)
        {
            if (nOrgx < 0 | nOrgx > 65535 | nOrgy < 0 | nOrgy > 65535 | nWidthTimes < 0 | nWidthTimes > 7 | nHeightTimes < 0 | nHeightTimes > 7 | nFontType < 0 | nFontType > 3)
                return new byte[0];

            byte[] pbString = Encoding.ASCII.GetBytes(pszString);
            int dataLength = pbString.Length + Cmd.GS_backslash_nL_nH.Length + Cmd.GS_dollors_nL_nH.Length +
                Cmd.GS_exclamationmark_n.Length + Cmd.ESC_M_n.Length + Cmd.GS_E_n.Length +
                Cmd.ESC_line_n.Length + Cmd.GS_B_n.Length;

            byte[] data = new byte[dataLength];
            int offset = 0;
            Cmd.ESC_dollors_nL_nH[2] = (byte)(nOrgx % 0x100);
            Cmd.ESC_dollors_nL_nH[3] = (byte)(nOrgx / 0x100);
            Cmd.ESC_dollors_nL_nH.CopyTo(data, offset);
            offset += Cmd.ESC_dollors_nL_nH.Length;
            Cmd.GS_dollors_nL_nH[2] = (byte)(nOrgy % 0x100);
            Cmd.GS_dollors_nL_nH[3] = (byte)(nOrgy / 0x100);
            Cmd.GS_dollors_nL_nH.CopyTo(data, offset);
            offset += Cmd.GS_dollors_nL_nH.Length;
            byte[] intToWidth = { 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70 };
            byte[] intToHeight = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            Cmd.GS_exclamationmark_n[2] = (byte)(intToWidth[nWidthTimes] + intToHeight[nHeightTimes]);
            Cmd.GS_exclamationmark_n.CopyTo(data, offset);
            offset += Cmd.GS_exclamationmark_n.Length;
            Cmd.ESC_M_n[2] = (byte)nFontType;
            Cmd.ESC_M_n.CopyTo(data, offset);
            offset += Cmd.ESC_M_n.Length;

            //字体风格
            //暂不支持平滑处理
            Cmd.GS_E_n[2] = (byte)((nFontStyle >> 3) & 0x01);
            Cmd.GS_E_n.CopyTo(data, offset);
            offset += Cmd.GS_E_n.Length;
            Cmd.FS_line_n[2] = (byte)((nFontStyle >> 7) & 0x03);
            Cmd.FS_line_n.CopyTo(data, offset);
            offset += Cmd.FS_line_n.Length;
            Cmd.GS_B_n[2] = (byte)((nFontStyle >> 10) & 0x01);
            Cmd.GS_B_n.CopyTo(data, offset);
            offset += Cmd.GS_B_n.Length;

            pbString.CopyTo(data, offset);
            return data;
        }

        public static byte[] POS_PL_SetBarcode(string pszInfoBuffer, int nOrgx, int nOrgy, int nType, int nWidthX, int nHeight, int nHriFontType, int nHriFontPosition)
        {
            if (nOrgx < 0 | nOrgx > 65535 | nOrgy < 0 | nOrgy > 65535 | nType < 0x41 | nType > 0x49 | nWidthX < 2 | nWidthX > 6 | nHeight < 1 | nHeight > 255)
                return new byte[0];

            byte[] pbString = Encoding.ASCII.GetBytes(pszInfoBuffer);
            int dataLength = Cmd.ESC_dollors_nL_nH.Length + Cmd.GS_dollors_nL_nH.Length + Cmd.GS_w_n.Length +
                Cmd.GS_h_n.Length + Cmd.GS_f_n.Length + Cmd.GS_H_n.Length + Cmd.GS_k_m_n_.Length + pbString.Length;

            byte[] data = new byte[dataLength];
            int offset = 0;
            Cmd.ESC_dollors_nL_nH[2] = (byte)(nOrgx % 0x100);
            Cmd.ESC_dollors_nL_nH[3] = (byte)(nOrgx / 0x100);
            Cmd.ESC_dollors_nL_nH.CopyTo(data, offset);
            offset += Cmd.ESC_dollors_nL_nH.Length;
            Cmd.GS_dollors_nL_nH[2] = (byte)(nOrgy % 0x100);
            Cmd.GS_dollors_nL_nH[3] = (byte)(nOrgy / 0x100);
            Cmd.GS_dollors_nL_nH.CopyTo(data, offset);
            offset += Cmd.GS_dollors_nL_nH.Length;
            Cmd.GS_w_n[2] = (byte)nWidthX;
            Cmd.GS_w_n.CopyTo(data, offset);
            offset += Cmd.GS_w_n.Length;
            Cmd.GS_h_n[2] = (byte)nHeight;
            Cmd.GS_h_n.CopyTo(data, offset);
            offset += Cmd.GS_h_n.Length;
            Cmd.GS_f_n[2] = (byte)(nHriFontType & 0x01);
            Cmd.GS_f_n.CopyTo(data, offset);
            offset += Cmd.GS_f_n.Length;
            Cmd.GS_H_n[2] = (byte)(nHriFontPosition & 0x03);
            Cmd.GS_H_n.CopyTo(data, offset);
            offset += Cmd.GS_H_n.Length;
            Cmd.GS_k_m_n_[2] = (byte)nType;
            Cmd.GS_k_m_n_[3] = (byte)pbString.Length;
            Cmd.GS_k_m_n_.CopyTo(data, offset);
            offset += Cmd.GS_k_m_n_.Length;
            pbString.CopyTo(data, offset);

            return data;
        }

        public static byte[] POS_PL_Print()
        {
            byte[] data = Cmd.ESC_FF;
            return data;
        }

        public static byte[] POS_PL_Clear()
        {
            byte[] data = Cmd.ESC_CAN;
            return data;
        }

    }
}