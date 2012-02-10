//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Microsoft.ComponentModel.Composition.Diagnostics;

namespace DiagnosticsExample
{
    public class Biff { }

    public interface IBiffMetadata
    {
        string Name { get; }
    }

    public class MissingMeta
    {
        [Export("biff")]
        public Biff Biff = new Biff();
    }

    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WrongCreationPolicy
    {
        [Export("biff"), ExportMetadata("Name", "BiffBiff")]
        public Biff Biff = new Biff();
    }

    public class WrongTypeIdentity
    {
        [Export("biff"), ExportMetadata("Name", "Baf")]
        public string Biff = "Yep";
    }

    [Export]
    public class Bar
    {
        [Import("biff", RequiredCreationPolicy = CreationPolicy.NonShared)]
        public Lazy<Biff, IBiffMetadata> Biff { get; set; }
    }

    [Export]
    public class Foo
    {
        [Import]
        public Bar Bar { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var cat = new AssemblyCatalog(typeof(Program).Assembly);
            using (var container = new CompositionContainer(cat))
            {
                var ci = new CompositionInfo(cat, container);
                CompositionInfoTextFormatter.Write(ci, Console.Out);
            }

            Console.ReadKey(true);
        }
    }
}
