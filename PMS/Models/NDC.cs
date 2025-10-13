using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("NDC")]
    public class NDC
    {
        [Key]
        [StringLength(10)]
        public string NDCID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? CustomerID { get; set; }

        [StringLength(100)]
        public string? NDCType { get; set; }

        [StringLength(500)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? WorkFlowStatus { get; set; }

        public string? Comments { get; set; }

        public DateTime IssuedDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Remarks { get; set; }

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }
    }
}
