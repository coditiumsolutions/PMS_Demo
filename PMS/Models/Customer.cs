using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        [StringLength(10)]
        public string CustomerID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? RegID { get; set; }

        [StringLength(10)]
        public string? PlanID { get; set; }

        // Personal Info
        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(150)]
        public string? FatherName { get; set; }

        [StringLength(50)]
        public string? CNIC { get; set; }

        [StringLength(50)]
        public string? PassportNo { get; set; }

        public DateTime? DOB { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        // Contact Info
        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? MailingAddress { get; set; }

        [StringLength(255)]
        public string? PermanentAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        // Project Info
        [StringLength(100)]
        public string? SubProject { get; set; }

        [StringLength(50)]
        public string? RegisteredSize { get; set; }

        // System Info
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; } = "Active";

        // Nominee Info
        [StringLength(100)]
        public string? NomineeName { get; set; }

        [StringLength(50)]
        public string? NomineeID { get; set; }

        [StringLength(50)]
        public string? NomineeRelation { get; set; }

        public string? AdditionalInfo { get; set; }

        // Navigation properties
        [ForeignKey("RegID")]
        public virtual Registration? Registration { get; set; }

        [ForeignKey("PlanID")]
        public virtual PaymentPlan? PaymentPlan { get; set; }

        public virtual ICollection<CustomerLog> CustomerLogs { get; set; } = new List<CustomerLog>();
        public virtual ICollection<Allotment> Allotments { get; set; } = new List<Allotment>();
        public virtual ICollection<Possession> Possessions { get; set; } = new List<Possession>();
        public virtual ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
        public virtual ICollection<Waiver> Waivers { get; set; } = new List<Waiver>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public virtual ICollection<Transfer> FromTransfers { get; set; } = new List<Transfer>();
        public virtual ICollection<Transfer> ToTransfers { get; set; } = new List<Transfer>();
        public virtual ICollection<NDC> NDCs { get; set; } = new List<NDC>();
    }
}
