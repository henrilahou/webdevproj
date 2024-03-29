using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace project.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal TotalPrice => Items.Sum(item => item.Quantity * item.Product.Price);

        public OrderStatus Status { get; set; } = OrderStatus.InCart; // Default to InCart

        public DateTime OrderPlaced { get; set; } = DateTime.UtcNow;

        public DateTime? OrderUpdated { get; set; }
    }
    public enum OrderStatus
    {
        InCart, // The order is being built as a shopping cart
        Pending, // The order has been submitted but not processed
        Completed, // The order has been completed
        Cancelled // The order has been cancelled
    }
}
