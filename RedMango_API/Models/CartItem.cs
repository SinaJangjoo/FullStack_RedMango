using System.ComponentModel.DataAnnotations.Schema;

namespace RedMango_API.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int MenuItemId { get; set; }
        [ForeignKey("MenuItemId")]    // Link these two tables (CartItem & MenuItem)
        public MenuItem MenuItem { get; set; } = new();

        public int Quantity { get; set; }
        public int ShoppingCartId { get; set; }  // One ShoppingCart can have multiple cartItem
    }
}
