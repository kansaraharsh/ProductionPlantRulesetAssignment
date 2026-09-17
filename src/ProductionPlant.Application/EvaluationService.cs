using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProductionPlant.Domain;

namespace ProductionPlant.Application;

public sealed class EvaluationService
{
    private readonly IRulesetRepository _rulesets;
    private readonly IEvaluationLogRepository _logs;
    private readonly IRuleEvaluator _evaluator;
    private readonly ILogger<EvaluationService> _logger;

    public EvaluationService(
        IRulesetRepository rulesets,
        IEvaluationLogRepository logs,
        IRuleEvaluator evaluator,
        ILogger<EvaluationService> logger)
    {
        _rulesets = rulesets;
        _logs = logs;
        _evaluator = evaluator;
        _logger = logger;
    }

    public async Task<EvaluateOrderResponse> EvaluateAsync(
        Order order,
        string inputJson,
        CancellationToken cancellationToken)
    {
        var rulesets = await _rulesets.GetActiveRulesetsAsync(cancellationToken);
        var result = _evaluator.Evaluate(order, rulesets);

        var audit = new EvaluationAudit
        {
            OrderId = order.OrderId,
            EvaluatedAtUtc = DateTimeOffset.UtcNow,
            InputJson = inputJson,
            Matched = result.Matched,
            ProductionPlant = result.ProductionPlant,
            MatchedRuleset = result.MatchedRuleset,
            MatchedRule = result.MatchedRule,
            Reason = result.Reason,
            ConditionsJson = JsonSerializer.Serialize(result.ConditionEvaluations)
        };

        await _logs.AddAsync(audit, cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} evaluated. Matched={Matched}, Ruleset={Ruleset}, Rule={Rule}, Plant={Plant}",
            order.OrderId, result.Matched, result.MatchedRuleset, result.MatchedRule, result.ProductionPlant);

        return new EvaluateOrderResponse(
            result.Matched,
            result.ProductionPlant,
            result.MatchedRuleset,
            result.MatchedRule,
            result.Reason);
    }
}
