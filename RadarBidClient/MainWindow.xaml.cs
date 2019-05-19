using Autofac;
using Butter.Update;
using log4net;
using Microsoft.Win32;
using Radar.bidding;
using Radar.bidding.socket;
using Radar.Common;
using Radar.ioc;
using Radar.model;
using Radar.utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Radar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(MainWindow));

        public static Updater updater;

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

            InitBizComponent();

            logger.InfoFormat("Launch Radar Client MainWindow at {0}", DateTime.Now);

            if (conf.EnableAutoUpdate)
            {
                EnableAutoUpdate();
            }

            // this.Topmost = true;
        }

        private static void EnableAutoUpdate()
        {
            updater = new Updater();
            updater.StartMonitoring();

            //DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            //timer.Tick += delegate
            //{
            //    updater.SafeCheck(null);
            //};
            //timer.Start();
            logger.InfoFormat("Start Updater-Timer.");
        }

        private void InitBizComponent()
        {

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


            string osName = KK.GetFitOSName();

            logger.InfoFormat("osName is {0}.", osName);

            string fullDictPath = "resource/dict/dict-" +  osName + ".txt";

            robot.SetDict(0, fullDictPath);
            foreach (int dictIdx in Enum.GetValues(typeof(DictIndex)))
            {
                robot.SetDict(dictIdx, "resource/dict/" + osName + "/dict-" + osName + "-" + dictIdx + ".txt");
            }

        }


        //private void UseLatestIE()
        //{
        //    int ieValue = 0;
            
        //    switch (webBro.Version.Major)
        //    {
        //        case 11:
        //            ieValue = 11001;
        //            break;
        //        case 10:
        //            ieValue = 10001;
        //            break;
        //        case 9:
        //            ieValue = 9999;
        //            break;
        //        case 8:
        //            ieValue = 8888;
        //            break;
        //        case 7:
        //            ieValue = 7000;
        //            break;
        //    }

        //    if (ieValue != 0)
        //    {
        //        using (RegistryKey registryKey =
        //            Registry.CurrentUser.OpenSubKey(
        //                @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
        //        {
        //            registryKey?.SetValue(Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName), ieValue,
        //                RegistryValueKind.DWord);
        //        }
        //    }
        //}


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


        void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            logger.InfoFormat("Nav Handl sender#{0}, e#{1}", sender, e);

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
