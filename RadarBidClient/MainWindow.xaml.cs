using log4net;
using Radar.Bidding;
using Radar.Bidding.Model;
using Radar.Bidding.Net;
using Radar.Bidding.Service;
using Radar.Butter;
using Radar.Common;
using Radar.Common.Threads;
using Radar.Common.Times;
using Radar.Common.Utils;
using Radar.IoC;
using Radar.Model;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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

        private SocketClient socketClient;

        private ClientService clientService;

        private ProjectConfig conf;

        private LoginActManager loginActManager;


        public MainWindow()
        {
            // Automatically resize height and width relative to content
            this.SizeToContent = SizeToContent.WidthAndHeight;
            Application.Current.MainWindow.WindowState = WindowState.Maximized;

            InitializeComponent();

            InitCanvasComponent();

            InitBizDir();

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

            logger.InfoFormat("Start UpdateTimer.");
        }

        private void InitCanvasComponent()
        {
            VerLabel.Content = Ver.ver;
            SeatNoLabel.Content = KK.ReadClientSeatNo();
        }

        private void InitBizComponent()
        {

            robot = ApplicationContext.me.Get<WindowSimulator>();
            // TODO: 开启异步会带来很多不一致，coding时必须实时注意 异步
            robot.SetEnableAsync(false);

            actionManager = ApplicationContext.me.Get<BidActionManager>();
            biddingScreen = ApplicationContext.me.Get<BiddingScreen>();
            conf = ApplicationContext.me.Get<ProjectConfig>();
            loginActManager = ApplicationContext.me.Get<LoginActManager>();

            // 为了禁用js错误提示
            this.webBro.LoadCompleted += new LoadCompletedEventHandler((sender, e) =>
            {
                loginActManager.BeforeLogin();

                logger.InfoFormat("webBro.LoadCompleted - EnableAutoInputAccount is {0}.", conf.EnableAutoInputAccount);
                
                // TODO: 这里每次都会在每一个页面载入的时候, 都会执行这一段逻辑。所以不能开启这段逻辑
                if (conf.EnableAutoInputAccount)
                {
                    InputLoginAccount();
                }

            });
            this.webBro.Navigated += new NavigatedEventHandler(wbMain_Navigated);
            this.webBro.Navigate(new Uri(conf.BidLoginUrl));

            biddingScreen.SetWebBrowser(this.webBro);
            biddingScreen.SetShowUpBlock(this.RecoBlock);

            if (conf.EnableSaberRobot)
            {
                socketClient = ApplicationContext.me.Get<SocketClient>();
                clientService = ApplicationContext.me.Get<ClientService>();

                socketClient.AfterSuccessConnected = (aa) =>
                {
                    try
                    {
                        clientService.DoClientRegister();
                    }
                    catch (Exception e)
                    {
                        logger.Error("DoClientLogin error", e);
                    }
                    return aa;
                };
                socketClient.EnableSocketGuard = true;

                socketClient.StartClient();
                
            }

            var captchaTaskDaemon = ApplicationContext.me.Get<CaptchaTaskDaemon>();
            captchaTaskDaemon.RestartInquiryThread();

            string osName = KK.GetFitOSName();
            logger.InfoFormat("osName is {0}.", osName);

            foreach (int dictIdx in Enum.GetValues(typeof(DictIndex)))
            {
                if (dictIdx == 0) {
                    robot.SetDict(0, string.Format("Resource/dict/dict-{0}.txt", osName));
                    continue;
                }

                robot.SetDict(dictIdx, string.Format("Resource/dict/{0}/dict-{0}-{1}.txt", osName, dictIdx));
            }

            if (conf.EnableCorrectNetTime)
            {
                ThreadUtils.StartNewTaskSafe(() => {
                    var bo = TimeSynchronizer.SyncFromNtpServer();
                    logger.InfoFormat("TimeSynchronizer.SyncFromNtpServer result is {0}.", bo);
                });
            }

        }

        private void InitBizDir()
        {
            FileUtils.CreateDir(KK.CapturesDir());
            FileUtils.CreateDir(KK.FlashScreenDir());
        }

        private void InputLoginAccount()
        {
            BidAccountInfo acc = KK.LoadResourceAccount();
            if (acc == null)
            {
                return;
            }

            loginActManager.LoginBidAccount(acc.BidNo, acc.Password, acc.IdCardNo, conf.LoginAccountAfterAutoInput);
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

            // PagePrice pp = PagePrice();
            // pp.basePrice = 8900;
            // KK.Sleep(100);
            // biddingScreen.PreviewPhase2Captcha(pp);

            clientService.DoClientRegister();

        }

        public void LoginAccountFromResource(object sender, RoutedEventArgs e)
        {
            InputLoginAccount();
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
