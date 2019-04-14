using log4net;
using RadarBidClient.dm;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarBidClient
{

    public class CommandMessageProcessor : MessageProcessor
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(CommandMessageProcessor));



        private BidderMocker biderMocker;

        public CommandMessageProcessor(BidderMocker biderMocker)
        {
            this.biderMocker = biderMocker;
        }

        public int messageType()
        {
            return 10002;
        }

        public void ExecuteCommand(int messageType, string command)
        {
            
            logger.InfoFormat("start execute command#{0}", command);

            CommandRequest co = parse(command);

            if (co.action == "MockLoginAndPhase1")
            {
                biderMocker.MockLoginAndPhase1();
            } 
            else if (co.action == "MockPhase2")
            {
                biderMocker.MockPhase2();
            }
            else if (co.action == "ReopenNewBidWindow")
            {
                biderMocker.ReopenNewBidWindow();
            }

            logger.InfoFormat("end execute command#{0}", command);
        }

        private CommandRequest parse(string command)
        {
            CommandRequest req = new CommandRequest();
            string comm = command.Trim();
            int len = comm.Length;

            int idx = comm.IndexOf("(");
            int idx2 = comm.IndexOf(")", idx + 1);

            if (idx > -1 && idx2 > -1)
            {
                req.action = comm.Substring(0, idx);

                if (idx2 > idx + 1)
                {
                    string[] arr = comm.Substring(idx + 1, idx2 - idx - 1).Split(',');
                    
                    List<string> lis = new List<string>();
                    foreach (string a in arr)
                    {
                        string tr = a.Trim();
                        if (tr.Length == 0)
                        {
                            continue;
                        }
                        lis.Add(tr);
                    }

                    req.args =  lis.ToArray();
                } else
                {
                    req.args = new string[0];
                }

            }
            else
            {
                req.action = comm;
                req.args = new string[0];
            }

            return req;
        }

        public ProcessResult process(RawMessage message)
        {
            this.ExecuteCommand(message.getMessageType(), message.getBodyText());

            return null;
        }
    }

    public class LoginResponseProcessor : MessageProcessor
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(LoginResponseProcessor));

        public int messageType()
        {
            return 10011;
        }

        public ProcessResult process(RawMessage message)
        {
            logger.InfoFormat("login result#{0}", message.getBodyText());

            return null;
        }
    }

    public class CommandRequest
    {
        public int messageType;

        public string action;

        public string[] args;
    }

    /// <summary>
    /// 控制程序 - 基本
    /// 1. 控制打开IE浏览器
    /// 2. 控制输入竞拍地址
    /// 3. 控制移动鼠标至特定坐标(x, y)
    /// 4. 控制点击左键
    /// 5. 控制键盘按键 - 输入文本
    /// 6. 控制特定大小截取屏幕
    /// 7. 控制 - 延时点击
    /// </summary>
    public class SmartRobot
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(SmartRobot));

        private DMControl _robot;

        public SmartRobot()
        {
            _robot = new DMControl();
        }

        // 查找符合类名或者标题名的顶层可见窗口
        public int FindWindow(string className, string title)
        {
            return _robot.FindWindow(className, title);
        }

        // 获取窗口的类名
        public string GetWindowClass(int hwnd)
        {
            return _robot.GetWindowClass(hwnd);
        }

        // 获取窗口的标题
        public string GetWindowTitle(int hwnd)
        {
            return _robot.GetWindowTitle(hwnd);
        }

        // 把鼠标移动到目的点(x, y)
        public int MoveTo(int x, int y)
        {
            return _robot.MoveTo(x, y);
        }
        
        // 按下鼠标左键
        public int LeftClick()
        {
            return _robot.LeftClick();
        }

        // 按下指定的虚拟键码
        public int KeyPressChar(string charStr)
        {
            charStr = charStr.Trim();
            if (charStr.Length > 1 || charStr.Length == 0)
            {
                throw new System.ArgumentException("charStr must be char ", charStr);
            }
            return _robot.KeyPressChar(charStr);
        }

        // 按下指定的虚拟键码(s), 输入字符串
        public int KeyPressString(string text)
        {
            var arr = text.ToCharArray();
            int ret = 0;
            foreach (char ch in arr)
            {
                ret += this.KeyPressChar(ch.ToString());
            }
            return ret;
        }

        public int FindStr(int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY)
        {
            return _robot.FindStr(x1, y1, x2, y2, text, colorFormat, sim, out intX, out intY);
        }

        public int FindStrFast(int x1, int y1, int x2, int y2, string text, string colorFormat, double sim, out int intX, out int intY)
        {
            return _robot.FindStrFast(x1, y1, x2, y2, text, colorFormat, sim, out intX, out intY);
        }

        public string OcrEx(int x1, int y1, int x2, int y2, string colorFormat, double sim)
        {
            return _robot.OcrEx(x1, y1, x2, y2, colorFormat, sim);
        }

        public string Ocr(int x1, int y1, int x2, int y2, string colorFormat, double sim)
        {
            return _robot.Ocr(x1, y1, x2, y2, colorFormat, sim);
        }

        public string OcrInFile(int x1, int y1, int x2, int y2, string picName, string colorFormat, double sim)
        {
            return _robot.OcrInFile(x1, y1, x2, y2, picName, colorFormat, sim);
        }

        public int SetDict(int index, string fileName)
        {
            return _robot.SetDict(index, fileName);
        }

        public int SetPath(string path)
        {
            return _robot.SetPath(path);
        }

        // 抓取指定区域(x1, y1, x2, y2)的图像,保存为file(JPG压缩格式)
        public int CaptureJpg(int x1, int y1, int x2, int y2, string filePath, int quality)
        {
            return _robot.CaptureJpg(x1, y1, x2, y2, filePath, quality);
        }

        public int Capture(int x1, int y1, int x2, int y2, string filePath)
        {
            return _robot.Capture(x1, y1, x2, y2, filePath);
        }

        // 获取机器码 - 需要admin权限, 不然会导致直接应用程序退出
        public string GetMachineCode()
        {
            return _robot.GetMachineCode();
        }

        public int ScreenToClient(int hwnd, ref object x, ref object y)
        {
            return _robot.ScreenToClient(hwnd, ref x, ref y);
        }


        public SimplePoint searchTextCoordXYInScreen(string colorForamt, string target)
        {
            string ret = this.OcrEx(0, 0, 2000, 2000, colorForamt, 0.8);
            logger.InfoFormat(" 2000 OCR 识别的内容是 {0}", ret);

            SimplePoint point = new SimplePoint();

            if (ret == null || ret.Length == 0)
            {
                return point;
            }

            int idx = ret.IndexOf(target);
            if (idx < 0)
            {
                return point;
            }

            string[] arr = ret.Split('|');
            int len = arr[0].Length;

            string[] xy = arr[idx + 1].Split(',');
            // TODO: 目前必须在全屏下才能成功正确找到 确定按钮
            int x = Int32.Parse(xy[0]);
            int y = Int32.Parse(xy[1]);

            point.x = x;
            point.y = y;

            return point;
        }

        public SimplePoint searchTextCoordXYInFlashScreen(int x1, int y1, string colorForamt, string target)
        {
            string ret = this.OcrEx(x1, y1, x1+900, y1+700, colorForamt, 0.8);
            logger.InfoFormat("900 OCR 识别的内容是 {0}, {1}. {2}", x1, y1, ret);

            SimplePoint point = new SimplePoint();

            if (ret == null || ret.Length == 0)
            {
                return point;
            }

            int idx = ret.IndexOf(target);
            if (idx < 0)
            {
                return point;
            }

            string[] arr = ret.Split('|');
            int len = arr[0].Length;

            string[] xy = arr[idx + 1].Split(',');
            // TODO: 目前必须在全屏下才能成功正确找到 确定按钮
            int x = Int32.Parse(xy[0]);
            int y = Int32.Parse(xy[1]);

            point.x = x;
            point.y = y;

            return point;
        }

    }
}
