using System.Web;
using HVIP.Models;
using Newtonsoft.Json;

namespace HVIP.Helpers
{
    public static class CartHelper
    {
        private const string CartKey = "HVIP_Cart";

        public static Cart GetCart(HttpSessionStateBase session)
        {
            var json = session[CartKey] as string;
            if (string.IsNullOrEmpty(json)) return new Cart();
            return JsonConvert.DeserializeObject<Cart>(json) ?? new Cart();
        }

        public static void SaveCart(HttpSessionStateBase session, Cart cart)
        {
            session[CartKey] = JsonConvert.SerializeObject(cart);
        }

        public static void AddItem(HttpSessionStateBase session, Product product, int qty = 1)
        {
            var cart = GetCart(session);
            var existing = cart.Items.Find(i => i.ProductId == product.Id);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSize = product.Size,
                    CategoryIcon = product.CategoryIcon,
                    CategoryColor = product.CategoryColor,
                    UnitPrice = product.Price,
                    Quantity = qty
                });
            }
            SaveCart(session, cart);
        }

        public static void RemoveItem(HttpSessionStateBase session, int productId)
        {
            var cart = GetCart(session);
            cart.Items.RemoveAll(i => i.ProductId == productId);
            SaveCart(session, cart);
        }

        public static void UpdateQuantity(HttpSessionStateBase session, int productId, int qty)
        {
            var cart = GetCart(session);
            var item = cart.Items.Find(i => i.ProductId == productId);
            if (item != null)
            {
                if (qty <= 0)
                    cart.Items.Remove(item);
                else
                    item.Quantity = qty;
            }
            SaveCart(session, cart);
        }

        public static void ClearCart(HttpSessionStateBase session)
        {
            session.Remove(CartKey);
        }
    }
}
