using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductGrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Waitting for server is running....");
            Thread.Sleep(2000);

            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);
            await GetAllProducts(client);
        }

        private static async Task GetAllProducts(ProductProtoService.ProductProtoServiceClient client)
        {
            //GetAllProductAsync with C#9
            Console.WriteLine("GetAllProductAsync with C#9 started...");
            using var clientData = client.GetAllProducts(new GetAllProductsRequest());
            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(responseData);
            }
            Console.ReadLine();
        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            //GetProductAsync
            Console.WriteLine("GetProductAsync started...");
            var response = await client.GetProductAsync(
                new GetProductRequest
                {
                    ProductId = 1
                });
            Console.WriteLine("GetProductAsync response: " + response.ToString());
        }
    }
}
