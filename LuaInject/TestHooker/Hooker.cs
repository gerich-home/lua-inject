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
        static int test(IntPtr L)
        {
            MessageBox.Show("Hacked");

            return 0;
        }

        private const int LUA_GLOBALSINDEX = -10001;

        private void lua_getglobal(IntPtr L, string s)
        {
            MessageBox.Show("lua_getglobal 1");
            Lua.lua_pushstring(L, s);
            MessageBox.Show("lua_getglobal 2");
            Lua.lua_gettable(L, LUA_GLOBALSINDEX);
            MessageBox.Show("lua_getglobal 3");
        }

        private void lua_pushcfunction(IntPtr L, Lua50.Types.lua_CFunction f)
        {
            MessageBox.Show("lua_pushcfunction 1");
            Lua.lua_pushcclosure(L, f, 0);
            MessageBox.Show("lua_pushcfunction 2");
        }

        private void lua_setglobal(IntPtr L, string s)
        {
            MessageBox.Show("lua_setglobal 1");
            Lua.lua_pushstring(L, s);
            MessageBox.Show("lua_setglobal 2");
            Lua.lua_insert(L, -2);
            MessageBox.Show("lua_setglobal 3");
            Lua.lua_settable(L, LUA_GLOBALSINDEX);
            MessageBox.Show("lua_setglobal 4");
        }

        private Lua50.Types.luaopen_base luaopen_base(Lua50.Types.luaopen_base original)
        {
            MessageBox.Show("setting up luaopen_base hook");
            return (L) =>
                       {

                           try
                           {
                               MessageBox.Show("registering test");
                               MessageBox.Show("1");

                               lua_pushcfunction(L, test);
                               MessageBox.Show("2");
                               lua_setglobal(L, "hack");
                               MessageBox.Show("3");


                               lua_getglobal(L, "hack");
                               MessageBox.Show("4");
                               Lua.lua_pcall(L, 0, 0, 0);
                               MessageBox.Show("5");
                           }
                           catch (Exception e)
                           {
                               MessageBox.Show(e.Message);
                               MessageBox.Show(e.StackTrace);
                               throw;
                           }

                           return original(L);
                       };
        }


        public Lua50 Lua { get; set; }

        public IEnumerable<LuaHook> Hooks
        {
            get
            {
                return new[]
                             {
                                 LuaHook.Create<Lua50.Types.luaopen_base>("luaopen_base", luaopen_base),
                             };
            }
        }
    }
}
