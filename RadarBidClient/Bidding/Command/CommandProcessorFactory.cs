using Radar.Bidding.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public class CommandProcessorFactory
    {
        // public static readonly CommandProcessorFactory executor = new CommandProcessorFactory();

        // TODO: ����ʹ�÷������������� - û�� �������͵ķ��� ? - ����û�� List<?> ����������ʹ����Base
        private static readonly Dictionary<ReceiveDirective, Radar.Bidding.Command.BaseCommand> commands = new Dictionary<ReceiveDirective, Radar.Bidding.Command.BaseCommand>();

        public static void Register(Radar.Bidding.Command.BaseCommand command)
        {
            commands[command.GetDirective()] = command;
        }

        public static Radar.Bidding.Command.ICommand<string> GetProcessor(ReceiveDirective commandName)
        {
            return commands[commandName];
        }

    }
}
