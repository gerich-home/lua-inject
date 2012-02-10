using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CachingSampleContracts;
using System.ComponentModel.Composition;

namespace CachingSampleParts
{
    [Plugin("Plugin A")]
    public class PluginA
        : IPlugin
    {
        public PluginA()
        {
            Console.WriteLine("\tPlugin A created");
        }
    }

    [Plugin("Plugin B")]
    public class PluginB
        : IPlugin
    {
        public PluginB()
        {
            Console.WriteLine("\tPlugin B created");
        }

    }

}
