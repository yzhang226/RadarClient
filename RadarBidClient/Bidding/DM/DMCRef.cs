using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Radar.Bidding.DM
{
    public class DMCRef
    {
        const string DMC_REF_PATH = "resource/dlls/dmc.dll";

        static readonly Radar.Bidding.DM.DMCRef me = new Radar.Bidding.DM.DMCRef();

        private DMCRef()
        {

        }

        #region import dm dll function

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateDM(string dmPath);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FreeDM();

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Ver(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPath(IntPtr dm, string path);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetPath(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetBasePath(IntPtr dm);


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

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int KeyPress(IntPtr dm, int vkCode);

        #endregion

        #region 图色相关
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int CaptureJpg(IntPtr dm, int x1, int y1, int x2, int y2, string filePath, int quality);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Capture(IntPtr dm, int x1, int y1, int x2, int y2, string filePath);


        #endregion


        #region 系统相关
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetMachineCode(IntPtr dm);

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
        public static extern IntPtr Ocr(IntPtr dm, int x1, int y1, int x2, int y2, string colorFormat, double sim);

        // 通ocr
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr OcrEx(IntPtr dm, int x1, int y1, int x2, int y2, string colorFormat, double sim);

        // 识别位图中区域(x1,y1,x2,y2)的文字
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string OcrInFile(IntPtr dm, int x1, int y1, int x2, int y2, string picName, string colorFormat, double sim);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetDict(IntPtr dm, int index, string fileName);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int UseDict(IntPtr dm, int index);

        #endregion

        #endregion
    }
}
