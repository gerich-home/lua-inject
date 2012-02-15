using System;
using System.Runtime.Remoting;
using System.Windows.Forms;
using EasyHook;
using System.IO;

namespace LuaInject
{
    public class Sheduler
    {
        public void Start(string targetExecutable, string commandLine = "")
        {
            try
            {
                // All loaded modules should use .Net Framework <= 3.5
                Config.Register(
                    "Lua injection",
                    
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

                    channelName,                                                    //channel
                    Path.Combine(Directory.GetCurrentDirectory(), "HookPlugins"),   //plugins directory
                    Path.Combine(Directory.GetCurrentDirectory(), "Config"),        //config directory
                    targetExecutable                                                //target executable
                    );
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace, "Failed", MessageBoxButtons.OK);
            }
        }
    }
}
