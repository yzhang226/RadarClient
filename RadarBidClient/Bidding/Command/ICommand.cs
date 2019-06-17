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
        /// �����Ϣ�е�ָ��Ϊ�� Directive, ��ʹ�ø�������Ӧ��Ϣ
        /// </summary>
        /// <returns></returns>
        CommandDirective GetDirective();

        /// <summary>
        /// ִ������������ֵ��Ϊ���ҷ���ָ�ΪNONE, �򷵻���Ϣ�������
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        JsonCommand Execute(JsonCommand req);

    }
}
