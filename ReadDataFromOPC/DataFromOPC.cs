using Serilog;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da;
using TitaniumAS.Opc.Client.Da.Browsing;
using static TitaniumAS.Opc.Client.Interop.Common.Interop;
using TitaniumAS.Opc.Client.Interop.Common;

namespace ReadDataFromOPC;

public static class DataFromOpc
{
#pragma warning disable S2190 // Recursion should not be infinite
    public static async IAsyncEnumerable<object> ReadDataFromOpc()
#pragma warning restore S2190 // Recursion should not be infinite
    {   
        var logger = new LoggerConfiguration()
            .WriteTo.Console().CreateLogger();

        TitaniumAS.Opc.Client.Bootstrap.Initialize();

        Uri url = UrlBuilder.Build("Matrikon.OPC.Simulation.1");
        using (var server = new OpcDaServer(url))
        {
            await server.ConnectAsync();
            OpcDaGroup group = server.AddGroup("WolkAbout");
            group.IsActive = true;

            //OpcDaItemDefinition[] itemCollection = new OpcDaItemDefinition[1];
            
            //var definition2 = new OpcDaItemDefinition
            //{
            //    ItemId = "Random.String",
            //    IsActive = true,
            //};

            //OpcDaItemDefinition[] itemCollection = new OpcDaItemDefinition[1];
            //itemCollection[0] = new OpcDaItemProperty { ItemName = tagName, MaxAge = -1 };

            //ItemValueResult[] results = _server.Read(itemCollection);

            //OpcDaItemDefinition[] definitions = { definition2 };

            //var opcItem = new OpcDaItem() { ItemId = }
            var maxAge = new List<TimeSpan>() { TimeSpan.FromMinutes(1)};
            var itemIds = new List<string>()
            {
                "Random.Int4"
            };
            //var results = server.Read(itemIds, maxAge);

            //foreach (var item in results)
            //{                
            //    logger.Information($"Item value from OPC:{item.Value}");
            //}


            //var browser = new OpcDaBrowserAuto(server);

            //BrowseChildren(browser);
            //var definition1 = new OpcDaItemDefinition
            //{
            //    ItemId = "Random.Int2",
            //    IsActive = true,
            //};
            //var definition2 = new OpcDaItemDefinition
            //{
            //    ItemId = "Random.String",
            //    IsActive = true,
            //};
            //OpcDaItemDefinition[] definitions = { definition1, definition2 };
            //OpcDaItemResult[] results = group.AddItems(definitions);



            //            OpcDaBrowseElement[] MySimulation = browser.GetElements("Simulation.Functions");
            //            OpcDaItemDefinition[] definitions = new OpcDaItemDefinition[MySimulation.Count()];
            //            for (int i = 0; i < MySimulation.Count(); i++)
            //            {
            //                definitions[i] = new OpcDaItemDefinition { ItemId = MySimulation[i].ItemId, IsActive = true };
            //            }

            //            OpcDaItemResult[] results = group.AddItems(definitions);

            //            foreach (OpcDaItemResult result in results)
            //            {
            //                if (result.Error.Failed)
            //                {
            //                    Console.WriteLine("Error adding items: {0}", result.Error);
            //                }
            //            }
            for (int i = 0; i < 10; i++)
            {

            
                var results = server.Read(itemIds, maxAge);

                foreach (var item in results)
                {
                    logger.Information($"Item value from OPC:{item.Value}");
                    yield return item.Value.ToString();
                }
                //                OpcDaItemValue[] values = await group.ReadAsync(group.Items);
                //                foreach (OpcDaItemValue item in values)
                //                {
                //                    logger.Information($"Item value from OPC:{item.Value}");
                //#pragma warning disable CS8603 // Possible null reference return.
                //                    yield return item.Value.ToString();
                //#pragma warning restore CS8603 // Possible null reference return.
                //                }

                //                Thread.Sleep(3000);
            }
        }
    }
    public static void BrowseChildren(IOpcDaBrowser browser, string itemId = null, int indent = 0)
    {
        // When itemId is null, root elements will be browsed.
        OpcDaBrowseElement[] elements = browser.GetElements(itemId);

        // Output elements.
        foreach (OpcDaBrowseElement element in elements)
        {
            // Output the element.
            Console.Write(new String(' ', indent));
            Console.WriteLine(element);

            // Skip elements without children.
            if (!element.HasChildren)
                continue;

            // Output children of the element.
            BrowseChildren(browser, element.ItemId, indent + 2);
        }
    }
}