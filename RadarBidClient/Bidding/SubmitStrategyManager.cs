using log4net;
using Radar.Bidding.Model;
using Radar.Common;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Radar.Bidding
{
    [Component]
    public class SubmitStrategyManager : InitializingBean
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SubmitStrategyManager));

        private static readonly string StrategyFileName = "submitStrategy.txt";
        private static readonly string StrategyPath = "resource\\" + StrategyFileName;

        // private BiddingScreen screen;

        private FileSystemWatcher watcher = null;
        // BiddingScreen screen

        public SubmitStrategyManager()
        {
            // this.screen = screen;
        }

        public void AfterPropertiesSet()
        {
            this.WatchStragetyFile();
        }

        public List<SubmitPriceSetting> LoadStrategies()
        {
            string lines = FileUtils.ReadTxtFile(StrategyPath);

            List<SubmitPriceSetting> settings = new List<SubmitPriceSetting>();
            string[] lis = lines.Split('\n');
            foreach (string li in lis)
            {
                logger.InfoFormat("load submit setting {0}", li);

                if (li == null || li.Trim().StartsWith("#"))
                {
                    continue;
                }

                var sps = SubmitPriceSetting.fromLine(li.Trim());
                if (sps != null)
                {
                    settings.Add(sps);
                }
            }

            return settings;
        }

        private void WatchStragetyFile()
        {
            logger.InfoFormat("start watch directory#{0}, strategy-file#{1}", KK.ResourceDir(), StrategyFileName);

            if (watcher != null)
            {
                watcher.Dispose();
                logger.InfoFormat("Dispose previous watcher#{0}", watcher);
            }

            watcher = new FileSystemWatcher();
            watcher.Path = KK.ResourceDir();
            watcher.Filter = StrategyFileName;
            watcher.Changed += new FileSystemEventHandler(OnProcess);
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            watcher.IncludeSubdirectories = false;
        }

        private void OnProcess(object source, FileSystemEventArgs e)
        {
            logger.InfoFormat("File#{0} with change#{1}, {2}.", e.FullPath, e.ChangeType, e.Name);

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // screen.ResetStrategyByReload();
            }
        }

    }
}