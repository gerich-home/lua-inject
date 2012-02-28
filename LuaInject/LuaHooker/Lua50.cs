using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EasyHook;

namespace LuaHooker
{
    public class Lua50
    {
        public static class Types
        {
            //int (*lua_CFunction) (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_CFunction(IntPtr L);

            //int const char * (*lua_Chunkreader) (lua_State *L, void *ud, size_t *sz);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_Chunkreader(IntPtr L, IntPtr ud, IntPtr sz);

            //int (*lua_Chunkwriter) (lua_State *L, const void* p, size_t sz, void* ud);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_Chunkwriter(IntPtr L, string p, IntPtr sz, IntPtr ud);

            [StructLayout(LayoutKind.Sequential)]
            private struct luaL_Reg
            {
                public string name;
                [MarshalAs(UnmanagedType.FunctionPtr)]
                public lua_CFunction func;
            };

            //void (*lua_Hook) (lua_State *L, lua_Debug *ar);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_Hook(IntPtr L, IntPtr ar);

            //lua_CFunction lua_atpanic (lua_State *L, lua_CFunction panicf);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate lua_CFunction lua_atpanic(IntPtr L, lua_CFunction panicf);

            //void lua_call (lua_State *L, int nargs, int nresults);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_call(IntPtr L, int nargs, int nresults);

            //int lua_checkstack (lua_State *L, int sz);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_checkstack(IntPtr L, int sz);

            //void lua_close (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_close(IntPtr L);

            //void lua_concat (lua_State *L, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_concat(IntPtr L, int n);

            //int lua_cpcall (lua_State *L, lua_CFunction func, void *ud);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_cpcall(IntPtr L, lua_CFunction func, IntPtr ud);

            //int lua_dobuffer (lua_State *L, const char *buff, size_t sz, const char *n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_dobuffer(IntPtr L, string buff, IntPtr sz, string n);

            //int lua_dofile (lua_State *L, const char *filename);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_dofile(IntPtr L, string filename);

            //int lua_dostring (lua_State *L, const char *str);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_dostring(IntPtr L, string str);

            //int lua_dump (lua_State *L, lua_Chunkwriter writer, void *data);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_dump(IntPtr L, lua_Chunkwriter writer, IntPtr data);

            //int lua_equal (lua_State *L, int idx1, int idx2);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_equal(IntPtr L, int idx1, int idx2);

            //int lua_error (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_error(IntPtr L);

            //void lua_getfenv (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_getfenv(IntPtr L, int idx);

            //int lua_getgccount (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_getgccount(IntPtr L);

            //int lua_getgcthreshold (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_getgcthreshold(IntPtr L);

            //lua_Hook lua_gethook (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate lua_Hook lua_gethook(IntPtr L);

            //int lua_gethookcount (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_gethookcount(IntPtr L);

            //int lua_gethookmask (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_gethookmask(IntPtr L);

            //int lua_getinfo (lua_State *L, const char *what, lua_Debug *ar);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_getinfo(IntPtr L, string what, IntPtr ar);

            //const char* lua_getlocal (lua_State *L, const lua_Debug *ar, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_getlocal(IntPtr L, IntPtr ar, int n);

            //int lua_getmetatable (lua_State *L, int objindex);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_getmetatable(IntPtr L, int objindex);

            //int lua_getstack (lua_State *L, int level, lua_Debug *ar);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_getstack(IntPtr L, int level, IntPtr ar);

            //void lua_gettable (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_gettable(IntPtr L, int idx);

            //int lua_gettop (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_gettop(IntPtr L);

            //const char* lua_getupvalue (lua_State *L, int funcindex, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_getupvalue(IntPtr L, int funcindex, int n);

            //void lua_insert (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_insert(IntPtr L, int idx);

            //int lua_iscfunction (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_iscfunction(IntPtr L, int idx);

            //int lua_isnumber (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_isnumber(IntPtr L, int idx);

            //int lua_isstring (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_isstring(IntPtr L, int idx);

            //int lua_isuserdata (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_isuserdata(IntPtr L, int idx);

            //int lua_lessthan (lua_State *L, int idx1, int idx2);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_lessthan(IntPtr L, int idx1, int idx2);

