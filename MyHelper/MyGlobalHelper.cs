using System;
using System.Security.Principal;
using FreeHttp.FreeHttpControl;
using FreeHttp.WebService.HttpServer;

namespace FreeHttp.MyHelper
{
    public class MyGlobalHelper
    {
        public delegate void GetGlobalMessageEventHandler(object sender, GlobalMessageEventArgs yourMessage);

        /// <summary>
        ///     it will called by other thread , you must keep the thread save
        /// </summary>
        public static GetGlobalMessageEventHandler OnGetGlobalMessage;

        public static MarkControlService markControlService;
        public static MyHttpListener myHttpListener;

        static MyGlobalHelper()
        {
            markControlService = new MarkControlService(1000);
            myHttpListener = new MyHttpListener();
        }

        public static void PutGlobalMessage(object sender, GlobalMessageEventArgs yourMessage)
        {
            if (OnGetGlobalMessage != null && yourMessage != null) OnGetGlobalMessage(sender, yourMessage);
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public class GlobalMessageEventArgs : EventArgs
        {
            public GlobalMessageEventArgs(bool isErrorMessage, string message)
            {
                IsErrorMessage = isErrorMessage;
                Message = message;
            }

            public bool IsErrorMessage { get; set; }
            public string Message { get; set; }
        }
    }
}