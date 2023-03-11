using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Entities;

namespace Muuzika.Gateway.Contexts;

public class MuuzikaDbContext: DbContext
{
    public MuuzikaDbContext(DbContextOptions<MuuzikaDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<AuthenticatableEntity> Authenticatables { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ServerEntity> Servers { get; set; }
    
    private void UpdateBaseEntityProperties()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: BaseEntity, State: EntityState.Added or EntityState.Modified });
        
        foreach (var entry in entries)
        {
            var entity = (BaseEntity) entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
                entity.CreatedAt = DateTime.UtcNow;
        }
    }
    
    public override int SaveChanges()
    {
        UpdateBaseEntityProperties();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateBaseEntityProperties();
        return base.SaveChangesAsync(cancellationToken);
    }
}