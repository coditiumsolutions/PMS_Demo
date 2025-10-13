using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [StringLength(10)]
        public string PaymentID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? ScheduleID { get; set; }

        [StringLength(10)]
        public string? CustomerID { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string? Method { get; set; }

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        [StringLength(250)]
        public string? Status { get; set; }

        [StringLength(255)]
        public string? Remarks { get; set; }

        // Navigation properties
        [ForeignKey("ScheduleID")]
        public virtual PaymentSchedule? PaymentSchedule { get; set; }

        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }
    }
}
