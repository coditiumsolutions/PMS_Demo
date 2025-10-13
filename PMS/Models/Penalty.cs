using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Penalties")]
    public class Penalty
    {
        [Key]
        [StringLength(10)]
        public string PenaltyID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? CustomerID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string? Reason { get; set; }

        public DateTime AppliedOn { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }
    }
}
