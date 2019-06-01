using Radar.Bidding.Command;
using Radar.Bidding.Model;
using Radar.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public class CommandProcessorFactory
    {
        // public static readonly CommandProcessorFactory executor = new CommandProcessorFactory();

        // TODO: ����ʹ�÷������������� - û�� �������͵ķ��� ? - ����û�� List<?> ����������ʹ����Base
        private static readonly Dictionary<CommandDirective, ICommand<string>> commands = new Dictionary<CommandDirective, ICommand<string>>();

        public static void Register(ICommand<string> comm)
        {
            commands[comm.GetDirective()] = comm;
        }

        public static ICommand<string> GetProcessor(CommandDirective directive)
        {
            return commands[directive];
        }

    }
}
