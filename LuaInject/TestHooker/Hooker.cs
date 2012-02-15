using System.ComponentModel.Composition;
using System.Windows.Forms;
using LuaHooker.HookDelegates;

namespace TestHooker
{
    public static class Hooker
    {
        [Export]
        static luaL_openlib HOOK(luaL_openlib original)
        {
            return (L, s, l, nup) =>
                       {
                           MessageBox.Show(string.Format("luaL_openlib is called for {0}", s));
                           original(L, s, l, nup);
                       };
        }
    }
}
