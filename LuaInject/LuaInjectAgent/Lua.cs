using System;
using System.Runtime.InteropServices;

namespace LuaInjectAgent
{
    public class Lua
    {
        internal Lua(string hookedModule, Client Interface)
        {

            SetupHooks(hookedModule, Interface);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void luaL_openlib_Delegate(IntPtr L, String s, IntPtr l, int nup);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int luaopen_mod_box2d_Delegate(IntPtr L);

        private object luaL_openlib_hook;

        private void SetupHooks(string hookedModule, Client Interface)
        {
            luaL_openlib_hook = Utils.SetupHook<luaL_openlib_Delegate>("luaL_openlib", hookedModule,
                (original) => (L, s, l, nup) =>
                {
                    Interface.Echo("luaL_openlib called for module '" + s + "'");
                    original(L, s, l, nup);
                });
        }
    }
}
