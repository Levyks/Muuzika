using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Muuzika.Gateway.Entities;

namespace Muuzika.Gateway.Database;

public class MuuzikaDbContext: DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public MuuzikaDbContext(DbContextOptions<MuuzikaDbContext> options, IHttpContextAccessor httpContextAccessor) 
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public DbSet<AuthenticatableEntity> Authenticatables { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<ServerEntity> Servers { get; set; } = null!;
    public DbSet<RoomEntity> Rooms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<RoomEntity>()
            .HasIndex(r => r.Code)
            .IsUnique()
            .HasFilter("not finished");
    }

    private void UpdateBaseEntityProperties()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e is { Entity: BaseLogEntity, State: EntityState.Added or EntityState.Modified });

        var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? userId = userIdString != null && int.TryParse(userIdString, out var id) ? id : null;

        foreach (var entry in entries)
        {
            var entity = (BaseLogEntity) entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedById = userId;
            if (entry.State != EntityState.Added) continue;
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedById = userId;
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