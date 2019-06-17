using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Radar.Common.Logging
{
    public class AsyncRollingFileAppender : RollingFileAppender, IDisposable
    {

        private readonly BlockingCollection<LoggingEvent> _logEvents;
        private bool onClosing;

        public void Dispose()
        {
            
        }

        public AsyncRollingFileAppender()
        {
            _logEvents = new BlockingCollection<LoggingEvent>();
            Start();
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            foreach (LoggingEvent loggingEvent in loggingEvents)
                Append(loggingEvent);
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (FilterEvent(loggingEvent))
            {
                _logEvents.Add(loggingEvent);
            }
        }

        private void Start()
        {
            if (!onClosing)
            {
                Thread thr = new Thread(LogMessages) { IsBackground = true };
                thr.Start();
            }
        }

        private void LogMessages()
        {
            while (!onClosing)
            {
                LoggingEvent loggingEvent = _logEvents.Take();
                if (loggingEvent != null)
                {
                    base.Append(loggingEvent);
                }
            }
        }

        protected override void OnClose()
        {
            onClosing = true;
            base.OnClose();
        }


    }
}
