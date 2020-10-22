using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineMarket.Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            Count = 1;
        }

        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        //[Range(1,10000, ErrorMessage = "Please enter a value between 1 and 10000")]
        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
