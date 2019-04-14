using log4net;
using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();

                application.InitializeComponent();
                application.Run();

                logger.InfoFormat("radar application run - {0}", application);

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
                logger.InfoFormat("SingleInstance cleanup");

            }
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
