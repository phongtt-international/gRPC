﻿using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Services
{
    public class ShoppingCartService: ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartContext;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly IMapper _mapper;
        private readonly DiscountService _discountService;

        public ShoppingCartService(ShoppingCartContext shoppingCartContext, ILogger<ShoppingCartService> logger, IMapper mapper, DiscountService discountService)
        {
            _shoppingCartContext = shoppingCartContext ?? throw new ArgumentNullException(nameof(shoppingCartContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(x => x.UserName == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={request.Username} is not found."));
            }
            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }
        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);
            var isExits = await _shoppingCartContext.ShoppingCart.AnyAsync(x => x.UserName == shoppingCart.UserName);
            if (isExits)
            {
                _logger.LogError("Invalid UserName for ShoppingCart creation. UserName : {userName}", shoppingCart.UserName);
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={request.Username} is already exist."));
            }
            _shoppingCartContext.ShoppingCart.Add(shoppingCart);
            await _shoppingCartContext.SaveChangesAsync();

            _logger.LogInformation("ShoppingCart is successfully created.UserName : {userName}", shoppingCart.UserName);

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }
        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream, 
            ServerCallContext context)
        {
            // Get sc if exist or not
            // Check item if exist in sc or not
            // if item exist +1 quantity
            // if not exist add new item into sc
            // check discount and set the item price
            while (await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == requestStream.Current.Username);
                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={requestStream.Current.Username} is not found."));
                }
                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);
                var cartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == newAddedCartItem.ProductId);
                if(null != cartItem)
                {
                    cartItem.Quantity++;
                }else
                {
                    // GRPC CALL DISCOUNT SERVICE -- check discount and set the item price
                    var discount =  await _discountService.GetDiscount(requestStream.Current.DiscountCode);
                    newAddedCartItem.Price -= discount.Amount;
                    shoppingCart.Items.Add(cartItem);
                }
            }
            var insertCount = await _shoppingCartContext.SaveChangesAsync();
            var response = new AddItemIntoShoppingCartResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };
            return response;
        }
        public override async Task<RemoveItemIntoShoppingCartResponse> RemoveItemIntoShoppingCart(RemoveItemIntoShoppingCartRequest request, ServerCallContext context)
        {
            // Get sc if exist or not
            // Check item if exist in sc or not
            // Remove item in SC db

            var shoppingCart = await _shoppingCartContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={request.Username} is not found."));
            }

            var removeCartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == request.RemoveCartItem.ProductId);
            if (null == removeCartItem)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"CartItem with ProductId={request.RemoveCartItem.ProductId} is not found in the ShoppingCart."));
            }

            shoppingCart.Items.Remove(removeCartItem);

            var removeCount = await _shoppingCartContext.SaveChangesAsync();

            var response = new RemoveItemIntoShoppingCartResponse
            {
                Success = removeCount > 0
            };

            return response;
        }

    }
}
