using System.ComponentModel.DataAnnotations;


namespace OnlineMarket.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        [Required(ErrorMessage = "This fild can't be empty")]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
