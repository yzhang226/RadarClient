using log4net;
using RadarBidClient.dm;
using RadarBidClient.ioc;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarBidClient
{
    /// <summary>
    /// 窗体模拟器 - 控制鼠标 键盘输入 截屏
    /// </summary>
    [Component]
    public class WindowSimulator : DMControl
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(WindowSimulator));

        

        public WindowSimulator()
        {
        }



        // 按下指定的虚拟键码
        public int KeyPressChar(char ch)
        {
            return base.KeyPressChar(ch.ToString());
        }

        // 按下指定的虚拟键码(s), 输入字符串
        public int KeyPressString(string text)
        {
            var arr = text.ToCharArray();
            int ret = 0;
            foreach (char ch in arr)
            {
                ret += base.KeyPressChar(ch.ToString());
            }
            return ret;
        }

        // 按下指定的虚拟键码 - 删除键
        public int PressDeleteKey()
        {
            // press delete key
            return base.KeyPress(46);
        }

        // 按下指定的虚拟键码 - 退格键
        public int PressBackspacKey()
        {
            // press delete key
            return base.KeyPress(8);
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
            return searchTextCoordXYInFlashScreen(x1, y1, 900, 700, colorForamt, target);
        }

        public SimplePoint searchTextCoordXYInFlashScreen(int x1, int y1, int width, int height, string colorForamt, string target)
        {
            long s1 = KK.currentTs();
            string ret = this.OcrEx(x1, y1, x1 + width, y1 + height, colorForamt, 0.8);
            logger.InfoFormat("{0} OCR 识别的内容是 {1}, {2}. elapsed {3}ms, ret is {4}", width, x1, y1, KK.currentTs() - s1, ret);

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
            int x = int.Parse(xy[0]);
            int y = int.Parse(xy[1]);

            point.x = x;
            point.y = y;

            return point;
        }

    }
}
