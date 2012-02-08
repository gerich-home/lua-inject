using System;
using System.Runtime.Remoting;
using System.Windows.Forms;
using EasyHook;

namespace LuaInject
{
    public class Sheduler
    {
        public void Start(string ownerName, string targetExecutable, string hookedModule = null, string commandLine = "")
        {
            hookedModule = hookedModule ?? targetExecutable;

            try
            {
                // All loaded modules should use .Net Framework <= 3.5
                Config.Register("Lua injection", ownerName, "LuaInjectAgent.dll", "LuaInject.dll");

                string channelName = null;
                RemoteHooking.IpcCreateServer<Client>(ref channelName, WellKnownObjectMode.SingleCall);

                int targetPid;

                RemoteHooking.CreateAndInject(targetExecutable, commandLine, 0, "LuaInjectAgent.dll", "LuaInjectAgent.dll", out targetPid, channelName, hookedModule);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace, "Failed", MessageBoxButtons.OK);
            }
        }
    }
}
