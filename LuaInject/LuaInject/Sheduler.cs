using System;
using System.Runtime.Remoting;
using System.Windows.Forms;
using EasyHook;
using System.IO;

namespace LuaInject
{
    public class Sheduler
    {
        public void Start(string targetExecutable, string hookedModule = null, string commandLine = "")
        {
            hookedModule = hookedModule ?? targetExecutable;

            try
            {
                // All loaded modules should use .Net Framework <= 3.5
                Config.Register("Lua injection",
                    
                    "LuaInjectAgent.dll",
                    "System.ComponentModel.Composition.dll"
                    );

                string channelName = null;
                RemoteHooking.IpcCreateServer<Client>(ref channelName, WellKnownObjectMode.SingleCall);

                int targetPid;

                RemoteHooking.CreateAndInject(targetExecutable, commandLine, 0,
                    "LuaInjectAgent.dll",
                    "LuaInjectAgent.dll",
                    out targetPid,
                    
                    channelName,
                    Path.Combine(Directory.GetCurrentDirectory(), "HookPlugins"),
                    hookedModule
                    );
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace, "Failed", MessageBoxButtons.OK);
            }
        }
    }
}
