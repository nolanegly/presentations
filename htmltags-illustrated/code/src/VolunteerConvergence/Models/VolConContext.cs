using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace VolunteerConvergence.Models
{
    public class VolConContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public VolConContext(DbContextOptions options) : base(options) { }

        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<OrganizationType> OrganizationTypes { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<VolunteerHour> VolunteerHours { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Applicant>().ToTable("Applicant");
            
            modelBuilder.Entity<Volunteer>().ToTable("Volunteer");
            
            modelBuilder.Entity<OrganizationType>().ToTable("OrganizationType");

            modelBuilder.Entity<Organization>().ToTable("Organization")
                .HasOne<OrganizationType>(o => o.OrganizationType).WithMany();

            modelBuilder.Entity<VolunteerHour>().ToTable("VolunteerHour")
                .HasOne<Organization>(vh => vh.Organization).WithMany();

            modelBuilder.Entity<VolunteerHour>()
                .HasOne<Volunteer>(vh => vh.Volunteer).WithMany(v => v.VolunteerHours);

            base.OnModelCreating(modelBuilder);
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction =
                await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync().ConfigureAwait(false);

                _currentTransaction?.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void OverrideOriginalConcurrencyToken(IEntityWithRowVersion entity, byte[] actualOriginalRowVersion)
        {
            // EF.Core silently ignores direct updates to concurrency tokens, presumably to "protect" developers
            // from making a mistake with a field that is set by the database. Unfortunately, it is problematic
            // for detached context scenarios, like, say, a web application.
            // https://github.com/dotnet/efcore/issues/12492
            // https://github.com/dotnet/efcore/issues/18505

            // While this is an understandable design choice, it puts a burden on the calling code to manually
            // check for version changes themselves. Every. Single. Time. Implementing this method allows 
            // data access code to be optimistic in systems when concurrent changes are rare, and accept the
            // rare general exception that might get thrown, rather than being forced to handle a rare situation
            // on every page.
            
            // This line forcibly sets the underlying value of the property, bypassing the protection.
            Entry(entity).Property(p => p.RowVersion).OriginalValue = actualOriginalRowVersion;
        }

    }
}