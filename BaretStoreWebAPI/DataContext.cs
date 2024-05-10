using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BaretStoreWebAPI.Models;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;

namespace BaretStoreWebAPI
{
    public class DataContext : IdentityDbContext<User, CustomRole, string,
        IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Ebook> Ebooks { get; set; }
        public DbSet<EmailCustomer> EmailCustomers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(b =>
            {
                b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            });

            builder.Entity<CustomRole>(b =>
            {
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
            });

            builder.Entity<Ebook>()
           .HasIndex(c => new { c.SKU, c.Id })
           .IsUnique(true);
        }
    }
}
