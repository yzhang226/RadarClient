using Radar.Bidding.Model;
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
    public class DirectiveScriptCommand : BaseCommand<string>
    {
        private BidActionManager bidActionManager;

        public DirectiveScriptCommand(BidActionManager bidActionManager)
        {
            this.bidActionManager = bidActionManager;
        }

        public override CommandDirective GetDirective()
        {
            return CommandDirective.DIRECTIVE_SCRIPT;
        }

        protected override JsonCommand DoExecute(string args)
        {
            // TODO: 这里是使用 换行符, 建立处理
            string[] arr = args.Split('\n');
            foreach(string arg in arr)
            {
                int idx1 = arg.IndexOf(',');
                string p1 = arg.Substring(0, idx1);
                string p2 = arg.Substring(idx1+1);

                CommandDirective direct = EnumHelper.ToEnum<CommandDirective>(int.Parse(p1));
                JsonCommand req = JsonCommands.OK(direct, p2);
                ICommand<string> commandProcessor = CommandProcessorFactory.GetProcessor(direct);
                JsonCommand dr = commandProcessor.Execute(req);
                // TODO: 

            }

            return null;
        }
    }

}
