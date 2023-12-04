using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedMango_API.Models
{
	public class OrderDetails
	{
		[Key]
        public int OrderDetailId { get; set; }
		[Required]
        public int MenuItemId { get; set; }
        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem { get; set; }
		[Required]
		public int Quantity { get; set; }

		//Sometimes Name or Price gets updated. In that case we don't want to toggle the Price that order was placed with.
		[Required]
		public string ItemName { get; set; }
		[Required]
		public int Price { get; set; }



        [Required]
        public int OrderHeaderId { get; set; }  //We have one "OrderHeader" with many "OrderDeatils"
    }
}
