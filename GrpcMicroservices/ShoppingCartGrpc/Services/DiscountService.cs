using DisCountGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Services
{
    public class DiscountService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoServiceClient;

        public DiscountService(DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient)
        {
            _discountProtoServiceClient = discountProtoServiceClient ?? throw new ArgumentNullException(nameof(discountProtoServiceClient));
        }
        public async Task<DiscountModel> GetDiscount(string discoutnCode)
        {
            var discountRequest = new GetDiscountRequest { DiscountCode = discoutnCode };
            return await _discountProtoServiceClient.GetDiscountAsync(discountRequest);
        }
    }
}
