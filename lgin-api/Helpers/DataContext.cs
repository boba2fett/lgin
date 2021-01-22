using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApi.Entities;

namespace WebApi.Helpers
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlite(Configuration.GetConnectionString("WebApiDatabase"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<UserGroup>()
            //     .HasKey(ug => new { ug.UserId, ug.GroupId});

            // modelBuilder.Entity<UserGroup>()
            //     .HasOne(ug => ug.User)
            //     .WithMany(u => u.UserGroup)
            //     .HasForeignKey(ug => ug.GroupId);

            // modelBuilder.Entity<UserGroup>()
            //     .HasOne(ug => ug.Group)
            //     .WithMany(g => g.UserGroup)
            //     .HasForeignKey(ug => ug.UserId);

            modelBuilder.Entity<Group>()
            .HasMany(g => g.Users)
            .WithMany(u => u.Groups)
            .UsingEntity<UserGroup>(
                j => j
                    .HasOne(ug => ug.User)
                    .WithMany(u => u.UserGroup)
                    .HasForeignKey(ug => ug.UserId),
                j => j
                    .HasOne(ug => ug.Group)
                    .WithMany(g => g.UserGroup)
                    .HasForeignKey(ug => ug.GroupId),
                j =>
                {
                    j.Property(ug => ug.MemberSince).HasDefaultValueSql("CURRENT_TIMESTAMP");
                    j.HasKey(t => new { t.UserId, t.GroupId });
                });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroup { get; set; }
    }
}