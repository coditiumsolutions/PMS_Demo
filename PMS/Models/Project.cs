using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Projects")]
    public class Project
    {
        [Key]
        [StringLength(10)]
        public string ProjectID { get; set; } = string.Empty;

        [StringLength(150)]
        public string? ProjectName { get; set; }

        [Required]
        [StringLength(4)]
        public string Prefix { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<PaymentPlan> PaymentPlans { get; set; } = new List<PaymentPlan>();
        public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
        public virtual ICollection<Balloting> Ballotings { get; set; } = new List<Balloting>();
    }
}
