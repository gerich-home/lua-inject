using System;
using System.Collections.Generic;

namespace LuaHooker
{
    public class LuaHook
    {
        public LuaHook(string name, Delegate hookerFunction)
        {
            Name = name;
            HookerFunction = hookerFunction;
        }

        public static LuaHook Create<T>(string name, Func<T, T> hookerFunction)
        {
            return new LuaHook(name, hookerFunction);
        }

        public readonly string Name;
        public readonly Delegate HookerFunction;
    }

    public interface ILuaHooker
    {
        Lua50 Lua { set; }
        IEnumerable<LuaHook> Hooks { get; }
    }
}
