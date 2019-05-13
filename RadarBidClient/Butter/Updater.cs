using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ionic.Zip;
using Butter.Net;
using log4net;
using RadarBidClient.bidding;
using RadarBidClient;
using System.Windows;

namespace Butter.Update
{
    public class Updater
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Updater));

        #region Constants
        /// <summary>
        /// The default check interval
        /// </summary>
        public const int DefaultCheckInterval = 900; // 900s == 15 min
        public const int FirstCheckDelay = 15;

        /// <summary>
        /// The default configuration file
        /// </summary>
        public const string DefaultConfigFileName = "Manifest.xml";
        
        public const string WorkPath = "work";
        #endregion

        #region Fields
        private Timer _timer;
        private volatile bool _updating;
        private readonly Manifest _localConfig;
        private Manifest _remoteConfig;
        private readonly FileInfo _localConfigFile;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class.
        /// </summary>
        public Updater() : this(new FileInfo(DefaultConfigFileName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        public Updater (FileInfo configFile)
        {
            
            _localConfigFile = configFile;
            logger.InfoFormat("Loaded. Initializing using file '{0}'.", configFile.FullName);
            if (!configFile.Exists)
            {
                logger.InfoFormat("Config file '{0}' does not exist, stopping.", configFile.Name);
                return;
            }

            string data = File.ReadAllText(configFile.FullName);
            this._localConfig = new Manifest(data);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the monitoring.
        /// </summary>
        public void StartMonitoring ()
        {
            logger.InfoFormat("Starting monitoring every {0}s.", this._localConfig.CheckInterval);
            _timer = new Timer(SafeCheck, null, 5000, this._localConfig.CheckInterval * 1000);
        }

        /// <summary>
        /// Stops the monitoring.
        /// </summary>
        public void StopMonitoring ()
        {
            logger.InfoFormat("Stopping monitoring.");
            if (_timer == null)
            {
                logger.InfoFormat("Monitoring was already stopped.");
                return;
            }
            _timer.Dispose();
        }

        public void SafeCheck(object state)
        {
            try
            {
                CheckState(state);
            }
            catch (Exception e)
            {
                logger.Error("Safe check error", e);
            }
        }

        /// <summary>
        /// Checks the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        private void CheckState(object state)
        {
            logger.InfoFormat("Check starting.");

            if (_updating)
            {
                logger.InfoFormat("Updater is already updating.");
                logger.InfoFormat("Check ending.");
            }
            var remoteUri = new Uri(this._localConfig.BaseUri + DefaultConfigFileName);

            logger.InfoFormat("Fetching '{0}'.", remoteUri.AbsoluteUri);
            var http = new Fetch { Retries = 5, RetrySleep = 30000, Timeout = 30000 };
            http.Load(remoteUri.AbsoluteUri);
            if (!http.Success)
            {
                logger.InfoFormat("Fetch error: {0}", http.Response.StatusDescription);
                this._remoteConfig = null;
                return;
            }

            string data = Encoding.UTF8.GetString(http.ResponseData);
            this._remoteConfig = new Manifest(data);

            if (this._remoteConfig == null)
                return;

            if (this._localConfig.SecurityToken != this._remoteConfig.SecurityToken)
            {
                logger.InfoFormat("Security token mismatch.");
                return;
            }
            logger.InfoFormat("Remote config is valid. Local version is  {0}, Remote version is {1}.", this._localConfig.Version, this._remoteConfig.Version);

            if (this._remoteConfig.Version == this._localConfig.Version)
            {
                logger.InfoFormat("Versions are the same. Check ending.");
                return;
            }
            if (this._remoteConfig.Version < this._localConfig.Version)
            {
                logger.InfoFormat("Remote version is older. That's weird. Check ending.");
                return;
            }

            logger.InfoFormat("Remote version is newer. Updating.");
            _updating = true;
            Update();
            _updating = false;
            logger.InfoFormat("Check ending.");
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private void Update ()
        {

            logger.InfoFormat("Updating '{0}' files.", this._remoteConfig.Payloads.Length);

            // Clean up failed attempts.
            if (Directory.Exists(WorkPath))
            {
                logger.InfoFormat("WARNING: Work directory already exists.");
                try { Directory.Delete(WorkPath, true); }
                catch (IOException)
                {
                    logger.InfoFormat("Cannot delete open directory '{0}'.", WorkPath);
                    return;
                }
            }

            Directory.CreateDirectory(WorkPath);
            
            // Download files in manifest.
            foreach (string updateFileName in this._remoteConfig.Payloads)
            {
                logger.InfoFormat("Fetching '{0}'.", updateFileName);
                var url = this._remoteConfig.BaseUri + updateFileName;
                var file = Fetch.Get(url);
                if (file == null)
                {
                    logger.InfoFormat("Fetch {0} failed.", updateFileName);
                    return;
                }
                var info = new FileInfo(Path.Combine(WorkPath, updateFileName));
                Directory.CreateDirectory(info.DirectoryName);
                File.WriteAllBytes(Path.Combine(WorkPath, updateFileName), file);

                // Unzip
                if ( Regex.IsMatch(updateFileName, @"\.zip"))
                {
                    try
                    {
                        var zipfile = Path.Combine(WorkPath, updateFileName);
                        using (var zip = ZipFile.Read(zipfile))
                            zip.ExtractAll(WorkPath, ExtractExistingFileAction.Throw);
                        File.Delete(zipfile);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Unpack failed:", ex);
                        return;
                    }
                }
            }

            // Change the currently running executable so it can be overwritten.
            Process thisprocess = Process.GetCurrentProcess();
            string me = thisprocess.MainModule.FileName;
            string bak = me + ".bak";
            logger.InfoFormat("Renaming running process to '{0}'.", bak);
            if(File.Exists(bak))
                File.Delete(bak);
            File.Move(me, bak);
            File.Copy(bak, me);

            // Write out the new manifest.
            _remoteConfig.Write(Path.Combine(WorkPath, _localConfigFile.Name));

            // Copy everything.
            var directory = new DirectoryInfo(WorkPath);
            var files = directory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                string destination = file.FullName.Replace(directory.FullName+@"\", "");
                logger.InfoFormat("installing file '{0}'.", destination);
                Directory.CreateDirectory(new FileInfo(destination).DirectoryName);
                file.CopyTo(destination, true);
            }

            // Clean up.
            logger.InfoFormat("Deleting work directory.");
            Directory.Delete(WorkPath, true);

            // Stop Business Thread
            try
            {
                BiddingScreen.StopCollectingWorkThread();
            }
            catch (Exception e)
            {
                logger.Error("StopCollectingWorkThread error.", e);
            }

            Thread.Sleep(2 * 1000);

            // Restart.
            logger.InfoFormat("Spawning new process with FileName#{0}.", me);
            // var spawn = Process.Start(me);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = me,
                UseShellExecute = true,
                // Arguments = arguments.ToString()
            };
            // Application.ResourceAssembly.Location
            var spawnProcess = Process.Start(processStartInfo);

            // ReStart();

            //System.Diagnostics.Process.Start(me);
            //Application.Exit(0);

            // System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            // Application.Current.Shutdown();

            logger.InfoFormat("New process ID is {0}", spawnProcess.Id);
            logger.InfoFormat("Closing old running process {0}.", thisprocess.Id);
            Application.Current.Shutdown();
            // thisprocess.CloseMainWindow();
            // thisprocess.Close();
            // thisprocess.Dispose();
        }

        private void ReStart()
        {

            Process thisprocess = Process.GetCurrentProcess();
            string me = thisprocess.MainModule.FileName;

            string sApplicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sAppName = "RadarBidClient";

            System.AppDomainSetup oSetup = new System.AppDomainSetup();
            string sApplicationFile = null;

            // Use this to ensure that if the application is running when the user performs the update, that we don't run into file locking issues.
            oSetup.ShadowCopyFiles = "true";
            oSetup.ApplicationName = sAppName;

            // Generate the name of the DLL we are going to launch
            sApplicationFile = System.IO.Path.Combine(sApplicationDirectory, sAppName + ".exe");

            oSetup.ApplicationBase = sApplicationDirectory;
            oSetup.ConfigurationFile = sApplicationFile + ".config";
            oSetup.LoaderOptimization = LoaderOptimization.MultiDomain;

            // Launch the application
            System.AppDomain oAppDomain = AppDomain.CreateDomain(sAppName, AppDomain.CurrentDomain.Evidence, oSetup);
            oAppDomain.SetData("App", sAppName);
            // oAppDomain.SetData("User", sUserName);
            // oAppDomain.SetData("Pwd", sUserPassword);

            oAppDomain.ExecuteAssembly(sApplicationFile);

            // When the launched application closes, close this application as well
            // Application.Exit();
            Application.Current.Shutdown();
        }

        #endregion
    }
}
