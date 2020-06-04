using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ElectronicPatientCard
{
    public partial class IwmdbContext : DbContext
    {
        public IwmdbContext()
        {
        }

        public IwmdbContext(DbContextOptions<IwmdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Observation> Observation { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=tcp:iwmdb.database.windows.net,1433;Initial Catalog=iwmdb;Persist Security Info=False;User ID=iwmDataBase;Password=udhas@342sdawW;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Observation>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.LastChanged)
                    .HasColumnName("lastChanged")
                    .HasColumnType("date");

                entity.Property(e => e.LastUpdated)
                    .HasColumnName("lastUpdated")
                    .HasColumnType("date");

                entity.Property(e => e.PrimaryId).ValueGeneratedOnAdd();

                entity.Property(e => e.ResourceType)
                    .IsRequired()
                    .HasColumnName("resourceType")
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Text).IsUnicode(false);

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasColumnType("numeric(10, 5)");

                entity.Property(e => e.VersionId).HasColumnName("versionId");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
