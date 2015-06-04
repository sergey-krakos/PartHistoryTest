using System;
using System.Diagnostics;
using System.Web;

namespace WebServices
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(HttpContext.Current.Server.MapPath("~/App_Data/log.txt")));
            Trace.AutoFlush = true;
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception exp = Server.GetLastError();
            Trace.WriteLine(exp);
        }
   }
}