using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineMarket.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public DateTime ShippingDate { get; set; }

        [Required]
        public double OrderTotalSum { get; set; }

        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public string OrderStatus { get; set; }

        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDueDate { get; set; }

        public string TransactionId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
    }
}
