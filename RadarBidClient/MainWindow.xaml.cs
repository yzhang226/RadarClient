using log4net;
using RadarBidClient.model;
using RadarBidClient.utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace RadarBidClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(MainWindow));

        private SmartRobot robot;

        private BidderMocker biderMocker;

        private List<SubmitPriceSetting> settings = new List<SubmitPriceSetting>();

        private Dictionary<int, SubmitPriceSetting> strategyMap = new Dictionary<int, SubmitPriceSetting>();

        private string submitStrategyPath = "submitStrategy.txt";

        private Dictionary<int, bool> doneStatus = new Dictionary<int, bool>();

        public MainWindow()
        {
            // Automatically resize height and width relative to content
            this.SizeToContent = SizeToContent.WidthAndHeight;
            Application.Current.MainWindow.WindowState = WindowState.Maximized;

            
            InitializeComponent();



            // http://127.0.0.1:3456/login.htm http://127.0.0.1:3456/bid.htm

            this.webBro.Navigated += new NavigatedEventHandler(wbMain_Navigated);

            this.webBro.Navigate(new Uri("http://127.0.0.1:3456/bid.htm"));


            robot = new SmartRobot();
            biderMocker = new BidderMocker(robot);

            LoginResponseProcessor loginResponseProcessor = new LoginResponseProcessor();
            CommandMessageProcessor commandExecutor = new CommandMessageProcessor(biderMocker);

            MessageDispatcher.dispatcher.register(loginResponseProcessor);
            MessageDispatcher.dispatcher.register(commandExecutor);

            //robot.SetDict(0, "dict2003-02.txt");
            // robot.SetDict(0, "dictwin7-01.txt");
            //robot.SetDict(0, "win10-01.txt");
            robot.SetDict(0, "dictwin10-001.txt");

            // TODO: load biddig-setting 

            logger.InfoFormat("launch bid client {0}", DateTime.Now);

            // 需要一个线程 收集页面信息 - 价格时间, 从11:29:00开始
            Thread collectorThread = new Thread(loopDetectPriceAndTimeInScreen);
            collectorThread.IsBackground = true;
            collectorThread.Start();

            //  
            string lines = FileUtils.readTxtFile(submitStrategyPath);
            string[] lis = lines.Split('\n');
            foreach (string li in lis)
            {
                logger.InfoFormat("load price setting {0}", li);
                var sps = SubmitPriceSetting.fromLine(li);
                if (sps != null)
                {
                    this.settings.Add(sps);
                }
            }

            this.preSetPriceSettingTextBlock();
            this.initSubmitPrice();

            // this.Topmost = true;
        }

        private void webBrowser1_NewWindow2(ref object ppDisp, ref bool Cancel)
        {
            // Handle the event.  
            logger.InfoFormat("webBrowser1_NewWindow2 is {0}, {1}.", ppDisp, Cancel);
        }

        void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(this.webBro, true); // make it silent
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        private void initSubmitPrice()
        {
            this.reSetStrategyMap();
            this.initDoneStatus();
        }

        private void loopDetectPriceAndTimeInScreen()
        {
            logger.InfoFormat("begin loopDetectPriceAndTimeInScreen");
            int i = 0;
            
            while (true)
            {
                long ss = KK.currentTs();

                try
                {
                    long s1 = KK.currentTs();
                    PagePrice pp = biderMocker.detectPriceAndTimeInScreen();
                    logger.DebugFormat("detectPriceAndTimeInScreen elapsed {0}ms", KK.currentTs() - s1);

                    if (pp != null)
                    {
                        s1 = KK.currentTs();
                        afterDetect(pp);
                        logger.DebugFormat("afterDetect elapsed {0}ms", KK.currentTs() - s1);

                        var dt = pp.occur;
                        if ( (dt.Minute == 28 && dt.Second == 59) 
                            || (dt.Minute == 29 && dt.Second == 59)
                            || (dt.Minute == 28 && dt.Second == 0)
                            || (dt.Minute == 29 && dt.Second == 0)
                            )
                        {
                            this.initDoneStatus();
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Error("detect price and time error", e);
                }
                finally
                {
                    KK.Sleep(100);
                }
                
                logger.DebugFormat("round {0} loopDetectPriceAndTimeInScreen elapsed {1}ms", i++, KK.currentTs() - ss);
            }

            logger.InfoFormat("end loopDetectPriceAndTimeInScreen ");
        }

        private void initDoneStatus()
        {
            foreach (SubmitPriceSetting sps in this.settings)
            {
                doneStatus[sps.second] = false;
            }
        }

        private void afterDetect(PagePrice pp)
        {
            long s1 = KK.currentTs();
            int sec = pp.occur.Second;
            int minute = pp.occur.Minute;
            foreach (var item in strategyMap)
            {
                int fixSec = item.Key;
                
                SubmitPriceSetting stra = item.Value;
                int fixMinute = stra.minute > 0 ? stra.minute : 29;

                //if (fixMinute != minute)
                //{
                //    continue;
                //}

                if (sec == fixSec)
                {
                    stra.basePrice = pp.basePrice;
                    logger.InfoFormat("set detected base-price {0} {1}, {2}", fixMinute, sec, pp.basePrice);
                }

                if (sec >= fixSec && !doneStatus[fixSec])
                {
                    int targetPrice = pp.basePrice + stra.deltaPrice;
                    if (pp.high >= targetPrice)
                    {
                        logger.InfoFormat("find target price {0}, {1}", sec, stra.deltaPrice, targetPrice);
                        biderMocker.MockPhase022(targetPrice);
                        doneStatus[fixSec] = true;
                    }
                }

            }

            logger.InfoFormat("afterDetect elapsed {0}ms", KK.currentTs() - s1);

        }

        private void detectPhase022()
        {
            //logger.InfoFormat("start detectPhase022");
            //biderMocker.awaitPrice(this.setting.deltaPrice022, 29, this.setting.timing022);
            //logger.InfoFormat("end detectPhase022");
        }

        public void ReopenBiddingPage(object sender, RoutedEventArgs e)
        {

        }

        public void saveSetting(object sender, RoutedEventArgs e)
        {
            // 格式: 秒数,加价,延迟提交毫秒数,
            this.settings = buildBiddingSetting();
            this.initSubmitPrice();
        }

        public void writeStrategyToFile()
        {
            string lines = "";
            foreach (SubmitPriceSetting sps in this.settings)
            {
                lines += sps.toLine() + "\n";
            }

            FileUtils.writeTxtFile(submitStrategyPath, lines);
        }

        public void reSetStrategyMap()
        {
            foreach (SubmitPriceSetting sps in this.settings)
            {
                strategyMap[sps.second] = sps;
            }
        }

        private List<SubmitPriceSetting> buildBiddingSetting()
        {
            SubmitPriceSetting setting1 = new SubmitPriceSetting();
            setting1.second = int.Parse(this.timing021.Text);
            setting1.deltaPrice = int.Parse(this.deltaPrice021.Text);
            setting1.delayMills = int.Parse(this.delayMills021.Text);

            SubmitPriceSetting setting2 = new SubmitPriceSetting();
            setting2.second = int.Parse(this.timing022.Text);
            setting2.deltaPrice = int.Parse(this.deltaPrice022.Text);
            setting2.delayMills = int.Parse(this.delayMills022.Text);

            return new List<SubmitPriceSetting>() { setting1, setting2 };
        }

        private void preSetPriceSettingTextBlock()
        {
            for (int idx = 0; idx < this.settings.Count; idx++)
            {
                SubmitPriceSetting sps = this.settings[idx];
                if (idx == 0)
                {
                    this.timing021.Text = sps.second.ToString();
                    this.deltaPrice021.Text = sps.deltaPrice.ToString();
                    this.delayMills021.Text = sps.delayMills.ToString();
                }
                else if (idx == 1)
                {
                    this.timing022.Text = sps.second.ToString();
                    this.deltaPrice022.Text = sps.deltaPrice.ToString();
                    this.delayMills022.Text = sps.delayMills.ToString();
                }
            }
            

        }
        
    }



    


    [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleServiceProvider
    {
        [PreserveSig]
        int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
    }

}
