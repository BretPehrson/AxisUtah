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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppLogEntry>()
            .HasIndex(l => l.CreatedAtUtc);

        modelBuilder.Entity<AppLogEntry>()
            .HasIndex(l => l.CorrelationId);

        modelBuilder.Entity<AppLogEntry>()
            .HasIndex(l => l.EventType);

        // Index frequently searched fields for instant queries
        modelBuilder.Entity<Property>()
            .HasIndex(p => p.StandardStatus);

        modelBuilder.Entity<Property>()
            .HasIndex(p => p.City);

        modelBuilder.Entity<Property>()
            .HasIndex(p => p.StructureType);

        modelBuilder.Entity<Property>()
            .HasIndex(p => p.ModificationTimestamp);

        // Relationship configuration
        modelBuilder.Entity<PropertyMedia>()
            .HasOne(m => m.Property)
            .WithMany(p => p.Media)
            .HasForeignKey(m => m.ListingKey)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SyncCheckpoint>()
            .HasKey(s => s.FeedName);
    }
}