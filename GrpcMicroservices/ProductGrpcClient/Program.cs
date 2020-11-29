﻿using Google.Protobuf.WellKnownTypes;
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
            await AddProductAsync(client);
            await UpdateProductAsync(client);
            await DeleteProductAsync(client);
            await InsertBulkProduct(client);

            Console.ReadLine();
        }

        private static async Task InsertBulkProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            // InsertBulkProduct
            Console.WriteLine("InsertBulkProduct started...");
            using var clientBulk = client.InsertBulkProduct();
            for (var i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Name = $"Product{i}",
                    Description = "Bulk inserted product",
                    Price = 399,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };
                await clientBulk.RequestStream.WriteAsync(productModel);
            }
            await clientBulk.RequestStream.CompleteAsync();
            var responseBulk = await clientBulk;
            Console.WriteLine($"Status: {responseBulk.Success}. Insert Count: {responseBulk.InsertCount}");
            Thread.Sleep(1000);
        }

        private static async Task DeleteProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("DeleteProductAsync started...");
            var deleteResponse = await client.DeleteProductAsync(
                new DeleteProductRequest
                { 
                    ProductId = 1
                });
            Console.WriteLine("DeleteProductAsync Response: " + deleteResponse.ToString());
        }

        private static async Task UpdateProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("UpdateProductAsync started...");
            var updateProductResponse = await client.UpdateProductAsync(
                new UpdateProductRequest
                {
                    Product = new ProductModel
                    {
                        ProductId = 1,
                        Name = "Red",
                        Description = "New Red Phone Mi10T",
                        Price = 699,
                        Status = ProductStatus.Instock,
                        CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                });
            Console.WriteLine("UpdateProductAsync Response: " + updateProductResponse.ToString());
        }

        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync started...");
            var addProductResponse = await client.AddProductAsync(
                    new AddProductRequest
                    {
                        Product = new ProductModel
                        {
                            Name = "Red",
                            Description = "New Red Phone Mi10T",
                            Price = 699,
                            Status = ProductStatus.Instock,
                            CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                        }
                    });
            Console.WriteLine("AddProductAsync response: " + addProductResponse.ToString());
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
