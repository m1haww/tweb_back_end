using Microsoft.EntityFrameworkCore;
using Groz_Backend.Models;

namespace Groz_Backend.Data;

/// <summary>
/// Contextul Entity Framework pentru PostgreSQL.
/// Expune tabelele (DbSet) și configurează modelele.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<AppStoreConnectCredential> AppStoreConnectCredentials => Set<AppStoreConnectCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Email unic (nu pot exista doi useri cu același email)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Limită lungime pentru coloane (opțional, dar clar)
        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(256);
        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .HasMaxLength(256);
        modelBuilder.Entity<User>()
            .Property(u => u.Name)
            .HasMaxLength(200);

        // App Store Connect: un user = o setare (UserId unic)
        modelBuilder.Entity<AppStoreConnectCredential>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AppStoreConnectCredential>()
            .HasIndex(c => c.UserId)
            .IsUnique();
        modelBuilder.Entity<AppStoreConnectCredential>()
            .Property(c => c.IssuerId)
            .HasMaxLength(100);
        modelBuilder.Entity<AppStoreConnectCredential>()
            .Property(c => c.KeyId)
            .HasMaxLength(100);
        modelBuilder.Entity<AppStoreConnectCredential>()
            .Property(c => c.PrivateKey)
            .HasMaxLength(2000);
    }
}
