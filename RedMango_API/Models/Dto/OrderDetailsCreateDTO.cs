using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RedMango_API.Models.Dto
{
	public class OrderDetailsCreateDTO
	{
		[Required]
		public int MenuItemId { get; set; }
		[Required]
		public int Quantity { get; set; }

		//Sometimes Name or Price gets updated. In that case we don't want to toggle the Price that order was placed with.
		[Required]
		public string ItemName { get; set; }
		[Required]
		public double Price { get; set; }
	}
}
