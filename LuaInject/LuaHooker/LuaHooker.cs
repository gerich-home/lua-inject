using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LuaInjectAgent;

namespace LuaHooker
{
    [Export(typeof(IHooker))]
    public class LuaHooker : IHooker
    {
        private ArrayList hooks;

        private void SetupHooks(XDocument doc, string pluginsPath, Client Interface)
        {
            // target module with lua functions
            var hookedModule = doc.Root.Attribute("luamodule").Value;

            var lua = new Lua50(hookedModule);

            Func<CompositionContainer, IEnumerable<LuaHook>> GetHooks = c =>
                                                                            {
                                                                                var luaHooker =
                                                                                    c.GetExportedValue<ILuaHooker>();
                                                                                luaHooker.Lua = lua;
                                                                                return luaHooker.Hooks.ToList();
                                                                            };


            // list of hookers (plugins) with their path, 
            var hookers = (from containerPath in
                               (from path in
                                    (from hooker in doc.Root.Element("Hookers").Elements("Hooker")
                                     select
                                         Path.Combine(Path.Combine(pluginsPath, @"LuaHookerPlugins"), string.Format("{0}.dll", hooker.Value)))
                                select new
                                           {
                                               container = new CompositionContainer(new AssemblyCatalog(path)),
                                               path
                                           })
                           select new
                                      {
                                          Container = containerPath.container,
                                          Path = containerPath.path,
                                          Hooks = GetHooks(containerPath.container)
                                      }).ToList();

            hooks = new ArrayList();

            foreach (var firstHooker in hookers)
            {
                foreach (var secondHooker in hookers)
                {
                    if (firstHooker == secondHooker)
                        continue;

                    var common =
                        (from hook in firstHooker.Hooks select hook.Name).Intersect(from hook in secondHooker.Hooks
                                                                                    select hook.Name);
                    foreach (var c in common)
                    {
                        throw new Exception(string.Format("Hookers {0} and {1} has the same hooked function {2}", firstHooker.Path, secondHooker.Path, c));
                    }
                }
            }

            foreach (var hooker in hookers)
            {

                foreach (var activeHooks in hooker.Hooks)
                {
                    var hookerMethod = activeHooks.HookerFunction.Method;
                    if (hookerMethod.GetParameters().Count() != 1)
                    {
                        throw new Exception(string.Format("{0}, {1}: parameters count should be equal to 1", hooker.Path, activeHooks.Name));
                    }

                    var delegateType = hookerMethod.GetParameters()[0].ParameterType;
                    if (!typeof(Delegate).IsAssignableFrom(delegateType))
                    {
                        throw new Exception(string.Format("{0}, {1}: parameter should be a delegate\ncurrent parameter type is {2}", hooker.Path, activeHooks.Name, delegateType));
                    }
                    if (hookerMethod.ReturnType != delegateType)
                    {
                        throw new Exception(string.Format("{0}, {1}: return type should be same as parameter type", hooker.Path, activeHooks.Name));
                    }

                    hooks.Add(typeof(Utils)
                        .GetMethod("SetupHook")
                        .MakeGenericMethod(delegateType)
                        .Invoke(null, new object[] {
                                              activeHooks.Name,
                                              hookedModule,
                                              activeHooks.HookerFunction,
                                              null
                                          }));
                }
            }
        }

        public void Hook(string targetExecutable, string configPath, string pluginsPath, Client Interface)
        {
            var doc = XDocument.Load(Path.Combine(configPath, "LuaHooker.xml"));
            SetupHooks(doc, pluginsPath, Interface);
        }
    }
}
