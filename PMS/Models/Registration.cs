using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Registration")]
    public class Registration
    {
        [Key]
        [StringLength(10)]
        public string RegID { get; set; } = string.Empty;

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(50)]
        public string? CNIC { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
