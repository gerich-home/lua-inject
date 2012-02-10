// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Reflection;
using System.ComponentModel.Composition;

namespace Microsoft.ComponentModel.Composition.DynamicInstantiation
{
    public class DynamicInstantiationExportProvider : ExportProvider
    {
        static readonly IEnumerable<Export> EmptyExports = Enumerable.Empty<Export>();

        static readonly string PartCreatorContractPrefix =
            typeof(PartCreator<>).FullName.Substring(0, typeof(PartCreator<>).FullName.IndexOf("`"));

        ConcurrentCache<ContractBasedImportDefinition, PartCreatorImport> _importDefinitionCache =
            new ConcurrentCache<ContractBasedImportDefinition, PartCreatorImport>();

        ExportProvider _sourceProvider;

        public ExportProvider SourceProvider
        {
            get { return _sourceProvider; }
            set
            {
                if (value != _sourceProvider)
                {
                    if (_sourceProvider != null)
                    {
                        _sourceProvider.ExportsChanging -= SourceExportsChanging;
                        _sourceProvider.ExportsChanged -= SourceExportsChanged;
                    }

                    _sourceProvider = value;

                    if (_sourceProvider != null)
                    {
                        _sourceProvider.ExportsChanging += SourceExportsChanging;
                        _sourceProvider.ExportsChanged += SourceExportsChanged;
                    }
                }
            }
        }

        class TaggedExportsChangedEventArgs : ExportsChangeEventArgs
        {
            public TaggedExportsChangedEventArgs(object sender, IEnumerable<ExportDefinition> added, IEnumerable<ExportDefinition> removed, AtomicComposition atomicComposition)
                : base(added, removed, atomicComposition)
            {
                Sender = sender;
            }

            public object Sender { get; private set; }
        }

        void SourceExportsChanging(object sender, ExportsChangeEventArgs e)
        {
            if (SentByThis(e))
                return;

            OnExportsChanging(ProjectChangeEvent(e));
        }

        ExportsChangeEventArgs ProjectChangeEvent(ExportsChangeEventArgs e)
        {
            var satisfiedImports = _importDefinitionCache.Values;

            return new TaggedExportsChangedEventArgs(
                this,
                ProjectProductExportsIntoSatsifedCreatorExports(satisfiedImports, e.AddedExports),
                ProjectProductExportsIntoSatsifedCreatorExports(satisfiedImports, e.RemovedExports),
                e.AtomicComposition);
        }

        bool SentByThis(ExportsChangeEventArgs e)
        {
            return e is TaggedExportsChangedEventArgs &&
                ((TaggedExportsChangedEventArgs)e).Sender == this;
        }

        void SourceExportsChanged(object sender, ExportsChangeEventArgs e)
        {
            if (SentByThis(e))
                return;

            OnExportsChanged(ProjectChangeEvent(e));
        }

        IEnumerable<ExportDefinition> ProjectProductExportsIntoSatsifedCreatorExports(
            IEnumerable<PartCreatorImport> satisfiedImports,
            IEnumerable<ExportDefinition> changedProductExports)
        {
           return from s in satisfiedImports
                from a in changedProductExports
                where s.ProductImport.IsConstraintSatisfiedBy(a)
                select s.CreateMatchingExportDefinition(a);
        }


        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (SourceProvider == null) throw new InvalidOperationException("SourceProvider must be set.");

            var cbid = definition as ContractBasedImportDefinition;
            
            if (cbid == null || !cbid.RequiredTypeIdentity.StartsWith(PartCreatorContractPrefix))
                return EmptyExports;

            var importInfo = _importDefinitionCache.GetOrCreate(
                cbid,
                () => new PartCreatorImport(cbid));
            
            var sourceExports = SourceProvider
                .GetExports(importInfo.ProductImport, atomicComposition);

            var result = sourceExports
                .Select(e => importInfo.CreateMatchingExport(e.Definition, SourceProvider))
                .ToArray();

            foreach (var e in sourceExports.OfType<IDisposable>())
                e.Dispose();

            return result;
        }
    }
}
