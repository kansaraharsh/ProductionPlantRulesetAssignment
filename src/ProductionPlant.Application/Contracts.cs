using ProductionPlant.Domain;

namespace ProductionPlant.Application;

public sealed record EvaluateOrderResponse(
    bool Matched,
    string? ProductionPlant,
    string? MatchedRuleset,
    string? MatchedRule,
    string Reason);

public interface IRulesetRepository
{
    Task<IReadOnlyList<Ruleset>> GetActiveRulesetsAsync(CancellationToken cancellationToken);
}

public interface IEvaluationLogRepository
{
    Task AddAsync(EvaluationAudit audit, CancellationToken cancellationToken);
}

public sealed class EvaluationAudit
{
    public long Id { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public DateTimeOffset EvaluatedAtUtc { get; set; }
    public string InputJson { get; set; } = string.Empty;
    public bool Matched { get; set; }
    public string? ProductionPlant { get; set; }
    public string? MatchedRuleset { get; set; }
    public string? MatchedRule { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ConditionsJson { get; set; } = string.Empty;
}
