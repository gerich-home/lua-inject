using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LuaHooker;

namespace TestHooker
{
    [Export(typeof(ILuaHooker))]
    public class Hooker : ILuaHooker
    {
        //int luaL_newmetatable (lua_State *L, const char *tname);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int luaL_newmetatable(IntPtr L, String tname);

        //int luaopen_base (lua_State *L);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int luaopen_base(IntPtr L);

        //int luaopen_table (lua_State *L);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int luaopen_table(IntPtr L);

        //int luaopen_io (lua_State *L);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int luaopen_io(IntPtr L);

        //int luaopen_string (lua_State *L);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int luaopen_string(IntPtr L);

        //int luaopen_math (lua_State *L);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int luaopen_math(IntPtr L);

        //int luaopen_debug (lua_State *L);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int luaopen_debug(IntPtr L);

        //void luaL_openlib (lua_State *L, const char *libname, const luaL_reg *l, int nup);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void luaL_openlib(IntPtr L, String s, IntPtr l, int nup);


        private static luaL_openlib luaL_openlib_hook(luaL_openlib original)
        {
            return (L, s, l, nup) =>
                       {
                           MessageBox.Show(string.Format("luaL_openlib is called for {0}", s));
                           original(L, s, l, nup);
                       };
        }

        public IEnumerable<LuaHook> Hooks
        {
            get
            {
                return new[]
                             {
                                 LuaHook.Create<luaL_openlib>("luaL_openlib", luaL_openlib_hook),
                             };
            }
        }
    }
}
