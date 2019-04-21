using log4net;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RadarBidClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(App));

        private const string Unique = "JX_RADAR_BID_Application";

        private static App application;

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                application = new App();

                application.InitializeComponent();
                application.Run();

                logger.InfoFormat("radar application run - {0}", application);

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
                logger.InfoFormat("SingleInstance cleanup");

                forceCloseWindow();
            }
            else
            {
                //Process current = Process.GetCurrentProcess();
                logger.InfoFormat("radar application already exist - {0}", "just");
            }
        }

        private static void forceCloseWindow()
        {
            // 
            logger.InfoFormat("force Kill current process, {0}, {1}.", Application.Current, application);
            Process.GetCurrentProcess().Kill();

            logger.InfoFormat("begin Application.Current Shutdown, {0}, {1}.", Application.Current, application);
            Application.Current.Shutdown();
            logger.InfoFormat("done Application.Current Shutdown, {0}, {1}.", Application.Current, application);

            Environment.Exit(0);
            logger.InfoFormat("Environment.Exit(0)");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            logger.InfoFormat("OnExit code is {0}", e.ApplicationExitCode);
        }

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // …

            return true;
        }

        #endregion
    }
}
