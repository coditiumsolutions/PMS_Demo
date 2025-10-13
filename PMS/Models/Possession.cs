using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Possession")]
    public class Possession
    {
        [Key]
        [StringLength(10)]
        public string PossessionID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? PropertyID { get; set; }

        [StringLength(10)]
        public string? CustomerID { get; set; }

        public DateTime PossessionDate { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? WorkFlowStatus { get; set; }

        public string? Comments { get; set; }

        [StringLength(255)]
        public string? Remarks { get; set; }

        // Navigation properties
        [ForeignKey("PropertyID")]
        public virtual Property? Property { get; set; }

        [ForeignKey("CustomerID")]
        public virtual Customer? Customer { get; set; }
    }
}
