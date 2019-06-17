using Radar.Bidding.Model;
using Radar.Common.Enums;
using Radar.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Bidding.Command
{
    public interface ICommand<T>
    {
        /// <summary>
        /// 如果消息中的指令为改 Directive, 则使用改命令处理对应消息
        /// </summary>
        /// <returns></returns>
        CommandDirective GetDirective();

        /// <summary>
        /// 执行命令，如果返回值不为空且返回指令不为NONE, 则返回消息给服务端
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        JsonCommand Execute(JsonCommand req);

    }
}
