using log4net;
using System;

namespace ConfigServer
{
    class Logger
    {
        private static ILog logger = null;

        public static ILog GetLoggerInstance()
        {
            if (logger == null)
            {
#if DEBUG
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("../../log/DEBUG.xml"));
                logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("../../log/RELEASE.xml"));
                logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
            }

            return logger;
        }
    }
}