            //int lua_load (lua_State *L, lua_Chunkreader reader, void *dt, const char *chunkname);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_load(IntPtr L, lua_Chunkreader reader, IntPtr dt, string chunkname);

            //void lua_newtable (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_newtable(IntPtr L);

            //lua_State* lua_newthread (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr lua_newthread(IntPtr L);

            //void* lua_newuserdata (lua_State *L, size_t sz);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr lua_newuserdata(IntPtr L, IntPtr sz);

            //int lua_next (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_next(IntPtr L, int idx);

            //lua_State* lua_open (void);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr lua_open();

            //int lua_pcall (lua_State *L, int nargs, int nresults, int errfunc);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_pcall(IntPtr L, int nargs, int nresults, int errfunc);

            //void lua_pushboolean (IntPtr L, int b);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushboolean(IntPtr L, int b);

            //void lua_pushcclosure (IntPtr L, lua_CFunction fn, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushcclosure(IntPtr L, lua_CFunction fn, int n);

            //const char* lua_pushfstring (IntPtr L, string fmt, ...);
            //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            //public delegate const char* lua_pushfstring (IntPtr L, string fmt, ...);

            //void lua_pushlightuserdata (IntPtr L, void *p);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushlightuserdata(IntPtr L, IntPtr p);

            //void lua_pushlstring (IntPtr L, string s, size_t l);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushlstring(IntPtr L, string s, IntPtr l);

            //void lua_pushnil (IntPtr L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushnil(IntPtr L);

            //void lua_pushnumber (IntPtr L, lua_Number n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushnumber(IntPtr L, double n);

            //void lua_pushstring (IntPtr L, string s);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushstring(IntPtr L, string s);

            //int lua_pushupvalues (IntPtr L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_pushupvalues(IntPtr L);

            //void lua_pushvalue (IntPtr L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_pushvalue(IntPtr L, int idx);

            //const char* lua_pushvfstring (IntPtr L, string fmt, va_list argp);
            //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            //public delegate string lua_pushvfstring (IntPtr L, string fmt, va_list argp);

            //int lua_rawequal (lua_State *L, int idx1, int idx2);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_rawequal(IntPtr L, int idx1, int idx2);

            //void lua_rawget (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_rawget(IntPtr L, int idx);

            //void lua_rawgeti (lua_State *L, int idx, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_rawgeti(IntPtr L, int idx, int n);

            //void lua_rawset (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_rawset(IntPtr L, int idx);

            //void lua_rawseti (lua_State *L, int idx, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_rawseti(IntPtr L, int idx, int n);

            //void lua_remove (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_remove(IntPtr L, int idx);

            //void lua_replace (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_replace(IntPtr L, int idx);

            //int lua_resume (lua_State *L, int narg);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_resume(IntPtr L, int narg);

            //int lua_setfenv (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_setfenv(IntPtr L, int idx);

            //void lua_setgcthreshold (lua_State *L, int newthreshold);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_setgcthreshold(IntPtr L, int newthreshold);

            //int lua_sethook (lua_State *L, lua_Hook func, int mask, int count);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_sethook(IntPtr L, lua_Hook func, int mask, int count);

            //const char* lua_setlocal (lua_State *L, const lua_Debug *ar, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_setlocal(IntPtr L, IntPtr ar, int n);

            //int lua_setmetatable (lua_State *L, int objindex);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_setmetatable(IntPtr L, int objindex);

            //void lua_settable (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_settable(IntPtr L, int idx);

            //void lua_settop (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_settop(IntPtr L, int idx);

            //const char* lua_setupvalue (lua_State *L, int funcindex, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_setupvalue(IntPtr L, int funcindex, int n);

            //size_t lua_strlen (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr lua_strlen(IntPtr L, int idx);

            //int lua_toboolean (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_toboolean(IntPtr L, int idx);

            //lua_CFunction lua_tocfunction (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate lua_CFunction lua_tocfunction(IntPtr L, int idx);

            //lua_Number lua_tonumber (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate double lua_tonumber(IntPtr L, int idx);

