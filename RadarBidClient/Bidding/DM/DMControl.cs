using Radar.Bidding.Model;
using Radar.Common;
using Radar.Common.Threads;
using Radar.IoC;
using Radar.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Radar.Bidding.DM
{

    // [ComponentRegister]
    public class DMControl : IDisposable
    {

        const string DM_REF_PATH = "resource/dlls/dm.dll";

        // public static readonly DMControl me = new DMControl();

        private bool EnableAsync = false;

        private IntPtr _dm = IntPtr.Zero;
        private bool disposed = false;

        public IntPtr DM
        {
            get { return _dm; }
            set { _dm = value; }
        }

        public DMControl()
        {
            _dm = DMCRef.CreateDM(DM_REF_PATH);
        }


        public void SetEnableAsync(bool EnableAsync)
        {
            this.EnableAsync = EnableAsync;
        }

        public string Ver()
        {
            return Marshal.PtrToStringUni(DMCRef.Ver(_dm));
        }

        public int SetPath(string path)
        {
            return DMCRef.SetPath(_dm, path);
        }

        public string GetPath()
        {
            return Marshal.PtrToStringUni(DMCRef.GetPath(_dm));
        }

        public string GetBasePath()
        {
            return Marshal.PtrToStringUni(DMCRef.GetBasePath(_dm));
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
                return DMCRef.FindWindow(_dm, className, title);
            } catch (Exception)
            {
                return -410;
            }
        }

        // 获取窗口的类名
        public string GetWindowClass(int hwnd)
        {
            try
            {
                return DMCRef.GetWindowClass(_dm, hwnd);
            } catch (Exception)
            {
                return "-410";
            }
        }

        // 获取窗口的标题
        public string GetWindowTitle(int hwnd)
        {
            try
            {
                return DMCRef.GetWindowTitle(_dm, hwnd);
            } catch(Exception)
            {
                return "-410";
            }
        }


        // 获取窗口客户区域在屏幕上的位置
        public int GetClientRect(int hwnd, out int x1, out int y1, out int x2, out int y2)
        {
            return DMCRef.GetClientRect(_dm, hwnd, out x1, out y1, out x2, out y2);
        }

        public int ScreenToClient(int hwnd, ref object x, ref object y)
        {
            return DMCRef.ScreenToClient(_dm, hwnd, ref x, ref y);
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

        private int Execute01(UserTask task)
        {
            int ret = -1;
            if (EnableAsync)
            {
                SingleThread.Execute(task, null, (taskStatus) => {
                    ret = (int) taskStatus.ReturnData;
                });
            }
            else
            {
                ret = (int) task.Invoke(null);
            }

            return ret;
        }

        #region 键鼠相关
        //        把鼠标移动到目的点(x, y)
        //        返回值:
        //        整形数:
        //        0:失败
        //        1:成功
        public int MoveTo(int x, int y)
        {
            return Execute01((obj) =>
            {
                return DMCRef.MoveTo(_dm, x, y);
            });
        }

        //按下鼠标左键
        public int LeftClick()
        {
            return Execute01((obj) =>
            {
                return DMCRef.LeftClick(_dm);
            });
        }

        //        按下指定的虚拟键码
        //            参数定义:
        //        key_str 字符串: 字符串描述的键码.大小写无所谓.点这里查看具体对应关系.
        //        返回值: 0:失败 1:成功
        public int KeyPressChar(string charStr)
        {
            charStr = charStr.Trim();
            if (charStr.Length > 1 || charStr.Length == 0)
            {
                throw new System.ArgumentException("charStr must be char ", charStr);
            }

            return Execute01((obj) =>
            {
                return DMCRef.KeyPressChar(_dm, charStr);
            });
        }

        // 按下指定的虚拟键码
        // vk_code 整形数:虚拟按键码
        // 返回值: 0:失败 1:成功
        public int KeyPress(int vkCode)
        {
            return Execute01((obj) =>
            {
                return DMCRef.KeyPress(_dm, vkCode);
            });
            
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
            return Execute01((obj) =>
            {
                return DMCRef.CaptureJpg(_dm, x1, y1, x2, y2, filePath, quality);
            });
        }

        public int Capture(int x1, int y1, int x2, int y2, string filePath)
        {
            return Execute01((obj) =>
            {
                return DMCRef.Capture(_dm, x1, y1, x2, y2, filePath);
            });
        }

        #endregion

        #region 系统相关
        public string GetMachineCode()
        {
            return Marshal.PtrToStringUni(DMCRef.GetMachineCode(_dm));
        }



        #endregion

        #region OCR相关

        // 在屏幕范围(x1,y1,x2,y2)内,查找string(可以是任意个字符串的组合),并返回符合color_format的坐标位置,相似度sim同Ocr接口描述.
        public int FindStr(int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY)
        {
            return DMCRef.FindStr(_dm, x1, y1, x2, y2, text, colorFormat, sim, out intX, out intY);
        }

        
        public int FindStrFast(int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY)
        {
            return DMCRef.FindStrFast(_dm, x1, y1, x2, y2, text, colorFormat, sim, out intX, out intY);
        }

        //
        public string Ocr(int x1, int y1, int x2, int y2, string color, double sim)
        {
            return Marshal.PtrToStringUni(DMCRef.Ocr(_dm, x1, y1, x2, y2, color, sim));
        }

        // 
        public string OcrEx(int x1, int y1, int x2, int y2, string colorFormat, double sim)
        {
            return Marshal.PtrToStringUni(DMCRef.OcrEx(_dm, x1, y1, x2, y2, colorFormat, sim));
        }

        // 识别位图中区域(x1,y1,x2,y2)的文字
        public string OcrInFile(int x1, int y1, int x2, int y2, string picName, string colorFormat, double sim)
        {
            return DMCRef.OcrInFile(_dm, x1, y1, x2, y2, picName, colorFormat, sim);
        }

        public int SetDict(int index, string fileName)
        {
            return DMCRef.SetDict(_dm, index, fileName);
        }

        public int UseDict(int index)
        {
            return DMCRef.UseDict(_dm, index);
        }

        public int UseDict(DictIndex index)
        {
            return DMCRef.UseDict(_dm, (int) index);
        }

        #endregion

        //        解除绑定窗口,并释放系统资源.一般在OnScriptExit调用
        //        返回值:
        ///        整形数:
        ///        0: 失败
        ///        1: 成功
        public int UnBindWindow()
        {
            return DMCRef.UnBindWindow(_dm);
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
                int ret = DMCRef.FreeDM();
            }

            disposed = true;

        }

        #endregion

    }
}
