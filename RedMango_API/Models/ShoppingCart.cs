using System.ComponentModel.DataAnnotations.Schema;

namespace RedMango_API.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        
        public ICollection<CartItem> CartItems { get; set; }  // One ShoppingCart can have multiple CartItem

        [NotMapped]  // This Attribute tell us that: This property define in Model but not add a column in database
        public double CartTotal { get; set; }
        [NotMapped]
        public string StripePaymentIntentId { get; set; }
        [NotMapped]
        public string ClientSecret { get; set; }
    }
}