            //const void* lua_topointer (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_topointer(IntPtr L, int idx);

            //const char* lua_tostring (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_tostring(IntPtr L, int idx);

            //lua_State* lua_tothread (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr lua_tothread(IntPtr L, int idx);

            //void* lua_touserdata (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr lua_touserdata(IntPtr L, int idx);

            //int lua_type (lua_State *L, int idx);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_type(IntPtr L, int idx);

            //const char* lua_typename (lua_State *L, int tp);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_typename(IntPtr L, int tp);

            //const char* lua_version (void);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string lua_version();

            //void lua_xmove (lua_State *from, lua_State *to, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void lua_xmove(IntPtr from, IntPtr to, int n);

            //int lua_yield (lua_State *L, int nresults);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int lua_yield(IntPtr L, int nresults);

            //void luaL_addlstring (luaL_Buffer *B, const char *s, size_t l);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_addlstring(IntPtr B, string s, IntPtr l);

            //void luaL_addstring (luaL_Buffer *B, const char *s);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_addstring(IntPtr B, string s);

            //void luaL_addvalue (luaL_Buffer *B);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_addvalue(IntPtr B);

            //int luaL_argerror (lua_State *L, int numarg, const char *extramsg);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_argerror(IntPtr L, int numarg, string extramsg);

            //void luaL_buffinit (lua_State *L, luaL_Buffer *B);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_buffinit(IntPtr L, IntPtr B);

            //int luaL_callmeta (lua_State *L, int obj, const char *e);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_callmeta(IntPtr L, int obj, string e);

            //void luaL_checkany (lua_State *L, int narg);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_checkany(IntPtr L, int narg);

            //const char* luaL_checklstring (lua_State *L, int numArg, size_t *l);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string luaL_checklstring(IntPtr L, int numArg, IntPtr l);

            //lua_Number luaL_checknumber (lua_State *L, int numArg);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate double luaL_checknumber(IntPtr L, int numArg);

            //void luaL_checkstack (lua_State *L, int sz, const char *msg);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_checkstack(IntPtr L, int sz, string msg);

            //void luaL_checktype (lua_State *L, int narg, int t);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_checktype(IntPtr L, int narg, int t);

            //void* luaL_checkudata (lua_State *L, int ud, const char *tname);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string luaL_checkudata(IntPtr L, int ud, string tname);

            //int luaL_error (lua_State *L, const char *fmt, ...);
            //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            //public delegate int luaL_error (IntPtr L, string fmt, ...);

            //int luaL_findstring (const char *st, const char *const lst[]);
            //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            //public delegate int luaL_findstring (string st, string const lst[]);

            //int luaL_getmetafield (lua_State *L, int obj, const char *e);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_getmetafield(IntPtr L, int obj, string e);

            //void luaL_getmetatable (lua_State *L, const char *tname);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_getmetatable(IntPtr L, string tname);

            //int luaL_getn (lua_State *L, int t);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_getn(IntPtr L, int t);

            //int luaL_loadbuffer (lua_State *L, const char *buff, size_t sz, const char *name);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_loadbuffer(IntPtr L, string buff, IntPtr sz, string name);

            //int luaL_loadfile (lua_State *L, const char *filename);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_loadfile(IntPtr L, string filename);

            //int luaL_newmetatable (lua_State *L, const char *tname);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_newmetatable(IntPtr L, string tname);

            //void luaL_openlib (lua_State *L, const char *libname, const luaL_reg *l, int nup);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_openlib(IntPtr L, string libname, IntPtr l, int nup);

            //const char* luaL_optlstring (lua_State *L, int numArg, const char *def, size_t *l);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string luaL_optlstring(IntPtr L, int numArg, string def, IntPtr l);

            //lua_Number luaL_optnumber (lua_State *L, int nArg, lua_Number def);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate double luaL_optnumber(IntPtr L, int nArg, double def);

            //char* luaL_prepbuffer (luaL_Buffer *B);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate string luaL_prepbuffer(IntPtr B);

            //void luaL_pushresult (IntPtr B);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_pushresult(IntPtr B);

            //int luaL_ref (lua_State *L, int t);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_ref(IntPtr L, int t);

