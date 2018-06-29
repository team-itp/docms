using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Docms.Web.VisualizationSystem.Data
{
    public partial class VisualizationSystemDBContext : DbContext
    {
        public VisualizationSystemDBContext()
        {
        }

        public VisualizationSystemDBContext(DbContextOptions<VisualizationSystemDBContext> options)
            : base(options)
        {
            Database.EnsureCreated();

            if (!Teams.Any())
            {
                var team1 = new Teams()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "営業１課",
                    Department = 1,
                };
                Teams.Add(team1);
                Users.Add(new Users()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test User",
                    AccountName = "testuser",
                    Password = "Passw0rd",
                    Rank = 1,
                    Department = team1.Department,
                    TeamId = team1.Id,
                    Theme = 0,
                    Obsolete = false,
                });
                SaveChanges();

            }
        }

        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Costs> Costs { get; set; }
        public virtual DbSet<CustomerProcesses> CustomerProcesses { get; set; }
        public virtual DbSet<Customers> Customers { get; set; }
        public virtual DbSet<Expenses> Expenses { get; set; }
        public virtual DbSet<Histories> Histories { get; set; }
        public virtual DbSet<Inquiries> Inquiries { get; set; }
        public virtual DbSet<Invoices> Invoices { get; set; }
        public virtual DbSet<IssuedQuotations> IssuedQuotations { get; set; }
        public virtual DbSet<LaborCosts> LaborCosts { get; set; }
        public virtual DbSet<Materials> Materials { get; set; }
        public virtual DbSet<PaymentDetailsObsolete> PaymentDetailsObsolete { get; set; }
        public virtual DbSet<Payments> Payments { get; set; }
        public virtual DbSet<ProcessManagements> ProcessManagements { get; set; }
        public virtual DbSet<PurchaseOrders> PurchaseOrders { get; set; }
        public virtual DbSet<QuotationDetails> QuotationDetails { get; set; }
        public virtual DbSet<Quotations> Quotations { get; set; }
        public virtual DbSet<SalesGoals> SalesGoals { get; set; }
        public virtual DbSet<SalesOrders> SalesOrders { get; set; }
        public virtual DbSet<Suppliers> Suppliers { get; set; }
        public virtual DbSet<TaxRatio> TaxRatios { get; set; }
        public virtual DbSet<Teams> Teams { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => new { e.Name });
            });

            modelBuilder.Entity<Costs>(entity =>
            {
                entity.HasKey(e => new { e.SalesOrderNo, e.No });

                entity.Property(e => e.SalesOrderNo)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Remark).HasColumnName("Remark ");
            });

            modelBuilder.Entity<CustomerProcesses>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CustomerId, e.FiscalYear, e.Term });

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Customers>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Fax1)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Fax2)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Fax3)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.PostalCode1)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.PostalCode2)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).IsUnicode(false);

                entity.Property(e => e.Tel1)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Tel2)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Tel3)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.UserIdOfPersonInCharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Expenses>(entity =>
            {
                entity.HasKey(e => e.SalesOrderNo);

                entity.Property(e => e.SalesOrderNo)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Supplies).HasColumnName("Supplies ");
            });

            modelBuilder.Entity<Histories>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.DateTime).HasColumnType("datetime");

                entity.Property(e => e.Operation).IsRequired();

                entity.Property(e => e.Reason).IsRequired();

                entity.Property(e => e.UserName).IsRequired();
            });

            modelBuilder.Entity<Inquiries>(entity =>
            {
                entity.HasKey(e => e.SalesOrderNo);

                entity.Property(e => e.SalesOrderNo)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.FollowDate).HasColumnType("date");

                entity.Property(e => e.InquiryDate).HasColumnType("date");

                entity.Property(e => e.LostDate).HasColumnType("date");

                entity.Property(e => e.MeetingDate).HasColumnType("date");

                entity.Property(e => e.QuotationDate).HasColumnType("date");

                entity.Property(e => e.RefollowDate).HasColumnType("date");

                entity.Property(e => e.SalesAmountFixedDate).HasColumnType("date");

                entity.Property(e => e.SuccessDate).HasColumnType("date");
            });

            modelBuilder.Entity<Invoices>(entity =>
            {
                entity.HasKey(e => e.No);

                entity.Property(e => e.No)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.InvoiceDate).HasColumnType("date");

                entity.Property(e => e.ReceiptDate).HasColumnType("date");

                entity.Property(e => e.SalesOrderNo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ScheduledInvoiceDate).HasColumnType("date");

                entity.Property(e => e.ScheduledReceiptDate).HasColumnType("date");
            });

            modelBuilder.Entity<IssuedQuotations>(entity =>
            {
                entity.HasKey(e => e.No);

                entity.Property(e => e.No)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.IssueDate).HasColumnType("date");

                entity.Property(e => e.LimitedDate).HasColumnType("date");

                entity.Property(e => e.SalesOrderNo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LaborCosts>(entity =>
            {
                entity.HasKey(e => new { e.SalesOrderNo, e.No });

                entity.Property(e => e.SalesOrderNo)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Remark).HasColumnName("Remark ");
            });

            modelBuilder.Entity<Materials>(entity =>
            {
                entity.HasKey(e => new { e.PurchaseOrderNo, e.No });

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PaymentDetailsObsolete>(entity =>
            {
                entity.HasKey(e => new { e.PurchaseOrderNo, e.No });

                entity.ToTable("PaymentDetails_obsolete");

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Payments>(entity =>
            {
                entity.HasKey(e => new { e.PurchaseOrderNo, e.No });

                entity.Property(e => e.PurchaseOrderNo)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.InvoiceDate).HasColumnType("date");

                entity.Property(e => e.PaymentDate).HasColumnType("date");

                entity.Property(e => e.ScheduledPaymentDate).HasColumnType("date");
            });

            modelBuilder.Entity<ProcessManagements>(entity =>
            {
                entity.HasKey(e => e.SalesOrderNo);

                entity.Property(e => e.SalesOrderNo)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<PurchaseOrders>(entity =>
            {
                entity.HasKey(e => e.No);

                entity.Property(e => e.No)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.EndDate2).HasColumnType("date");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.PaymentDate).HasColumnType("date");

                entity.Property(e => e.SalesOrderNo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.SupplierId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxRatio).HasDefaultValueSql("((8))");
            });

            modelBuilder.Entity<QuotationDetails>(entity =>
            {
                entity.HasKey(e => new { e.QuotationNo, e.No });

                entity.Property(e => e.QuotationNo)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Unit).HasMaxLength(5);
            });

            modelBuilder.Entity<Quotations>(entity =>
            {
                entity.HasKey(e => e.No);

                entity.Property(e => e.No)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.IssueDate).HasColumnType("date");

                entity.Property(e => e.LimitedDate).HasColumnType("date");

                entity.Property(e => e.SalesOrderNo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SalesGoals>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CustomerId });

                entity.Property(e => e.UserId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SalesOrders>(entity =>
            {
                entity.HasKey(e => e.No);

                entity.Property(e => e.No)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.CustomerId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName).IsRequired();

                entity.Property(e => e.DepartmentName).IsRequired();

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.EndDate2).HasColumnType("date");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.RelatedNo)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ScheduledInvoiceDate).HasColumnType("date");

                entity.Property(e => e.ScheduledQuotationDate).HasColumnType("date");

                entity.Property(e => e.ScheduledReceiptDate).HasColumnType("date");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.StartDate2).HasColumnType("date");

                entity.Property(e => e.TaxRatio).HasDefaultValueSql("((8))");

                entity.Property(e => e.UserIdOfPersonInCharge)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.WorkType).IsRequired();
            });

            modelBuilder.Entity<Suppliers>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.Fax1)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Fax2)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Fax3)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.PostalCode1)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.PostalCode2)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Tel1)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Tel2)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Tel3)
                    .HasMaxLength(5)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TaxRatio>(entity =>
            {
                entity.HasKey(e => new { e.From, e.To });
                entity.Property(e => e.Ratio)
                    .HasColumnName("TaxRatio");
            });

            modelBuilder.Entity<Teams>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AccountName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TeamId).IsUnicode(false);
            });
        }
    }
}
