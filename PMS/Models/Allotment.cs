using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Allotment")]
    public class Allotment
    {
        [Key]
        [StringLength(10)]
        public string AllotmentID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? PropertyID { get; set; }

        [StringLength(10)]
        public string? CustomerID { get; set; }

        [StringLength(10)]
        public string? AllottedBy { get; set; }

        public DateTime AllotmentDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ApprovedBy { get; set; }

        [StringLength(50)]
        public string? AllottmentType { get; set; }

        [StringLength(250)]
        public string? WorkFlowStatus { get; set; }

        public string? Comments { get; set; }

        public string? AdditionalInfo { get; set; }

        // Navigation properties
        [ForeignKey("PropertyID")]
        public virtual Property? Property { get; set; }

        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("AllottedBy")]
        public virtual User? AllottedByUser { get; set; }
    }
}
