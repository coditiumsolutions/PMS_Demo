using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace PMS.Models
{
    [Table("Customers")]
    public class Customer : IValidatableObject
    {
        [Key]
        [StringLength(10)]
        public string? CustomerID { get; set; } = string.Empty;

        [StringLength(10)]
        public string? RegID { get; set; }

        [Required]
        [StringLength(10)]
        public string? PlanID { get; set; }

        [Required]
        [StringLength(10)]
        public string? ProjectID { get; set; }

        // Personal Info
        [StringLength(150)]
        [RegularExpression(@"^[a-zA-Z\s\.\-]+$", ErrorMessage = "Full Name must contain letters only (spaces, dots, and hyphens are allowed).")]
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

        [StringLength(100)]
        public string? Nationality { get; set; }

        // Contact Info
        [StringLength(50)]
        [RegularExpression(@"^[0-9\+]+$", ErrorMessage = "Phone must contain digits and '+' only.")]
        public string? Phone { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mailing Address is required.")]
        [StringLength(255)]
        public string? MailingAddress { get; set; }

        [StringLength(255)]
        public string? PermanentAddress { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100)]
        public string? City { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100)]
        public string? Country { get; set; }

        // Project Info
        [Required]
        [StringLength(100)]
        public string? SubProject { get; set; }

        [Required]
        [StringLength(50)]
        public string? RegisteredSize { get; set; }

        // System Info
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? Status { get; set; } = "Active";

        // Nominee Info
        [StringLength(100)]
        public string? NomineeName { get; set; }

        [StringLength(50)]
        public string? NomineeID { get; set; }

        [StringLength(50)]
        public string? NomineeRelation { get; set; }

        [StringLength(255)]
        public string? NomineeNICDocumentPath { get; set; }

        [StringLength(255)]
        public string? NomineePicturePath { get; set; }

        public string? AdditionalInfo { get; set; }

        /// <summary>Optional. Customer can exist without a dealer.</summary>
        public int? DealerID { get; set; }

        /// <summary>1=Yes (use DealerID), 0=No (use DealerName).</summary>
        [Column("isDealerRegistered")]
        public int? IsDealerRegistered { get; set; }

        /// <summary>Manual dealer name when IsDealerRegistered=0.</summary>
        [StringLength(200)]
        public string? DealerName { get; set; }

        // Navigation properties
        [ForeignKey("RegID")]
        public virtual Registration? Registration { get; set; }

        [ForeignKey("DealerID")]
        public virtual Dealer? Dealer { get; set; }

        [ForeignKey("PlanID")]
        public virtual PaymentPlan? PaymentPlan { get; set; }

        [ForeignKey("ProjectID")]
        public virtual Project? Project { get; set; }

        public virtual ICollection<CustomerLog> CustomerLogs { get; set; } = new List<CustomerLog>();
        public virtual ICollection<Allotment> Allotments { get; set; } = new List<Allotment>();
        public virtual ICollection<Possession> Possessions { get; set; } = new List<Possession>();
        public virtual ICollection<Penalty> Penalties { get; set; } = new List<Penalty>();
        public virtual ICollection<Waiver> Waivers { get; set; } = new List<Waiver>();
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
        public virtual ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
        public virtual ICollection<NDC> NDCs { get; set; } = new List<NDC>();

        /// <summary>Either CNIC format 5-7-1 (e.g. 11111-1111111-1) or Passport No with at least 5 characters.</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var cnicTrimmed = CNIC?.Trim() ?? "";
            var passportTrimmed = PassportNo?.Trim() ?? "";
            var cnicValid = Regex.IsMatch(cnicTrimmed, @"^\d{5}-\d{7}-\d$");
            var passportValid = passportTrimmed.Length >= 5;
            if (!cnicValid && !passportValid)
            {
                if (string.IsNullOrEmpty(cnicTrimmed) && string.IsNullOrEmpty(passportTrimmed))
                    yield return new ValidationResult("Either National ID (CNIC) in format XXXXX-XXXXXXX-X or Passport Number (min 5 characters) is required.", new[] { nameof(CNIC), nameof(PassportNo) });
                else if (!string.IsNullOrEmpty(cnicTrimmed) && !cnicValid)
                    yield return new ValidationResult("National ID (CNIC) must be in format XXXXX-XXXXXXX-X (5 digits, hyphen, 7 digits, hyphen, 1 digit). Otherwise provide Passport Number with at least 5 characters.", new[] { nameof(CNIC) });
                else
                    yield return new ValidationResult("Passport Number must be at least 5 characters. Otherwise provide National ID (CNIC) in format XXXXX-XXXXXXX-X.", new[] { nameof(PassportNo) });
            }
            // Date of Birth: must be at least 16 years ago
            if (DOB.HasValue)
            {
                var minDob = DateTime.Today.AddYears(-16);
                if (DOB.Value.Date > minDob)
                    yield return new ValidationResult("Date of Birth must be at least 16 years ago (customer must be 16 or older).", new[] { nameof(DOB) });
            }
            // Dealer: if IsDealerRegistered=1 then DealerID required; if 0 then DealerName required
            if (IsDealerRegistered == 1)
            {
                if (!DealerID.HasValue || DealerID.Value <= 0)
                    yield return new ValidationResult("Please select a dealer from the dropdown when dealer is registered.", new[] { nameof(DealerID) });
            }
            else if (IsDealerRegistered == 0)
            {
                if (string.IsNullOrWhiteSpace(DealerName))
                    yield return new ValidationResult("Please enter the dealer name when dealer is not registered.", new[] { nameof(DealerName) });
            }
        }
    }
}
