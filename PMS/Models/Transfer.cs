using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Transfer")]
    public class Transfer
    {
        [Key]
        [StringLength(10)]
        public string TransferID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? FromCustomerID { get; set; }

        [StringLength(10)]
        public string? ToCustomerID { get; set; }

        [StringLength(10)]
        public string? PropertyID { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FromCustomerID")]
        public virtual Customer? FromCustomer { get; set; }

        [ForeignKey("ToCustomerID")]
        public virtual Customer? ToCustomer { get; set; }

        [ForeignKey("PropertyID")]
        public virtual Property? Property { get; set; }
    }
}
