﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RadarBidClient.dm
{
    class DMControl : IDisposable
    {

        const string DM_REF_PATH = "dm.dll";

        const string DMC_REF_PATH = "dmc.dll";

        #region import dm dll function

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention =CallingConvention.StdCall)]
        public static extern IntPtr CreateDM(string dmPath);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FreeDM();

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Ver(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPath(IntPtr dm, string path);


        #region 窗口相关
        // 获取窗口客户区域在屏幕上的位置
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetClientRect(IntPtr dm, int hwnd, out int x1, out int y1, out int x2, out int y2);

        // 设置窗口的状态
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowState(IntPtr dm, int hwnd, int flag);

        // 查找符合类名或者标题名的顶层可见窗口
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FindWindow(IntPtr dm, string className, string title);

        // 获取窗口的类名
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string GetWindowClass(IntPtr dm, int hwnd);

        // 获取窗口的标题
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string GetWindowTitle(IntPtr dm, int hwnd);

        // 把屏幕坐标转换为窗口坐标
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ScreenToClient(IntPtr dm, int hwnd, ref object x, ref object y);


        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int UnBindWindow(IntPtr dm);

        #endregion

        #region 键鼠相关
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MoveTo(IntPtr dm, int x, int y);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int LeftClick(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int KeyPressChar(IntPtr dm, string charStr);

        #endregion

        #region 图色相关
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int CaptureJpg(IntPtr dm, int x1, int y1, int x2, int y2, string filePath, int quality);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Capture(IntPtr dm, int x1, int y1, int x2, int y2, string filePath);


        #endregion


        #region 系统相关
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string GetMachineCode(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetScreenDepth(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetScreenHeight(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetScreenWidth(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetScreen(IntPtr dm, int width, int height, int depth);

        #endregion

        #region OCR相关

        // 在屏幕范围(x1,y1,x2,y2)内,查找string(可以是任意个字符串的组合),并返回符合color_format的坐标位置,相似度sim同Ocr接口描述.
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FindStr(IntPtr dm, int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY);

        // 同FindStr。
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FindStrFast(IntPtr dm, int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY);

        // 识别屏幕范围(x1,y1,x2,y2)内符合color_format的字符串,并且相似度为sim,sim取值范围(0.1-1.0),
        // 返回识别到的字符串 格式如  "识别到的信息|x0,y0|…|xn,yn"
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string OcrEx(IntPtr dm, int x1, int y1, int x2, int y2, string colorFormat, double sim);

        // 识别位图中区域(x1,y1,x2,y2)的文字
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string OcrInFile(IntPtr dm, int x1, int y1, int x2, int y2, string picName, string colorFormat, double sim);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetDict(IntPtr dm, int index, string fileName);


        

        #endregion

        #endregion

        private IntPtr _dm = IntPtr.Zero;
        private bool disposed = false;

        public IntPtr DM
        {
            get { return _dm; }
            set { _dm = value; }
        }

        public DMControl()
        {
            _dm = CreateDM(DM_REF_PATH);
        }

        public string Ver()
        {
            return Marshal.PtrToStringUni(Ver(_dm));
        }

        public int SetPath(string path)
        {
            return SetPath(_dm, path);
        }

        #region 窗口相关

        // 查找符合类名或者标题名的顶层可见窗口
        // 需要admin权限, 必须try
        // 参数定义:
        //  class 字符串 : 窗口类名，如果为空，则匹配所有.这里的匹配是模糊匹配.
        //  title 字符串: 窗口标题, 如果为空，则匹配所有.这里的匹配是模糊匹配.
        //  返回值:
        // 整形数:
        //  整形数表示的窗口句柄，没找到返回0
        public int FindWindow(string className, string title)
        {
            try
            {
                return FindWindow(_dm, className, title);
            } catch (Exception e)
            {
                return -410;
            }
        }

        // 获取窗口的类名
        public string GetWindowClass(int hwnd)
        {
            try
            {
                return GetWindowClass(_dm, hwnd);
            } catch (Exception e)
            {
                return "-410";
            }
        }

        // 获取窗口的标题
        public string GetWindowTitle(int hwnd)
        {
            try
            {
                return GetWindowTitle(_dm, hwnd);
            } catch(Exception e)
            {
                return "-410";
            }
        }


        // 获取窗口客户区域在屏幕上的位置
        public int GetClientRect(int hwnd, out int x1, out int y1, out int x2, out int y2)
        {
            return GetClientRect(_dm, hwnd, out x1, out y1, out x2, out y2);
        }

        public int ScreenToClient(int hwnd, ref object x, ref object y)
        {
            return ScreenToClient(_dm, hwnd, ref x, ref y);
        }


        // 设置窗口的状态
        //        参数定义:
        // hwnd 整形数: 指定的窗口句柄
        // flag 整形数: 取值定义如下
        // 0 : 关闭指定窗口
        // 1 : 激活指定窗口
        // 2 : 最小化指定窗口,但不激活
        // 3 : 最小化指定窗口,并释放内存,但同时也会激活窗口.
        // 4 : 最大化指定窗口,同时激活窗口.
        // 5 : 恢复指定窗口 ,但不激活
        // 6 : 隐藏指定窗口
        // 7 : 显示指定窗口
        // 8 : 置顶指定窗口
        // 9 : 取消置顶指定窗口
        // 10 : 禁止指定窗口
        // 11 : 取消禁止指定窗口
        // 12 : 恢复并激活指定窗口
        // 13 : 强制结束窗口所在进程.
        //返回值:
        // 整形数:
        // 0: 失败
        // 1: 成功
        public int SetWindowState(int hwnd, int flag)
        {
            return SetWindowState(hwnd, flag);
        }

        #endregion

        #region 键鼠相关
        //        把鼠标移动到目的点(x, y)
        //        返回值:
        //        整形数:
        //        0:失败
        //        1:成功
        public int MoveTo(int x, int y)
        {
            return MoveTo(_dm, x, y);
        }

        //按下鼠标左键
        public int LeftClick()
        {
            return LeftClick(_dm);
        }

        //        按下指定的虚拟键码
        //            参数定义:
        //        key_str 字符串: 字符串描述的键码.大小写无所谓.点这里查看具体对应关系.
        //返回值:
        //整形数:
        //0:失败
        //1:成功
        public int KeyPressChar(string charStr)
        {
            return KeyPressChar(_dm, charStr);
        }
        #endregion

        #region 图色相关
        //        抓取指定区域(x1, y1, x2, y2)的图像,保存为file(JPG压缩格式)
        //            参数定义:
        //        x1 整形数:区域的左上X坐标
        //        y1 整形数:区域的左上Y坐标
        //        x2 整形数:区域的右下X坐标
        //        y2 整形数:区域的右下Y坐标
        //        file 字符串:保存的文件名,保存的地方一般为SetPath中设置的目录
        //             当然这里也可以指定全路径名.
        //        quality 整形数: jpg压缩比率(1-100) 越大图片质量越好
        //        返回值:
        //        整形数:
        //        0:失败
        //        1:成功
        public int CaptureJpg(int x1, int y1, int x2, int y2, string filePath, int quality)
        {
            return CaptureJpg(_dm, x1, y1, x2, y2, filePath, quality);
        }

        public int Capture(int x1, int y1, int x2, int y2, string filePath)
        {
            return Capture(_dm, x1, y1, x2, y2, filePath);
        }

        #endregion

        #region 系统相关
        public string GetMachineCode()
        {
            return GetMachineCode(_dm);
        }



        #endregion

        #region OCR相关

        // 在屏幕范围(x1,y1,x2,y2)内,查找string(可以是任意个字符串的组合),并返回符合color_format的坐标位置,相似度sim同Ocr接口描述.
        public int FindStr(int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY)
        {
            return FindStr(_dm, x1, y1, x2, y2, text, colorFormat, sim, out intX, out intY);
        }

        
        public int FindStrFast(int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY)
        {
            return FindStrFast(_dm, x1, y1, x2, y2, text, colorFormat, sim, out intX, out intY);
        }

        // 
        public string OcrEx(int x1, int y1, int x2, int y2, string colorFormat, double sim)
        {
            return OcrEx(_dm, x1, y1, x2, y2, colorFormat, sim);
        }

        // 识别位图中区域(x1,y1,x2,y2)的文字
        public string OcrInFile(int x1, int y1, int x2, int y2, string picName, string colorFormat, double sim)
        {
            return OcrInFile(_dm, x1, y1, x2, y2, picName, colorFormat, sim);
        }

        public int SetDict(int index, string fileName)
        {
            return SetDict(_dm, index, fileName);
        }

        #endregion

        //        解除绑定窗口,并释放系统资源.一般在OnScriptExit调用
        //        返回值:
        ///        整形数:
        ///        0: 失败
        ///        1: 成功
        public int UnBindWindow()
        {
            return UnBindWindow(_dm);
        }

        #region 释放
        public void Dispose()
        {
            Dispose(true);

            // 通知垃圾回收
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }

        ~DMControl()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // 清理托管資源
            }

            // 清理非托管資源
            if (_dm != IntPtr.Zero)
            {
                UnBindWindow();
                _dm = IntPtr.Zero;
                int ret = FreeDM();
            }

            disposed = true;

        }

        #endregion

    }
}
