﻿using log4net;
using System;
using System.Runtime.InteropServices;

namespace ConfigServer
{
    
    class Program
    {
        private static ILog logger = Logger.GetLoggerInstance();
        private static bool isclosing = false;
        static SocketManager conn;

        static void Main(string[] args)
        {
            logger.Info("start");
            SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
            conn = new SocketManager("MS");
            while (!isclosing)
            {
                if (!conn.Accept())
                    break;
            }
            
        }
         
        #region unmanaged
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {

            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    isclosing = true;
                    Environment.Exit(0);
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    isclosing = true;
                    logger.Info("CTRL+BREAK received!");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    isclosing = true;
                    logger.Info("Program being closed!");
                    Environment.Exit(0);
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    isclosing = true;
                    logger.Info("User is logging off!");
                    break;
            }
            return true;
        }
    }
}
