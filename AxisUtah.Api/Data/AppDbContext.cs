namespace AxisUtah.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppLogEntry> AppLogEntries => Set<AppLogEntry>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Brokerage> Brokerages => Set<Brokerage>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyMedia> PropertyMedia => Set<PropertyMedia>();
    public DbSet<SyncCheckpoint> SyncCheckpoints => Set<SyncCheckpoint>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserInfo> UserInfos => Set<UserInfo>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PropertyHistory> PropertyHistories => Set<PropertyHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // QUERY FILTERS (Soft Delete)
        // =========================
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.IsActive);

        modelBuilder.Entity<UserInfo>()
            .HasQueryFilter(ui => ui.IsActive);

        modelBuilder.Entity<Agent>()
            .HasQueryFilter(a => a.IsActive);

        modelBuilder.Entity<Brokerage>()
            .HasQueryFilter(b => b.IsActive);

        modelBuilder.Entity<Property>()
            .HasQueryFilter(p => p.IsActive);

        modelBuilder.Entity<PropertyMedia>()
            .HasQueryFilter(pm => pm.Property!.IsActive);
        
        modelBuilder.Entity<PropertyHistory>()
            .HasQueryFilter(ph => ph.Property!.IsActive);

        // ====================
        // UNIQUE CONSTRAINTS
        // ====================
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        modelBuilder.Entity<UserInfo>()
            .HasIndex(ui => ui.UserId)
            .IsUnique();

        // ========================
        // FOREIGN KEY RELATIONSHIPS
        // ========================

        // User -> UserInfo (1:1 relationship)
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserInfo)
            .WithOne(ui => ui.User)
            .HasForeignKey<UserInfo>(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> RefreshToken (1:Many relationship)
        modelBuilder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Property -> Address
        modelBuilder.Entity<Property>()
            .HasOne(p => p.Address)
            .WithMany()
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        // Property -> Agent
        modelBuilder.Entity<Property>()
            .HasOne(p => p.Agent)
            .WithMany()
            .HasForeignKey(p => p.AgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Property -> Brokerage
        modelBuilder.Entity<Property>()
            .HasOne(p => p.Brokerage)
            .WithMany()
            .HasForeignKey(p => p.BrokerageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Property -> User
        modelBuilder.Entity<Property>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // PropertyMedia -> Property (1:Many relationship)
        modelBuilder.Entity<PropertyMedia>()
            .HasOne(m => m.Property)
            .WithMany(p => p.Media)
            .HasForeignKey(m => m.ListingKey)
            .OnDelete(DeleteBehavior.Restrict);

        // Agent -> Brokerage
        modelBuilder.Entity<Agent>()
            .HasOne(a => a.Brokerage)
            .WithMany()
            .HasForeignKey(a => a.BrokerageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Lead -> Property
        modelBuilder.Entity<Lead>()
            .HasOne(l => l.Property)
            .WithMany()
            .HasForeignKey(l => l.PropertyListingKey)
            .OnDelete(DeleteBehavior.SetNull);

        // PropertyHistory -> Property
        modelBuilder.Entity<PropertyHistory>()
            .HasOne(ph => ph.Property)
            .WithMany()
            .HasForeignKey(ph => ph.ListingKey)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================
        // INDEXES - PROPERTY
        // ====================
        modelBuilder.Entity<Property>()
            .HasIndex(p => new { p.City, p.StandardStatus });

        modelBuilder.Entity<Property>()
            .HasIndex(p => new { p.AgentId, p.StandardStatus });

        modelBuilder.Entity<Property>()
            .HasIndex(p => new { p.BrokerageId, p.StandardStatus });

        modelBuilder.Entity<Property>()
            .HasIndex(p => p.ModificationTimestamp)
            .IsDescending();

        modelBuilder.Entity<Property>()
            .HasIndex(p => p.ListingKey);

        // ====================
        // INDEXES - PROPERTY HISTORY
        // ====================
        modelBuilder.Entity<PropertyHistory>()
            .HasIndex(ph => ph.ListingKey);

        modelBuilder.Entity<PropertyHistory>()
            .HasIndex(ph => ph.ChangedAtUtc)
            .IsDescending();

        modelBuilder.Entity<PropertyHistory>()
            .HasIndex(ph => new { ph.ListingKey, ph.ChangedAtUtc })
            .IsDescending(false, true);

        modelBuilder.Entity<PropertyHistory>()
            .HasIndex(ph => ph.CorrelationId);

        // ====================
        // INDEXES - PROPERTY MEDIA
        // ====================
        modelBuilder.Entity<PropertyMedia>()
            .HasIndex(m => new { m.ListingKey, m.Order });

        // ====================
        // INDEXES - LEAD
        // ====================
        modelBuilder.Entity<Lead>()
            .HasIndex(l => new { l.PropertyListingKey, l.CreatedAt })
            .IsDescending(false, true);

        modelBuilder.Entity<Lead>()
            .HasIndex(l => l.Email);

        // ====================
        // INDEXES - ADDRESS
        // ====================
        modelBuilder.Entity<Address>()
            .HasIndex(a => new { a.City, a.PostalCode });

        // ====================
        // INDEXES - AGENT
        // ====================
        modelBuilder.Entity<Agent>()
            .HasIndex(a => new { a.BrokerageId, a.IsActive });

        modelBuilder.Entity<Agent>()
            .HasIndex(a => a.Email);

        modelBuilder.Entity<Agent>()
            .HasIndex(a => new { a.LastName, a.FirstName });

        // ====================
        // INDEXES - BROKERAGE
        // ====================
        modelBuilder.Entity<Brokerage>()
            .HasIndex(b => b.IsActive);

        modelBuilder.Entity<Brokerage>()
            .HasIndex(b => new { b.City, b.IsActive });

        modelBuilder.Entity<Brokerage>()
            .HasIndex(b => b.Email);

        // ====================
        // INDEXES - USER
        // ====================
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Email, u.IsActive });

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.IsActive, u.CreatedAt })
            .IsDescending(false, true);

        // ====================
        // INDEXES - USER INFO
        // ====================
        modelBuilder.Entity<UserInfo>()
            .HasIndex(ui => new { ui.UserId, ui.IsActive });

        // ====================
        // INDEXES - REFRESH TOKEN
        // ====================
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.UserId, rt.Expires });

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Expires);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.UserId, rt.Expires })
            .IsDescending(false, true);

        // ====================
        // INDEXES - APP LOG ENTRY
        // ====================
        modelBuilder.Entity<AppLogEntry>()
            .HasIndex(l => l.CreatedAtUtc)
            .IsDescending();

        modelBuilder.Entity<AppLogEntry>()
            .HasIndex(l => l.CorrelationId);

        modelBuilder.Entity<AppLogEntry>()
            .HasIndex(l => l.EventType);

        modelBuilder.Entity<AppLogEntry>()
            .HasIndex(l => new { l.Level, l.CreatedAtUtc })
            .IsDescending(false, true);

        // ====================
        // SPECIAL CONFIGURATIONS
        // ====================
        modelBuilder.Entity<SyncCheckpoint>()
            .HasKey(s => s.FeedName);

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Ignore(r => r.IsExpired);
            entity.Ignore(r => r.IsActive);
        });
    }
}