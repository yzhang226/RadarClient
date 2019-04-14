using log4net;
using RadarBidClient.model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

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

        private BiddingSetting setting;

        private Dictionary<int, BiddingStrategy> strategyMap = new Dictionary<int, BiddingStrategy>();

        public MainWindow()
        {
            InitializeComponent();

            robot = new SmartRobot();
            biderMocker = new BidderMocker(robot);

            LoginResponseProcessor loginResponseProcessor = new LoginResponseProcessor();
            CommandMessageProcessor commandExecutor = new CommandMessageProcessor(biderMocker);

            MessageDispatcher.dispatcher.register(loginResponseProcessor);
            MessageDispatcher.dispatcher.register(commandExecutor);

            //robot.SetDict(0, "dict2003-02.txt");
            robot.SetDict(0, "dictwin7-01.txt");

            // TODO: load biddig-setting 

            logger.InfoFormat("launch bid client {0}", DateTime.Now);

            // 需要一个线程 收集页面信息 - 价格时间, 从11:29:00开始
            Thread collectorThread = new Thread(loopDetectPriceAndTimeInScreen);
            collectorThread.Start();

            //  


            this.Topmost = true;
        }

        // private bool 
        

        private void loopDetectPriceAndTimeInScreen()
        {
            logger.InfoFormat("begin loopDetectPriceAndTimeInScreen");
            while (true)
            {
                try
                {
                    PagePrice pp = biderMocker.detectPriceAndTimeInScreen();
                    if (pp != null)
                    {
                        afterDetect(pp);
                    }

                    KK.Sleep(100);
                }
                catch (Exception e)
                {
                    logger.Error("detect price and time error", e);
                }
            }

            logger.InfoFormat("end loopDetectPriceAndTimeInScreen ");
        }

        private void afterDetect(PagePrice pp)
        {
            int sec = pp.occur.Second;
            int minute = pp.occur.Minute;
            foreach (var item in strategyMap)

            {
                int fixSec = item.Key;
                
                BiddingStrategy stra = item.Value;
                int fixMinute = stra.minute > 0 ? stra.minute : 29;

                if (fixMinute != minute)
                {
                    continue;
                }

                if (sec == fixSec)
                {
                    stra.basePrice = pp.basePrice;
                    logger.InfoFormat("set detected base-price {0}, {1}", sec, pp.basePrice);
                }

                if (sec >= fixSec && !stra.done)
                {
                    int targetPrice = pp.basePrice + stra.deltaPrice;
                    if (pp.high >= targetPrice)
                    {
                        logger.InfoFormat("find target price {0}, {1}", sec, stra.deltaPrice, targetPrice);
                        biderMocker.MockPhase022(targetPrice);
                        stra.done = true;
                    }
                }

            }

        }

        private void detectPhase022()
        {
            logger.InfoFormat("start detectPhase022");
            biderMocker.awaitPrice(this.setting.deltaPrice022, 29, this.setting.timing022);
            logger.InfoFormat("end detectPhase022");
        }

        public void ReopenBiddingPage(object sender, RoutedEventArgs e)
        {

        }

        public void saveSetting(object sender, RoutedEventArgs e)
        {
            // TODO: 
            this.setting = buildBiddingSetting();
            BiddingStrategy s1 = new BiddingStrategy();
            s1.second = setting.timing021;
            s1.deltaPrice = setting.deltaPrice021;
            s1.delayMills = setting.delayMills021;

            strategyMap[setting.timing021] = s1;

            BiddingStrategy s2 = new BiddingStrategy();
            s2.second = setting.timing022;
            s2.deltaPrice = setting.deltaPrice022;
            s2.delayMills = setting.delayMills022;

            strategyMap[setting.timing021] = s2;
        }

        private BiddingSetting buildBiddingSetting()
        {
            BiddingSetting setting = new BiddingSetting();
            setting.timing021 = int.Parse(this.timing021.Text);
            setting.deltaPrice021 = int.Parse(this.deltaPrice021.Text);
            setting.delayMills021 = int.Parse(this.delayMills021.Text);

            setting.timing022 = int.Parse(this.timing022.Text);
            setting.deltaPrice022 = int.Parse(this.deltaPrice022.Text);
            setting.delayMills022 = int.Parse(this.delayMills022.Text);

            return setting;
        }
        
    }
    
    public class BiddingSetting
    {

        public int timing021 { get; set; }

        public int deltaPrice021 { get; set; }

        public int delayMills021 { get; set; }


        public int timing022 { get; set; }

        public int deltaPrice022 { get; set; }

        public int delayMills022 { get; set; }

    }

}
