using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartGrpc.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        public ShoppingCart()
        {

        }
        public float TotalPrice
        {
            get
            {
                float totalprice = 0;
                foreach (var item in Items)
                {
                    totalprice += item.Price * item.Quantity;
                }
                return totalprice;
            }
        }
    }
}
