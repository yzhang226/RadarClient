using Autofac;
using log4net;
using RadarBidClient.bidding;
using RadarBidClient.bidding.socket;
using RadarBidClient.common;
using RadarBidClient.ioc;
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

        private WindowSimulator robot;

        private BidActionManager actionManager;

        private BiddingScreen biddingScreen;

        private ProjectConfig conf;


        public MainWindow()
        {
            // Automatically resize height and width relative to content
            this.SizeToContent = SizeToContent.WidthAndHeight;
            Application.Current.MainWindow.WindowState = WindowState.Maximized;

            InitializeComponent();

            robot = IoC.me.Get<WindowSimulator>();
            // TODO: 开启异步会带来很多不一致，coding时必须实时注意 异步
            robot.SetEnableAsync(false);

            actionManager = IoC.me.Get<BidActionManager>();
            biddingScreen = IoC.me.Get<BiddingScreen>();
            conf = IoC.me.Get<ProjectConfig>();

            // 为了禁用js错误提示
            this.webBro.Navigated += new NavigatedEventHandler(wbMain_Navigated);
            this.webBro.Navigate(new Uri(conf.BidLoginUrl));

            biddingScreen.SetWebBrowser(this.webBro);
            biddingScreen.SetShowUpBlock(this.RecoBlock);

            robot.SetDict(0, "resource/dict/dictwin10-001.txt");

            // 
            robot.SetDict((int) DictIndex.INDEX_CURRENT_TIME, "resource/dict/win10/dict-win10-1.txt");
            robot.SetDict((int) DictIndex.INDEX_PRICE_SECTION, "resource/dict/win10/dict-win10-2.txt");
            robot.SetDict((int) DictIndex.INDEX_NUMBER, "resource/dict/win10/dict-win10-3.txt");

            // TODO: load biddig-setting 

            logger.InfoFormat("launch bid client {0}", DateTime.Now);


            // this.Topmost = true;
        }


        public void ReopenBiddingPage(object sender, RoutedEventArgs e)
        {
            this.webBro.Navigate(new Uri(conf.BidLoginUrl));
        }

        public void saveSetting(object sender, RoutedEventArgs e)
        {
            // 格式: 秒数,加价,延迟提交毫秒数,
            //this.settings = buildBiddingSetting();
            //this.initSubmitPrice();
        }

        public void JustTest(object sender, RoutedEventArgs e)
        {

            PagePrice pp = new PagePrice();
            pp.basePrice = 8900;
            // KK.Sleep(100);
            biddingScreen.PreviewPhase2Captcha(pp);
        }

        public void AutoLoginPhase1(object sender, RoutedEventArgs e)
        {
            actionManager.MockLogin();

            
        }

        public void StartAutoBidding(object sender, RoutedEventArgs e)
        {
            biddingScreen.ResetAndRestart();
        }


        //public void writeStrategyToFile()
        //{
        //    string lines = "";
        //    foreach (SubmitPriceSetting sps in this.settings)
        //    {
        //        lines += sps.toLine() + "\n";
        //    }

        //    FileUtils.writeTxtFile(submitStrategyPath, lines);
        //}

        //public void reSetStrategyMap()
        //{
        //    foreach (SubmitPriceSetting sps in this.settings)
        //    {
        //        strategyMap[sps.second] = sps;
        //    }
        //}

        //private List<SubmitPriceSetting> buildBiddingSetting()
        //{
        //    SubmitPriceSetting setting1 = new SubmitPriceSetting();
        //    setting1.second = int.Parse(this.timing021.Text);
        //    setting1.deltaPrice = int.Parse(this.deltaPrice021.Text);
        //    setting1.delayMills = int.Parse(this.delayMills021.Text);

        //    SubmitPriceSetting setting2 = new SubmitPriceSetting();
        //    setting2.second = int.Parse(this.timing022.Text);
        //    setting2.deltaPrice = int.Parse(this.deltaPrice022.Text);
        //    setting2.delayMills = int.Parse(this.delayMills022.Text);

        //    return new List<SubmitPriceSetting>() { setting1, setting2 };
        //}

        //private void preSetPriceSettingTextBlock()
        //{
        //    for (int idx = 0; idx < this.settings.Count; idx++)
        //    {
        //        SubmitPriceSetting sps = this.settings[idx];
        //        if (idx == 0)
        //        {
        //            this.timing021.Text = sps.second.ToString();
        //            this.deltaPrice021.Text = sps.deltaPrice.ToString();
        //            this.delayMills021.Text = sps.delayMills.ToString();
        //        }
        //        else if (idx == 1)
        //        {
        //            this.timing022.Text = sps.second.ToString();
        //            this.deltaPrice022.Text = sps.deltaPrice.ToString();
        //            this.delayMills022.Text = sps.delayMills.ToString();
        //        }
        //    }

        //}



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

    }


    [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleServiceProvider
    {
        [PreserveSig]
        int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
    }

}
