using System;
using System.Collections;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using LuaInjectAgent;
using System.ComponentModel.Composition;

namespace LuaHooker
{
    [Export(typeof(IHooker))]
    public class LuaHooker : IHooker
    {
        private ArrayList hooks;

        private void SetupHooks(string hookedModule, string configPath, string pluginsPath, Client Interface)
        {
            var doc = XDocument.Load(Path.Combine(configPath, "LuaHooker.xml"));
            var hookersFromConfig = from hooker in doc.Root.Element("Hookers").Elements("Hooker")
                                    select new
                                               {
                                                   Path = Path.Combine(Path.Combine(pluginsPath, @"LuaHookerPlugins"), hooker.Value),
                                                   Name = hooker.Attribute("name").Value
                                               };

            var hooksFromConfig = from hook in doc.Root.Element("Hooks").Elements("Hook")
                                  select new
                                             {
                                                 Name = hook.Attribute("name").Value,
                                                 HookerName = hook.Value
                                             };

            var availableHooks = from t in Assembly.GetExecutingAssembly().GetTypes()
                                 where t.Namespace == "LuaHooker.HookDelegates" && typeof(Delegate).IsAssignableFrom(t)
                                 select t.Name;

            var hooksWithContainer = from activeHookers in
                                         (from activeHook in
                                              (from
                                                   availableHook in availableHooks
                                               join
                                                   hook in hooksFromConfig
                                                   on availableHook equals hook.Name
                                               select hook)
                                          group activeHook by activeHook.HookerName)
                                     join hooker in hookersFromConfig on activeHookers.Key equals hooker.Name
                                     select new
                                                {
                                                    Container = new CompositionContainer(new AssemblyCatalog(hooker.Path)),
                                                    activeHookers
                                                };

            hooks = new ArrayList();

            foreach (var hookWithContainer in hooksWithContainer)
            {
                foreach (var activeHooker in hookWithContainer.activeHookers)
                {
                    var delegateType = Type.GetType(string.Format("LuaHooker.HookDelegates.{0}", activeHooker.Name));
                    
                    var hookerProcType = typeof(Func<,>)
                        .MakeGenericType(delegateType, delegateType);

                    var impotedMethod = typeof(ExportProvider)
                        .GetMethod("GetExportedValue", Type.EmptyTypes)
                        .MakeGenericMethod(hookerProcType)
                        .Invoke(hookWithContainer.Container, new object[] { });

                    hooks.Add(typeof(Utils)
                        .GetMethod("SetupHook")
                        .MakeGenericMethod(delegateType)
                        .Invoke(null, new[] {
                                              activeHooker.Name,
                                              hookedModule,
                                              impotedMethod,
                                              null
                                          }));
                }
            }
        }

        public void Hook(string targetExecutable, string configPath, string pluginsPath, Client Interface)
        {
            SetupHooks("lua5.1.dll", configPath, pluginsPath, Interface);
        }
    }
}
