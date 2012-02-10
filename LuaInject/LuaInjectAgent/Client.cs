using System;

namespace LuaInjectAgent
{
    /// <summary>
    /// Interface between agent and client
    /// </summary>
    public abstract class Client : MarshalByRefObject
    {
        public abstract void Ping();
        public abstract void ReportException(Exception e);
        public abstract void IsInstalled(int clientPID);
        public abstract void Echo(String text);
    }
}
