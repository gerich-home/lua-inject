using System;
using System.Windows.Forms;

namespace LuaInject
{
    public class Client : LuaInjectAgent.Client
    {
        public override void Ping()
        {
        }

        public override void Echo(String text)
        {
            MessageBox.Show(text);
        }

        public override void ReportException(Exception e)
        {
            MessageBox.Show(e.Message, "Exception thrown");
            MessageBox.Show(e.StackTrace, "Stack trace");
        }

        public override void IsInstalled(int clientPID)
        {
        }
    }
}
