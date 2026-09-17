using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProductionPlant.Application;
using ProductionPlant.Domain;

namespace ProductionPlant.Infrastructure;

public sealed class RulesetRepository : IRulesetRepository
{
    private const string CacheKey = "active-rulesets-v1";
    private readonly RulesDbContext _db;
    private readonly IMemoryCache _cache;

    public RulesetRepository(RulesDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IReadOnlyList<Ruleset>> GetActiveRulesetsAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyList<Ruleset>? cached) && cached is not null)
            return cached;

        var rulesets = await _db.Rulesets
            .AsNoTracking()
            .Include(x => x.Conditions)
            .Include(x => x.Rules)
                .ThenInclude(x => x.Conditions)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Priority)
            .ToListAsync(cancellationToken);

        _cache.Set(CacheKey, rulesets, TimeSpan.FromMinutes(5));
        return rulesets;
    }
}

public sealed class EvaluationLogRepository : IEvaluationLogRepository
{
    private readonly RulesDbContext _db;

    public EvaluationLogRepository(RulesDbContext db) => _db = db;

    public async Task AddAsync(EvaluationAudit audit, CancellationToken cancellationToken)
    {
        _db.EvaluationAudits.Add(audit);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
