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


        #region �������
        // ��ȡ���ڿͻ���������Ļ�ϵ�λ��
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetClientRect(IntPtr dm, int hwnd, out int x1, out int y1, out int x2, out int y2);

        // ���ô��ڵ�״̬
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowState(IntPtr dm, int hwnd, int flag);

        // ���ҷ����������߱������Ķ���ɼ�����
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FindWindow(IntPtr dm, string className, string title);

        // ��ȡ���ڵ�����
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string GetWindowClass(IntPtr dm, int hwnd);

        // ��ȡ���ڵı���
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string GetWindowTitle(IntPtr dm, int hwnd);

        // ����Ļ����ת��Ϊ��������
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ScreenToClient(IntPtr dm, int hwnd, ref object x, ref object y);


        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int UnBindWindow(IntPtr dm);

        #endregion

        #region �������
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MoveTo(IntPtr dm, int x, int y);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int LeftClick(IntPtr dm);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int KeyPressChar(IntPtr dm, string charStr);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int KeyPress(IntPtr dm, int vkCode);

        #endregion

        #region ͼɫ���
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int CaptureJpg(IntPtr dm, int x1, int y1, int x2, int y2, string filePath, int quality);

        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Capture(IntPtr dm, int x1, int y1, int x2, int y2, string filePath);


        #endregion


        #region ϵͳ���
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

        #region OCR���

        // ����Ļ��Χ(x1,y1,x2,y2)��,����string(������������ַ��������),�����ط���color_format������λ��,���ƶ�simͬOcr�ӿ�����.
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FindStr(IntPtr dm, int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY);

        // ͬFindStr��
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int FindStrFast(IntPtr dm, int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY);

        // ʶ����Ļ��Χ(x1,y1,x2,y2)�ڷ���color_format���ַ���,�������ƶ�Ϊsim,simȡֵ��Χ(0.1-1.0),
        // ����ʶ�𵽵��ַ��� ��ʽ��  "ʶ�𵽵���Ϣ|x0,y0|��|xn,yn"
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr Ocr(IntPtr dm, int x1, int y1, int x2, int y2, string colorFormat, double sim);

        // ͨocr
        [DllImport(DMC_REF_PATH, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr OcrEx(IntPtr dm, int x1, int y1, int x2, int y2, string colorFormat, double sim);

        // ʶ��λͼ������(x1,y1,x2,y2)������
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
