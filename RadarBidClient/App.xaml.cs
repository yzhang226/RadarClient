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
using Butter.Update;
using System.Windows.Threading;
using Radar.Common;

namespace Radar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(App));

        private const string Unique = "JX_RADAR_BID_Application";

        private static App application;

        private static Process thisProcess;

        private static Application thisApplication;

        [STAThread]
        public static void Main()
        {
            logger.InfoFormat("App RUNNING and version is {0}", Ver.ver);

            //if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            //{

            thisProcess = Process.GetCurrentProcess();
            thisApplication = Application.Current;

            try
            {
                application = new App();

                logger.InfoFormat("new App done.");

                application.InitializeComponent();

                logger.InfoFormat("InitializeComponent done.");

                application.Run();

                ForceCloseWindow();
            } 
            catch (Exception e)
            {
                logger.Error("run App error.", e);
            }

            logger.InfoFormat("Radar application DONE Run - {0}", application);

            // Allow single instance code to perform cleanup operations
            // SingleInstance<App>.Cleanup();
            // logger.InfoFormat("SingleInstance cleanup");

            
            //}
            //else
            //{
            //    Process current = Process.GetCurrentProcess();
            //    logger.InfoFormat("Radar application already exist - {0}", "just");
            //}
        }

        private static void ForceCloseWindow()
        {
            // 
            try
            {
                logger.InfoFormat("Force Kill current process, {0}, {1}.", thisProcess, application);
                thisProcess.Kill();

                logger.InfoFormat("Begin Application.Current Shutdown, {0}, {1}.", thisApplication, application);
                thisApplication.Shutdown();
                logger.InfoFormat("Done Application.Current Shutdown, {0}, {1}.", thisApplication, application);

                // Environment.Exit(0);
                // logger.InfoFormat("Environment.Exit(0)");

                // logger.InfoFormat("ForceCloseWindow doing nothing.");
            }
            catch (Exception)
            {

            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // base.OnExit(e);

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
