using log4net;
using RadarBidClient.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadarBidClient.command
{

    // 

    public class ExecuteResult<T>
    {
        public int status;

        public string message;

        public T data;
    }

    public class LocalCommandExecutor
    {
        public static readonly LocalCommandExecutor executor = new LocalCommandExecutor();

        // TODO: 这里使用泛型遇到了问题 - 没有 任意类型的泛型 ? - 例如没有 List<?> 。所以这里使用了Base
        private static readonly Dictionary<string, BaseLocalCommand> commands = new Dictionary<string, BaseLocalCommand>();

        public void register(BaseLocalCommand command)
        {
            commands[command.commandName()] = command;
        }

        public LocalCommand<object> get(string commandName)
        {
            return commands[commandName];
        }

    }

    public interface LocalCommand<T>
    {

        string commandName();

        ExecuteResult<T> execute(String[] args);

    }

    public abstract class BaseLocalCommand : LocalCommand<object>
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(BaseLocalCommand));

        public BaseLocalCommand()
        {
            logger.InfoFormat("create local command process#{0}", this);

            LocalCommandExecutor.executor.register(this);
        }

        public abstract string commandName();
        public abstract ExecuteResult<object> execute(string[] args);
    }

    public class OpenIEBrowserCommand : BaseLocalCommand
    {
        public override string commandName()
        {
            return "reopenBiddingIEBrowser";
        }

        public override ExecuteResult<object> execute(string[] args)
        {
            string url = args[0];
            BidderMocker.mocker.ReopenNewBidWindow();

            return new ExecuteResult<object>();
        }
    }


    /**
     * 初始化
1.1 打开IE浏览器 - openIEBrowser(url, x, y)

1.2 设置基准坐标（可选） - setBaseCoordinate(x, y)
    */


}
