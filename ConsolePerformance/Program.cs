namespace ConsolePerformance
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;

    public class Program
    {
        private const string EndpointUrl = "ENTER YOU COSMOS URL";
        private const string AuthorizationKey = "ENTER YOUR COSMOS KEY";

        private const string DatabaseName = "bulkPerformance";
        private const string ContainerName = "consoleitems";
        private const int AmountToInsert = 50000;

        static async Task Main(string[] args)
        {
            CosmosClient cosmosClient = new CosmosClient(EndpointUrl, AuthorizationKey, new CosmosClientOptions() { AllowBulkExecution = true });

            Console.WriteLine("This tutorial will create a 5000 RU/s container, press any key to continue.");
            Console.ReadKey();

            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(Program.DatabaseName);

            await database.DefineContainer(Program.ContainerName, "/fullName")
                    .WithIndexingPolicy()
                        .WithIndexingMode(IndexingMode.Consistent)
                        .WithIncludedPaths()
                            .Attach()
                        .WithExcludedPaths()
                            .Path("/*")
                            .Attach()
                    .Attach()
                .CreateAsync(5000);

            // </Initialize>

            try
            {
                // Prepare items for insertion
                Console.WriteLine($"Preparing {AmountToInsert} items to insert...");
                // <Operations>
                IReadOnlyCollection<Name> itemsToInsert = Program.GetItemsToInsert(AmountToInsert);
                // </Operations>

                // Create the list of Tasks
                Console.WriteLine($"Starting...");
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(ContainerName);
                List<Task> tasks = new List<Task>(AmountToInsert);
                foreach (var item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemAsync(item, new PartitionKey(item.FullName))
                        .ContinueWith(itemResponse =>
                        {
                            if (!itemResponse.IsCompletedSuccessfully)
                            {
                                AggregateException innerExceptions = itemResponse.Exception.Flatten();
                                if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                                {
                                    Console.WriteLine($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                                }
                                else
                                {
                                    Console.WriteLine($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                                }
                            }
                        }));
                }

                // Wait until all are done
                await Task.WhenAll(tasks);
                // </ConcurrentTasks>
                stopwatch.Stop();

                Console.WriteLine($"Finished in writing {AmountToInsert} items in {stopwatch.Elapsed}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("Cleaning up resources...");
                await database.DeleteAsync();
            }
        }
        // <Bogus>
        private static IReadOnlyCollection<Name> GetItemsToInsert(int AmountToInsert)
        {
            return new Bogus.Faker<Name>()
            .StrictMode(false)
            //Generate item
            .RuleFor(o => o.FirstName, f => f.Name.FirstName())
            .RuleFor(o => o.LastName, f => f.Name.LastName())
            .RuleFor(o => o.Id, f => Guid.NewGuid().ToString())
            .Generate(AmountToInsert);
        }

        public class Name
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("firstName")]
            public string FirstName { get; set; }

            [JsonProperty("lastName")]
            public string LastName { get; set; }

            [JsonProperty("fullName")]
            public string FullName
            {
                get
                {
                    return $"{FirstName} {LastName}";
                }

                private set
                {
                }
            }
        }
    }
}