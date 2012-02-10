using System;
using System.Runtime.InteropServices;
using LuaInjectAgent;
using System.ComponentModel.Composition;

namespace LuaHooker
{
    [Export(typeof(IHooker))]
    public class LuaHooker : IHooker
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void luaL_openlib_Delegate(IntPtr L, String s, IntPtr l, int nup);
        private object luaL_openlib_hook;
        public luaL_openlib_Delegate luaL_openlib;

        private void SetupHooks(string hookedModule, Client Interface)
        {
            luaL_openlib_hook = Utils.SetupHook<luaL_openlib_Delegate>("luaL_openlib", hookedModule,
                (original) => (L, s, l, nup) =>
                {
                    luaL_openlib = original;
                    Interface.Echo("luaL_openlib called for module '" + s + "'");
                    original(L, s, l, nup);
                });
        }

        public void Hook(Client Interface)
        {
            SetupHooks("lua5.1.dll", Interface);
        }
    }
}
