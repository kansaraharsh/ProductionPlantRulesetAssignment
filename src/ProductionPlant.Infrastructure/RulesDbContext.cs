using Microsoft.EntityFrameworkCore;
using ProductionPlant.Application;
using ProductionPlant.Domain;

namespace ProductionPlant.Infrastructure;

public sealed class RulesDbContext : DbContext
{
    public RulesDbContext(DbContextOptions<RulesDbContext> options) : base(options) { }

    public DbSet<Ruleset> Rulesets => Set<Ruleset>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<Condition> Conditions => Set<Condition>();
    public DbSet<EvaluationAudit> EvaluationAudits => Set<EvaluationAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ruleset>(e =>
        {
            e.ToTable("Rulesets");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.IsActive, x.Priority });
            e.HasMany(x => x.Conditions)
                .WithOne()
                .HasForeignKey(x => x.RulesetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Rules)
            .WithOne()
            .HasForeignKey(x => x.RulesetId)
            .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Rule>(e =>
        {
            e.ToTable("Rules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ProductionPlant).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.RulesetId, x.Priority });
            e.HasMany(x => x.Conditions)
            .WithOne()
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Condition>(e =>
        {
            e.ToTable("Conditions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Field).HasMaxLength(100).IsRequired();
            e.Property(x => x.Operator).HasMaxLength(50).IsRequired();
            e.Property(x => x.Value).HasMaxLength(500).IsRequired();
            e.HasIndex(x => new { x.RulesetId, x.RuleId, x.Sequence });
        });

        modelBuilder.Entity<EvaluationAudit>(e =>
        {
            e.ToTable("EvaluationAudits");
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderId).HasMaxLength(100).IsRequired();
            e.Property(x => x.InputJson).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            e.Property(x => x.ConditionsJson).IsRequired();
            e.HasIndex(x => new { x.OrderId, x.EvaluatedAtUtc });
        });
    }
}
