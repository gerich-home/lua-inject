using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel;

namespace CachingSampleContracts
{
    public interface IPlugin
    {
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class PluginAttribute : ExportAttribute
    {
        public PluginAttribute(string name)
            :base(typeof(IPlugin))
        {
            this.Name = name;
        }

        public string Name { get; internal set; }
    }

    public interface IPluginMetadata
    {
        string Name { get; }
    }
}
