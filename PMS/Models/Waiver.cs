using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Waiver")]
    public class Waiver
    {
        [Key]
        [StringLength(10)]
        public string WaiverID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? CustomerID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string? Reason { get; set; }

        [StringLength(10)]
        public string? ApprovedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }
    }
}
