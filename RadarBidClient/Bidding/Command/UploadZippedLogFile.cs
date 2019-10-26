using log4net;
using Radar.Bidding.Model.Dto;
using Radar.Bidding.Service;
using Radar.Common;
using Radar.Common.Enums;
using Radar.Common.Model;
using Radar.Common.Utils;
using Radar.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Radar.Bidding.Command
{

    [Component]
    public class UploadZippedLogFile : BaseCommand<string>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(UploadZippedLogFile));

        private BidActionManager bidActionManager;

        private ClientService clientService;

        public UploadZippedLogFile(BidActionManager bidActionManager, ClientService clientService)
        {
            this.bidActionManager = bidActionManager;
            this.clientService = clientService;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.UPLOAD_ZIPPED_LOG_FILE;
        }

        protected override JsonCommand DoExecute(string args)
        {

            string infoFilePath = "/data/logs/RadarClient/info.log";

            if (!FileUtils.IsFileExist(infoFilePath))
            {
                logger.ErrorFormat("log file#{0} do not exist", infoFilePath);
                return null;
            }


            int clientNo = ClientService.AssignedClientNo;
            string dt = DateTime.Now.ToString("yyyy-MM-dd");

            string zippedLogFile = "/temp/" + dt + "-" + clientNo + "-" + KK.CurrentMills() + "-info.log.gz";

            ZipUtils.Compress(zippedLogFile, infoFilePath);
            
            // upload 
            ScreenImageUploadResponse resp = bidActionManager.UploadFileToSaber(zippedLogFile, 10);

            // FileUtils.DeleteFile(zippedLogFile);
            logger.InfoFormat("delete file#{0}", zippedLogFile);

            return null;
        }
    }
    

}
