using System;
using System.Collections.Generic;
using System.Threading;
using EasyHook;

namespace LuaInjectAgent
{
    public class Main : IEntryPoint
    {
        //Stack<string> _queue = new Stack<string>();
        public Client Interface { get; private set; }

        public Main(RemoteHooking.IContext context, string channelName, string hookedModule)
        {
            // connect to host...
            Interface = RemoteHooking.IpcConnectClient<Client>(channelName);
            Interface.Ping();
        }

        public void Run(RemoteHooking.IContext context, string channelName, string hookedModule)
        {
            // install hook...
            Lua lua;

            try
            {
                lua = new Lua(hookedModule, Interface);
            }
            catch (Exception e)
            {
                Interface.ReportException(e);
                return;
            }

            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

            //Interface.Lua = lua;

            RemoteHooking.WakeUpProcess();

            // wait for host process termination...
            try
            {
                while (true)
                {
                    Thread.Sleep(500);

                    // transmit newly monitored file accesses...
                    /*
                    if (_queue.Count > 0)
                    {
                        String[] Package = null;

                        lock (_queue)
                        {
                            Package = _queue.ToArray();

                            _queue.Clear();
                        }
                    }
                    else
                    */
                        Interface.Ping();
                }
            }
            catch
            {
                // Ping() will raise an exception if host is unreachable
            }
        }
    }
}