            //void luaL_setn (lua_State *L, int t, int n);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_setn(IntPtr L, int t, int n);

            //int luaL_typerror (lua_State *L, int narg, const char *tname);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaL_typerror(IntPtr L, int narg, string tname);

            //void luaL_unref (lua_State *L, int t, int ref);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_unref(IntPtr L, int t, int _ref);

            //void luaL_where (lua_State *L, int lvl);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void luaL_where(IntPtr L, int lvl);

            //int luaopen_base (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaopen_base(IntPtr L);

            //int luaopen_debug (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaopen_debug(IntPtr L);

            //int luaopen_io (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaopen_io(IntPtr L);

            //int luaopen_math (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaopen_math(IntPtr L);

            //int luaopen_string (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaopen_string(IntPtr L);

            //int luaopen_table (lua_State *L);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int luaopen_table(IntPtr L);
        }

        public readonly Types.lua_atpanic lua_atpanic;
        public readonly Types.lua_call lua_call;
        public readonly Types.lua_checkstack lua_checkstack;
        public readonly Types.lua_close lua_close;
        public readonly Types.lua_concat lua_concat;
        public readonly Types.lua_cpcall lua_cpcall;
        public readonly Types.lua_dobuffer lua_dobuffer;
        public readonly Types.lua_dofile lua_dofile;
        public readonly Types.lua_dostring lua_dostring;
        public readonly Types.lua_dump lua_dump;
        public readonly Types.lua_equal lua_equal;
        public readonly Types.lua_error lua_error;
        public readonly Types.lua_getfenv lua_getfenv;
        public readonly Types.lua_getgccount lua_getgccount;
        public readonly Types.lua_getgcthreshold lua_getgcthreshold;
        public readonly Types.lua_gethook lua_gethook;
        public readonly Types.lua_gethookcount lua_gethookcount;
        public readonly Types.lua_gethookmask lua_gethookmask;
        public readonly Types.lua_getinfo lua_getinfo;
        public readonly Types.lua_getlocal lua_getlocal;
        public readonly Types.lua_getmetatable lua_getmetatable;
        public readonly Types.lua_getstack lua_getstack;
        public readonly Types.lua_gettable lua_gettable;
        public readonly Types.lua_gettop lua_gettop;
        public readonly Types.lua_getupvalue lua_getupvalue;
        public readonly Types.lua_insert lua_insert;
        public readonly Types.lua_iscfunction lua_iscfunction;
        public readonly Types.lua_isnumber lua_isnumber;
        public readonly Types.lua_isstring lua_isstring;
        public readonly Types.lua_isuserdata lua_isuserdata;
        public readonly Types.lua_lessthan lua_lessthan;
        public readonly Types.lua_load lua_load;
        public readonly Types.lua_newtable lua_newtable;
        public readonly Types.lua_newthread lua_newthread;
        public readonly Types.lua_newuserdata lua_newuserdata;
        public readonly Types.lua_next lua_next;
        public readonly Types.lua_open lua_open;
        public readonly Types.lua_pcall lua_pcall;
        public readonly Types.lua_pushboolean lua_pushboolean;
        public readonly Types.lua_pushcclosure lua_pushcclosure;
        //public readonly Types.lua_pushfstring lua_pushfstring;
        public readonly Types.lua_pushlightuserdata lua_pushlightuserdata;
        public readonly Types.lua_pushlstring lua_pushlstring;
        public readonly Types.lua_pushnil lua_pushnil;
        public readonly Types.lua_pushnumber lua_pushnumber;
        public readonly Types.lua_pushstring lua_pushstring;
        public readonly Types.lua_pushupvalues lua_pushupvalues;
        public readonly Types.lua_pushvalue lua_pushvalue;
        //public readonly Types.lua_pushvfstring lua_pushvfstring;
        public readonly Types.lua_rawequal lua_rawequal;
        public readonly Types.lua_rawget lua_rawget;
        public readonly Types.lua_rawgeti lua_rawgeti;
        public readonly Types.lua_rawset lua_rawset;
        public readonly Types.lua_rawseti lua_rawseti;
        public readonly Types.lua_remove lua_remove;
        public readonly Types.lua_replace lua_replace;
        public readonly Types.lua_resume lua_resume;
        public readonly Types.lua_setfenv lua_setfenv;
        public readonly Types.lua_setgcthreshold lua_setgcthreshold;
        public readonly Types.lua_sethook lua_sethook;
        public readonly Types.lua_setlocal lua_setlocal;
        public readonly Types.lua_setmetatable lua_setmetatable;
        public readonly Types.lua_settable lua_settable;
        public readonly Types.lua_settop lua_settop;
        public readonly Types.lua_setupvalue lua_setupvalue;
        public readonly Types.lua_strlen lua_strlen;
        public readonly Types.lua_toboolean lua_toboolean;
        public readonly Types.lua_tocfunction lua_tocfunction;
        public readonly Types.lua_tonumber lua_tonumber;
        public readonly Types.lua_topointer lua_topointer;
        public readonly Types.lua_tostring lua_tostring;
        public readonly Types.lua_tothread lua_tothread;
        public readonly Types.lua_touserdata lua_touserdata;
        public readonly Types.lua_type lua_type;
        public readonly Types.lua_typename lua_typename;
        public readonly Types.lua_version lua_version;
        public readonly Types.lua_xmove lua_xmove;
        public readonly Types.lua_yield lua_yield;
        public readonly Types.luaL_addlstring luaL_addlstring;
        public readonly Types.luaL_addstring luaL_addstring;
        public readonly Types.luaL_addvalue luaL_addvalue;
        public readonly Types.luaL_argerror luaL_argerror;
        public readonly Types.luaL_buffinit luaL_buffinit;
        public readonly Types.luaL_callmeta luaL_callmeta;
        public readonly Types.luaL_checkany luaL_checkany;
        public readonly Types.luaL_checklstring luaL_checklstring;
        public readonly Types.luaL_checknumber luaL_checknumber;
        public readonly Types.luaL_checkstack luaL_checkstack;
        public readonly Types.luaL_checktype luaL_checktype;
        public readonly Types.luaL_checkudata luaL_checkudata;
        //public readonly Types.luaL_error luaL_error;
        //public readonly Types.luaL_findstring luaL_findstring;
        public readonly Types.luaL_getmetafield luaL_getmetafield;
        public readonly Types.luaL_getmetatable luaL_getmetatable;
        public readonly Types.luaL_getn luaL_getn;
        public readonly Types.luaL_loadbuffer luaL_loadbuffer;
        public readonly Types.luaL_loadfile luaL_loadfile;
        public readonly Types.luaL_newmetatable luaL_newmetatable;
        public readonly Types.luaL_openlib luaL_openlib;
        public readonly Types.luaL_optlstring luaL_optlstring;
        public readonly Types.luaL_optnumber luaL_optnumber;
        public readonly Types.luaL_prepbuffer luaL_prepbuffer;
        public readonly Types.luaL_pushresult luaL_pushresult;
        public readonly Types.luaL_ref luaL_ref;
        public readonly Types.luaL_setn luaL_setn;
        public readonly Types.luaL_typerror luaL_typerror;
        public readonly Types.luaL_unref luaL_unref;
        public readonly Types.luaL_where luaL_where;
        public readonly Types.luaopen_base luaopen_base;
        public readonly Types.luaopen_debug luaopen_debug;
        public readonly Types.luaopen_io luaopen_io;
        public readonly Types.luaopen_math luaopen_math;
        public readonly Types.luaopen_string luaopen_string;
        public readonly Types.luaopen_table luaopen_table;

        public Lua50(string hookedModule)
        {
            foreach (var prop in typeof(Lua50).GetFields().Where(prop => typeof(Delegate).IsAssignableFrom(prop.FieldType)))
            {
                try
                {
                    prop.SetValue(this,
                        typeof(LocalHook)
                        .GetMethod("GetProcDelegate")
                        .MakeGenericMethod(prop.FieldType)
                        .Invoke(null, new object[] {
                                              hookedModule,
                                              prop.Name
                                                }));
                }
                catch
                {
                    MessageBox.Show(string.Format("can't hook {0}", prop.Name));
                }
            }
        }
    }
}
