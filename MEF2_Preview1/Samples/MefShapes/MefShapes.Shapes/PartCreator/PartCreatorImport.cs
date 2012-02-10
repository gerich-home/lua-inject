using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using System.ComponentModel.Composition.ReflectionModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Microsoft.ComponentModel.Composition.DynamicInstantiation
{
    class PartCreatorImport
    {
        const string PropertySetterPrefix = "set_";

        static readonly MethodInfo CreatePartCreatorMethod =
            typeof(PartCreatorImport).GetMethod("CreatePartCreatorOfT", BindingFlags.Static | BindingFlags.NonPublic);

        static readonly MethodInfo CreatePartCreatorWithMetadataMethod =
            typeof(PartCreatorImport).GetMethod("CreatePartCreatorOfTWithMetadata", BindingFlags.Static | BindingFlags.NonPublic);

        public PartCreatorImport(ContractBasedImportDefinition partCreatorImport)
        {
            ContractName = partCreatorImport.ContractName;

            PartCreatorType = GetTargetPartCreatorType(partCreatorImport);

            ExportedValueType = PartCreatorType.GetGenericArguments()[0];

            MetadataViewType = null;
            if (PartCreatorType.GetGenericArguments().Length == 2)
                MetadataViewType = PartCreatorType.GetGenericArguments()[1];

            ProductImport = GetProductImportDefinition(partCreatorImport, ExportedValueType, MetadataViewType);
        }

        public string ContractName { get; private set; }
        public Type PartCreatorType { get; private set; }
        public Type ExportedValueType { get; private set; }
        public Type MetadataViewType { get; private set; }
        public ContractBasedImportDefinition ProductImport { get; private set; }

        public ExportDefinition CreateMatchingExportDefinition(ExportDefinition productExportDefinition)
        {
            return new ExportDefinition(ContractName, GetPartCreatorExportMetadata(productExportDefinition.Metadata));
        }

        IDictionary<string, object> GetPartCreatorExportMetadata(IDictionary<string, object> productMeta)
        {
            var result = new Dictionary<string, object>(productMeta);
            result[CompositionConstants.ExportTypeIdentityMetadataName] =
                AttributedModelServices.GetTypeIdentity(PartCreatorType);
            return result;
        }

        ContractBasedImportDefinition GetProductImportDefinition(ContractBasedImportDefinition cbid, Type createdType, Type metadataViewType)
        {
            var productContractName =
                cbid.ContractName == cbid.RequiredTypeIdentity ?
                AttributedModelServices.GetContractName(createdType) :
                cbid.ContractName;

            return new ContractBasedImportDefinition(
                productContractName,
                AttributedModelServices.GetTypeIdentity(createdType),
                GetProductRequiredMetadataKeys(cbid.RequiredMetadata, metadataViewType),
                cbid.Cardinality,
                cbid.IsRecomposable,
                cbid.IsPrerequisite,
                CreationPolicy.NonShared);
        }

        static Type GetTargetPartCreatorType(ContractBasedImportDefinition partCreatorImportDefinition)
        {
            Type importType = null;

            var member = ReflectionModelServices.GetImportingMember(partCreatorImportDefinition);
            if (member != null)
            {
                var setMi = member.GetAccessors()
                    .Where(a => a.Name.StartsWith(PropertySetterPrefix))
                    .OfType<MethodInfo>()
                    .SingleOrDefault();

                if (setMi != null)
                {
                    importType = setMi.GetParameters()
                        .First()
                        .ParameterType;
                }
                else
                {
                    importType = member.GetAccessors()
                        .OfType<FieldInfo>()
                        .Single()
                        .FieldType;
                }
            }
            else
            {
                var param = ReflectionModelServices.GetImportingParameter(partCreatorImportDefinition);
                if (param != null)
                    importType = param.Value.ParameterType;
                else
                    throw new NotSupportedException("Import type not supported.");
            }

            if (partCreatorImportDefinition.Cardinality == ImportCardinality.ZeroOrMore)
            {
                if (importType.IsGenericType)
                    importType = importType.GetGenericArguments()[0];
                else if (importType.IsArray)
                    importType = importType.GetElementType();
            }

            if (!(importType.IsGenericType &&
                importType.GetGenericTypeDefinition() == typeof(PartCreator<>) ||
                importType.GetGenericTypeDefinition() == typeof(PartCreator<,>)))
                throw new NotSupportedException("Import type not supported.");

            return importType;
        }

        IEnumerable<KeyValuePair<string, Type>> GetProductRequiredMetadataKeys(IEnumerable<KeyValuePair<string, Type>> standardRequiredKeys, Type metadataViewType)
        {
            var result = standardRequiredKeys;

            if (metadataViewType != null)
                result = result.Union(
                    metadataViewType
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                        .Select(pi => new KeyValuePair<string, Type>(pi.Name, pi.PropertyType)));

            return result;
        }

        public Export CreateMatchingExport(ExportDefinition exportDefinition, ExportProvider sourceProvider)
        {
            return new Export(
                    CreateMatchingExportDefinition(exportDefinition),
                    () => CreatePartCreator(ExportedValueType, MetadataViewType, ProductImport, sourceProvider, exportDefinition));
        }

        static object CreatePartCreator(Type createdType, Type metadataViewType, ContractBasedImportDefinition productImport, ExportProvider sourceProvider, ExportDefinition productDefinition)
        {
            if (metadataViewType == null)
                return CreatePartCreatorMethod.MakeGenericMethod(createdType).Invoke(null, new object[] { productImport, sourceProvider, productDefinition });
            else
                return CreatePartCreatorWithMetadataMethod.MakeGenericMethod(createdType, metadataViewType).Invoke(null, new object[] { productImport, sourceProvider, productDefinition });
        }

        static PartCreator<T> CreatePartCreatorOfTWithMetadata<T, TMetadata>(ImportDefinition productImport, ExportProvider sourceProvider, ExportDefinition productDefinition)
        {
            Func<PartLifetimeContext<T>> creator = CreatePartLifetimeContextCreator<T>(productImport, sourceProvider, productDefinition);
            return new PartCreator<T, TMetadata>(creator, AttributedModelServices.GetMetadataView<TMetadata>(productDefinition.Metadata));
        }

        static PartCreator<T> CreatePartCreatorOfT<T>(ImportDefinition productImport, ExportProvider sourceProvider, ExportDefinition productDefinition)
        {
            Func<PartLifetimeContext<T>> creator = CreatePartLifetimeContextCreator<T>(productImport, sourceProvider, productDefinition);
            return new PartCreator<T>(creator);
        }

        static Func<PartLifetimeContext<T>> CreatePartLifetimeContextCreator<T>(ImportDefinition productImport, ExportProvider sourceProvider, ExportDefinition productDefinition)
        {
            Func<PartLifetimeContext<T>> creator = () =>
            {
                var product = sourceProvider.GetExports(productImport).Single(e => e.Definition == productDefinition);
                return new PartLifetimeContext<T>((T)(product.Value), () =>
                {
                    if (product is IDisposable)
                        ((IDisposable)product).Dispose();
                });
            };
            return creator;
        }
    }
}
