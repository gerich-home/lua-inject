namespace LuaInjectAgent
{
    public interface IHooker
    {
        void Hook(string targetExecutable, string configPath, string pluginsPath, Client Interface);
    }
}
