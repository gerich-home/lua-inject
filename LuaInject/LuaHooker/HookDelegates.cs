using System;
using System.Runtime.InteropServices;

namespace LuaHooker.HookDelegates
{
    //int luaL_newmetatable (lua_State *L, const char *tname);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int luaL_newmetatable(IntPtr L, String tname);

    //int luaopen_base (lua_State *L);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int luaopen_base(IntPtr L);

    //int luaopen_table (lua_State *L);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int luaopen_table(IntPtr L);

    //int luaopen_io (lua_State *L);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int luaopen_io(IntPtr L);

    //int luaopen_string (lua_State *L);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int luaopen_string(IntPtr L);

    //int luaopen_math (lua_State *L);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int luaopen_math(IntPtr L);

    //int luaopen_debug (lua_State *L);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int luaopen_debug(IntPtr L);

    //void luaL_openlib (lua_State *L, const char *libname, const luaL_reg *l, int nup);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void luaL_openlib(IntPtr L, String s, IntPtr l, int nup);
}
