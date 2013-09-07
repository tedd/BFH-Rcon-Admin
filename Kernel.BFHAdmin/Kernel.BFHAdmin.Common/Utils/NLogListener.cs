using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Common.Utils
{
    [Target("NLogListener")]
    public class NLogListener : TargetWithLayout
    {
        public delegate void LogWriteDelegate(object sender, LogEventInfo logEvent, string message);
        public event LogWriteDelegate LogWrite;
        private bool _hasRegistered = false;
        private string _id;
        private string _targetId ;
        private LoggingRule _loggingRule;
        protected virtual void OnLogWrite(LogEventInfo logevent, string message)
        {
            LogWriteDelegate handler = LogWrite;
            if (handler != null) handler(this, logevent, message);
        }

        public NLogListener()
        {
            _id= "NLogListener_" + Guid.NewGuid().ToString();
            _targetId = _id + "_target";

            RegisterAsLogger();
            //LoggingConfiguration config = new LoggingConfiguration();
            var config = LogManager.Configuration;

            config.AddTarget(_targetId, this);
            this.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message} ${exception:format=tostring}";
            _loggingRule = new LoggingRule("*", LogLevel.Trace, this);
            config.LoggingRules.Add(_loggingRule);
            
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }

        private void RegisterAsLogger()
        {
            lock (this)
            {
                if (_hasRegistered)
                    return;
                _hasRegistered = true;
                //Register the custom target
                ConfigurationItemFactory.Default.Targets.RegisterDefinition("NLogListener", typeof(NLogListener));
                LogManager.ReconfigExistingLoggers();
            }
        }
        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);
            OnLogWrite(logEvent, logMessage);
        }


    }
}
