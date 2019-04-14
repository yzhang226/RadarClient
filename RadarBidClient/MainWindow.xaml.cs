using log4net;
using log4net.Config;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RadarBidClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(MainWindow));

        private SmartRobot robot;

        private SocketClient socketClient;

        private int x1 = 200;
        private int y1 = 100;

        private int w1 = 1800;

        private int h1 = 1900;

        private BidderMocker biderMocker;

        public MainWindow()
        {
            InitializeComponent();

            robot = new SmartRobot();
            biderMocker = new BidderMocker(robot);

            LoginResponseProcessor loginResponseProcessor = new LoginResponseProcessor();
            CommandMessageProcessor commandExecutor = new CommandMessageProcessor(biderMocker);

            MessageDispatcher.dispatcher.register(loginResponseProcessor);
            MessageDispatcher.dispatcher.register(commandExecutor);

            // robot.SetPath("D:\\work\\bid\\大漠插件\\雷达字库");
            robot.SetDict(0, "dict2003-02.txt");

            logger.InfoFormat("launch bid client {0}", DateTime.Now);

            //socketClient = new SocketClient("192.168.31.182", 9966);
            //socketClient = new SocketClient("119.3.64.205", 9966);
            //socketClient.StartClient();

            // try to login
            //string mcode = robot.GetMachineCode();
            //string ip = KK.GetLocalIPAddress();
            //string data = "" + mcode + "||" + ip + "";
            
            //RawMessage raw = RawMessages.from(10010, 333333, data);
            //socketClient.Send(raw);

            this.Topmost = true;

        }

        private void saveSetting(object sender, RoutedEventArgs e)
        {

        }

        //private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //}

        //private void textBox_TextChanged_1(object sender, TextChangedEventArgs e)
        //{

        //}

        //private void button_Click(object sender, RoutedEventArgs e)
        //{
        //    var msg = this.x1 + "," + this.y1;
        //    int ret = robot.MoveTo(x1, y1);
        //    // MessageBox.Show(msg + ", " + ret);
        //    logger.InfoFormat("msg is {0}", ret);
        //}

        //private void button1_Click(object sender, RoutedEventArgs e)
        //{
        //    var msg = this.x1 + "," + this.y1;
        //    var tarText = this.targetText.Text;
        //    msg += ". " + tarText;
        //    int ret = robot.MoveTo(x1, y1);
        //    ret = robot.LeftClick();
        //    ret = robot.KeyPressString(tarText);

        //    //MessageBox.Show(msg + ", " + ret);
        //    logger.InfoFormat("msg is {0}", ret);
        //}

        //private void button2_Click(object sender, RoutedEventArgs e)
        //{
        //    int x2 = x1 + w1;
        //    int y2 = y1 + h1;

        //    int ret = robot.Capture(x1, y1, x2, y2, "test2.bmp");

        //    //MessageBox.Show(" capture jpg result is " + ret + ", " + x1 + ", " + y1 + ", " + x2 + ", " + y2);

        //    logger.InfoFormat(" capture jpg result is " + ret + ", " + x1 + ", " + y1 + ", " + x2 + ", " + y2);

        //}

        //private void h1_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //}

        //private void w1_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //}

        //private void button3_Click(object sender, RoutedEventArgs e)
        //{
        //    string msg = robot.GetMachineCode();
        //    logger.InfoFormat(" code is " + msg);
        //}

        //private void ReopenBidingPage(object sender, RoutedEventArgs e)
        //{
        //    biderMocker.ReopenNewBidWindow();
        //}


        //private void StartLoginAndPhase(object sender, RoutedEventArgs e)
        //{
        //    biderMocker.MockLoginAndPhase1();
        //}

        //private void StartPhase2(object sender, RoutedEventArgs e)
        //{
        //    biderMocker.MockPhase2();
        //}

        //private void button6_Click(object sender, RoutedEventArgs e)
        //{
        //    string ret = robot.OcrInFile(0, 0, 2000, 2000, "test2.bmp", "2e6e9e-2e6e9e", 0.8);

        //    logger.InfoFormat(" ocr ret is " + ret);
        //}

        //private void button7_Click(object sender, RoutedEventArgs e)
        //{
        //    robot.SetPath("D:\\work\\bid\\大漠插件\\雷达字库");
        //    robot.SetDict(0, "dict1.txt");
        //}

        //private void mockLogin_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void textBox_TextChanged_3(object sender, TextChangedEventArgs e)
        //{

        //}

        //private void button_Click_1(object sender, RoutedEventArgs e)
        //{

        //}




        //private void button_Click_SendMessage(object sender, RoutedEventArgs e)
        //{

        //    Func<RawMessage, String> func = x =>
        //    {
        //        this.serverMessage.Dispatcher.Invoke(new Action(
        //            delegate
        //            {
        //                this.serverMessage.Text = x.getOccurMills() + " - " + x.getBodyText();
        //            }
        //            ));

        //        return "";
        //    };

        //    string mm = this.clientMessage.Text;
        //    socketClient.Send(mm);
        //    socketClient.setAnotherRecvCallback(func);

        //}
    }
}
