using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PMS.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [StringLength(10)]
        public string UserID { get; set; } = string.Empty;

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(256)]
        public string? PasswordHash { get; set; }

        [StringLength(10)]
        public string? RoleID { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RoleID")]
        public virtual ACL? Role { get; set; }

        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
