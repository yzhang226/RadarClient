using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar.Common.Enums
{
    /// <summary>
    /// 控制指定 
    /// </summary>
    public enum CommandDirective
    {
        /// <summary>
        /// 表示空
        /// </summary>
        NONE = 0,

        /// <summary>
        /// 客户端注册 - 请求
        /// </summary>
        CLIENT_REGISTER = 60060,

        /// <summary>
        /// 客户端注册 - 响应
        /// </summary>
        RESP_REGISTER_LOGIN = 60061,


        /// <summary>
        /// 同步NTP服务器时间
        /// </summary>
        SYNC_SYSTEM_TIME = 90100,

        /// <summary>
        /// 截图flash屏幕
        /// </summary>
        CAPTURE_BID_SCREEN = 90201,

        /// <summary>
        /// 上传flash屏幕
        /// </summary>
        UPLOAD_BID_SCREEN = 90202,

        /// <summary>
        /// 截图且上传flash屏幕
        /// </summary>
        CAPTURE_UPLOAD_BID_SCREEN = 90203,

        /// <summary>
        /// 截图且上传全屏幕
        /// </summary>
        CAPTURE_UPLOAD_FULL_SCREEN = 90204,

        /// <summary>
        /// 上传压缩日志文件
        /// </summary>
        UPLOAD_ZIPPED_LOG_FILE = 90210,

        /// <summary>
        /// 移动鼠标
        /// </summary>
        MOVE_CURSOR = 90300,

        /// <summary>
        /// 移动鼠标 并 左键点击
        /// </summary>
        MOVE_LEFT_CLICK = 90301,

        /// <summary>
        /// 移动鼠标 并 左键点击 并 输入文本
        /// </summary>
        MOVE_CLICK_INPUT_TEXT = 90302,

        /// <summary>
        /// 指令脚本（多个指令的集合）
        /// </summary>
        DIRECTIVE_SCRIPT = 90400,

        /// <summary>
        /// 通知客户端价格
        /// </summary>
        CLIENT_PRICE_TELL = 90500,


        /// <summary>
        /// 竞拍账号登录
        /// </summary>
        BID_ACCOUNT_LOGIN = 90501,


        /// <summary>
        /// 第一阶段价格出价
        /// </summary>
        PHASE1_PRICE_OFFER = 90601,

        /// <summary>
        /// 第一阶段价格提交
        /// </summary>
        PHASE1_PRICE_SUBMIT = 90602,

        /// <summary>
        /// 设置竞拍策略
        /// </summary>
        BID_STRATEGIES_SET = 90603,



    }
}
