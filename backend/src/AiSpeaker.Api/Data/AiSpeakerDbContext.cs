using AiSpeaker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiSpeaker.Api.Data;

public sealed class AiSpeakerDbContext : DbContext
{
    public AiSpeakerDbContext(DbContextOptions<AiSpeakerDbContext> options) : base(options)
    {
    }

    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();
    public DbSet<ConversationSessionEntity> ConversationSessions => Set<ConversationSessionEntity>();
    public DbSet<ConversationMessageEntity> ConversationMessages => Set<ConversationMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceEntity>(builder =>
        {
            builder.ToTable("Devices");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.DeviceCode).IsRequired();
            builder.Property(d => d.DeviceName).IsRequired();
            builder.Property(d => d.SecretKey).IsRequired();
            builder.HasIndex(d => d.DeviceCode).IsUnique();
        });

        modelBuilder.Entity<ConversationSessionEntity>(builder =>
        {
            builder.ToTable("ConversationSessions");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.SessionId).IsRequired();
            builder.HasIndex(s => s.SessionId).IsUnique();
            builder.HasOne(s => s.Device)
                .WithMany(d => d.Sessions)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConversationMessageEntity>(builder =>
        {
            builder.ToTable("ConversationMessages");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Role).IsRequired();
            builder.Property(m => m.Content).IsRequired();
            builder.HasOne(m => m.ConversationSession)
                .WithMany(s => s.Messages)
                .HasForeignKey(m => m.ConversationSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
