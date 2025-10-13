using Microsoft.EntityFrameworkCore;
using PMS.Models;

namespace PMS.Data
{
    public class PMSDbContext : DbContext
    {
        public PMSDbContext(DbContextOptions<PMSDbContext> options) : base(options)
        {
        }

        // Users & Access Control
        public DbSet<ACL> ACLs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Registration & Customers
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<PaymentPlan> PaymentPlans { get; set; }
        public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerLog> CustomerLogs { get; set; }

        // Projects & Properties
        public DbSet<Project> Projects { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Allotment> Allotments { get; set; }
        public DbSet<Balloting> Ballotings { get; set; }
        public DbSet<Possession> Possessions { get; set; }

        // Payment Management
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Penalty> Penalties { get; set; }
        public DbSet<Waiver> Waivers { get; set; }
        public DbSet<Refund> Refunds { get; set; }

        // Transfer & NDC
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<NDC> NDCs { get; set; }

        // Approvals & Workflow
        public DbSet<Approval> Approvals { get; set; }

        // Attachments & Media
        public DbSet<Attachment> Attachments { get; set; }

        // Configuration
        public DbSet<Configuration> Configurations { get; set; }

        // Sales & Leads
        public DbSet<PropertyInquiry> PropertyInquiries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ACL
            modelBuilder.Entity<ACL>(entity =>
            {
                entity.HasKey(e => e.RoleID);
                entity.Property(e => e.RoleID).HasMaxLength(10);
                entity.Property(e => e.RoleName).HasMaxLength(100);
            });

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserID).HasMaxLength(10);
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.PasswordHash).HasMaxLength(256);
                entity.Property(e => e.RoleID).HasMaxLength(10);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure UserSession
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.SessionID);
                entity.Property(e => e.SessionID).HasMaxLength(10);
                entity.Property(e => e.UserID).HasMaxLength(10);
                entity.Property(e => e.IPAddress).HasMaxLength(50);
                entity.Property(e => e.DeviceInfo).HasMaxLength(150);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserSessions)
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Registration
            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => e.RegID);
                entity.Property(e => e.RegID).HasMaxLength(10);
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.CNIC).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.Status).HasMaxLength(50);
            });

            // Configure PaymentPlan
            modelBuilder.Entity<PaymentPlan>(entity =>
            {
                entity.HasKey(e => e.PlanID);
                entity.Property(e => e.PlanID).HasMaxLength(10);
                entity.Property(e => e.ProjectID).HasMaxLength(10);
                entity.Property(e => e.PlanName).HasMaxLength(150);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Frequency).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.HasOne(e => e.Project)
                      .WithMany(p => p.PaymentPlans)
                      .HasForeignKey(e => e.ProjectID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure PaymentSchedule
            modelBuilder.Entity<PaymentSchedule>(entity =>
            {
                entity.HasKey(e => e.ScheduleID);
                entity.Property(e => e.ScheduleID).HasMaxLength(10);
                entity.Property(e => e.PlanID).HasMaxLength(10);
                entity.Property(e => e.PaymentDescription).HasMaxLength(250);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SurchargeRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.HasOne(e => e.PaymentPlan)
                      .WithMany(p => p.PaymentSchedules)
                      .HasForeignKey(e => e.PlanID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerID);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.RegID).HasMaxLength(10);
                entity.Property(e => e.PlanID).HasMaxLength(10);
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.FatherName).HasMaxLength(150);
                entity.Property(e => e.CNIC).HasMaxLength(50);
                entity.Property(e => e.PassportNo).HasMaxLength(50);
                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.MailingAddress).HasMaxLength(255);
                entity.Property(e => e.PermanentAddress).HasMaxLength(255);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.SubProject).HasMaxLength(100);
                entity.Property(e => e.RegisteredSize).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.NomineeName).HasMaxLength(100);
                entity.Property(e => e.NomineeID).HasMaxLength(50);
                entity.Property(e => e.NomineeRelation).HasMaxLength(50);

                entity.HasOne(e => e.Registration)
                      .WithMany(r => r.Customers)
                      .HasForeignKey(e => e.RegID)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.PaymentPlan)
                      .WithMany(p => p.Customers)
                      .HasForeignKey(e => e.PlanID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure CustomerLog
            modelBuilder.Entity<CustomerLog>(entity =>
            {
                entity.HasKey(e => e.LogID);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.Action).HasMaxLength(150);
                entity.Property(e => e.Remarks).HasMaxLength(255);
                entity.Property(e => e.CreatedBy).HasMaxLength(10);
                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.CustomerLogs)
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Project
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.ProjectID);
                entity.Property(e => e.ProjectID).HasMaxLength(10);
                entity.Property(e => e.ProjectName).HasMaxLength(150);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(150);
            });

            // Configure Property
            modelBuilder.Entity<Property>(entity =>
            {
                entity.HasKey(e => e.PropertyID);
                entity.Property(e => e.PropertyID).HasMaxLength(10);
                entity.Property(e => e.ProjectID).HasMaxLength(10);
                entity.Property(e => e.PlotNo).HasMaxLength(50);
                entity.Property(e => e.Street).HasMaxLength(50);
                entity.Property(e => e.PlotType).HasMaxLength(50);
                entity.Property(e => e.Block).HasMaxLength(50);
                entity.Property(e => e.PropertyType).HasMaxLength(50);
                entity.Property(e => e.Size).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Properties)
                      .HasForeignKey(e => e.ProjectID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Allotment
            modelBuilder.Entity<Allotment>(entity =>
            {
                entity.HasKey(e => e.AllotmentID);
                entity.Property(e => e.AllotmentID).HasMaxLength(10);
                entity.Property(e => e.PropertyID).HasMaxLength(10);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.AllottedBy).HasMaxLength(10);
                entity.Property(e => e.ApprovedBy).HasMaxLength(50);
                entity.Property(e => e.AllottmentType).HasMaxLength(50);
                entity.Property(e => e.WorkFlowStatus).HasMaxLength(250);

                entity.HasOne(e => e.Property)
                      .WithMany(p => p.Allotments)
                      .HasForeignKey(e => e.PropertyID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Allotments)
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AllottedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.AllottedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Balloting
            modelBuilder.Entity<Balloting>(entity =>
            {
                entity.HasKey(e => e.BallotID);
                entity.Property(e => e.BallotID).HasMaxLength(10);
                entity.Property(e => e.ProjectID).HasMaxLength(10);
                entity.Property(e => e.ConductedBy).HasMaxLength(10);
                entity.Property(e => e.Remarks).HasMaxLength(255);

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Ballotings)
                      .HasForeignKey(e => e.ProjectID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ConductedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ConductedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Possession
            modelBuilder.Entity<Possession>(entity =>
            {
                entity.HasKey(e => e.PossessionID);
                entity.Property(e => e.PossessionID).HasMaxLength(10);
                entity.Property(e => e.PropertyID).HasMaxLength(10);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.WorkFlowStatus).HasMaxLength(250);
                entity.Property(e => e.Remarks).HasMaxLength(255);

                entity.HasOne(e => e.Property)
                      .WithMany(p => p.Possessions)
                      .HasForeignKey(e => e.PropertyID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Possessions)
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentID);
                entity.Property(e => e.PaymentID).HasMaxLength(10);
                entity.Property(e => e.ScheduleID).HasMaxLength(10);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Method).HasMaxLength(50);
                entity.Property(e => e.ReferenceNo).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(250);
                entity.Property(e => e.Remarks).HasMaxLength(255);

                entity.HasOne(e => e.PaymentSchedule)
                      .WithMany(p => p.Payments)
                      .HasForeignKey(e => e.ScheduleID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Penalty
            modelBuilder.Entity<Penalty>(entity =>
            {
                entity.HasKey(e => e.PenaltyID);
                entity.Property(e => e.PenaltyID).HasMaxLength(10);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Reason).HasMaxLength(255);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Penalties)
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Waiver
            modelBuilder.Entity<Waiver>(entity =>
            {
                entity.HasKey(e => e.WaiverID);
                entity.Property(e => e.WaiverID).HasMaxLength(10);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Reason).HasMaxLength(255);
                entity.Property(e => e.ApprovedBy).HasMaxLength(10);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Waivers)
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Refund
            modelBuilder.Entity<Refund>(entity =>
            {
                entity.HasKey(e => e.RefundID);
                entity.Property(e => e.RefundID).HasMaxLength(10);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Reason).HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.ApprovedBy).HasMaxLength(10);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Refunds)
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Transfer
            modelBuilder.Entity<Transfer>(entity =>
            {
                entity.HasKey(e => e.TransferID);
                entity.Property(e => e.TransferID).HasMaxLength(10);
                entity.Property(e => e.FromCustomerID).HasMaxLength(10);
                entity.Property(e => e.ToCustomerID).HasMaxLength(10);
                entity.Property(e => e.PropertyID).HasMaxLength(10);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(e => e.FromCustomer)
                      .WithMany(c => c.FromTransfers)
                      .HasForeignKey(e => e.FromCustomerID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ToCustomer)
                      .WithMany(c => c.ToTransfers)
                      .HasForeignKey(e => e.ToCustomerID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Property)
                      .WithMany(p => p.Transfers)
                      .HasForeignKey(e => e.PropertyID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure NDC
            modelBuilder.Entity<NDC>(entity =>
            {
                entity.HasKey(e => e.NDCID);
                entity.Property(e => e.NDCID).HasMaxLength(10);
                entity.Property(e => e.CustomerID).HasMaxLength(10);
                entity.Property(e => e.NDCType).HasMaxLength(100);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.WorkFlowStatus).HasMaxLength(500);
                entity.Property(e => e.Remarks).HasMaxLength(255);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.NDCs)
                      .HasForeignKey(e => e.CustomerID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Approval
            modelBuilder.Entity<Approval>(entity =>
            {
                entity.HasKey(e => e.ApprovalID);
                entity.Property(e => e.ApprovalID).HasMaxLength(10);
                entity.Property(e => e.RefType).HasMaxLength(50);
                entity.Property(e => e.RefID).HasMaxLength(10);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Remarks).HasMaxLength(255);
            });

            // Configure Attachment
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(e => e.AttachmentID);
                entity.Property(e => e.AttachmentID).HasMaxLength(10);
                entity.Property(e => e.RefType).HasMaxLength(50);
                entity.Property(e => e.RefID).HasMaxLength(10);
                entity.Property(e => e.FilePath).HasMaxLength(255);
                entity.Property(e => e.UploadedBy).HasMaxLength(10);

                entity.HasOne(e => e.UploadedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ActivityLog
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.LogID);
                entity.Property(e => e.UserID).HasMaxLength(10);
                entity.Property(e => e.Action).HasMaxLength(255);
                entity.Property(e => e.RefType).HasMaxLength(50);
                entity.Property(e => e.RefID).HasMaxLength(10);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.ActivityLogs)
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationID);
                entity.Property(e => e.NotificationID).HasMaxLength(10);
                entity.Property(e => e.UserID).HasMaxLength(10);
                entity.Property(e => e.Message).HasMaxLength(255);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Configuration
            modelBuilder.Entity<Configuration>(entity =>
            {
                entity.HasKey(e => e.ConfigKey);
                entity.Property(e => e.ConfigKey).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.UpdatedBy).HasMaxLength(10);

                entity.HasOne(e => e.UpdatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UpdatedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure PropertyInquiry
            modelBuilder.Entity<PropertyInquiry>(entity =>
            {
                entity.HasKey(e => e.InquiryID);
                entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.EmailAddress).HasMaxLength(150);
                entity.Property(e => e.InquiryType).HasMaxLength(100);
                entity.Property(e => e.IPAddress).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("New");
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.IsContacted).HasDefaultValue(false);
            });
        }
    }
}
