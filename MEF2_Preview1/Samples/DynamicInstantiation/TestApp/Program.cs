using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.ComponentModel.Composition.DynamicInstantiation;
using System.ComponentModel.Composition.Hosting;

namespace TestApp
{
    public interface ICoffeeMetadata { string Origin { get; } }

    public class Coffee : IDisposable
    {
        static Random r = new Random();
        int num = r.Next();

        public override string ToString()
        {
            return num.ToString();
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing {0}", num);
        }
    }

    [Export(typeof(Coffee))]
    [ExportMetadata("Origin", "Sumatra")]
    public class Sumatran : Coffee { }

    [Export(typeof(Coffee))]
    [ExportMetadata("Origin", "Brazil")]
    public class Brazilian : Coffee { }

    [Export]
    public class Cafe
    {
        [ImportMany(AllowRecomposition=true)]
        PartCreator<Coffee, ICoffeeMetadata>[] CoffeeMakers { get; set; }

        public void Run()
        {
            foreach (var coffeeMaker in CoffeeMakers)
            {
                Console.WriteLine("Brewing coffee from {0}.", coffeeMaker.Metadata.Origin);
                using (var p = coffeeMaker.CreatePart())
                    Console.WriteLine("{0} ({1})", p.ExportedValue, p.ExportedValue.GetType());
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var catalog = new AggregateCatalog(new AssemblyCatalog(typeof(Program).Assembly));
            var dynamicInstantiationProvider = new DynamicInstantiationExportProvider();
            var container = new CompositionContainer(catalog, dynamicInstantiationProvider);
            dynamicInstantiationProvider.SourceProvider = container;

            var cafe = container.GetExportedValue<Cafe>();
            cafe.Run();

            Console.WriteLine("*****");

            catalog.Catalogs.Add(new TypeCatalog(typeof(Sumatran)));

            cafe.Run();
        }
    }
}
