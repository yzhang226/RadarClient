using Radar.Bidding.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public class CommandProcessorFactory
    {
        // public static readonly CommandProcessorFactory executor = new CommandProcessorFactory();

        // TODO: 这里使用泛型遇到了问题 - 没有 任意类型的泛型 ? - 例如没有 List<?> 。所以这里使用了Base
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
