namespace ProductionPlant.Domain;

public interface IRuleEvaluator
{
    EvaluationResult Evaluate(Order order, IReadOnlyList<Ruleset> rulesets);
}

public sealed class RuleEvaluator : IRuleEvaluator
{
    private readonly IConditionOperator _operator;

    public RuleEvaluator(IConditionOperator @operator)
    {
        _operator = @operator;
    }

    public EvaluationResult Evaluate(Order order, IReadOnlyList<Ruleset> rulesets)
    {
        var allEvaluations = new List<ConditionEvaluation>();

        // First matching ruleset wins.
        foreach (var ruleset in rulesets.Where(x => x.IsActive).OrderBy(x => x.Priority))
        {
            var rulesetEvaluations = ruleset.Conditions
                .OrderBy(x => x.Sequence)
                .Select(c => EvaluateCondition(order, c))
                .ToList();

            allEvaluations.AddRange(rulesetEvaluations);

            if (rulesetEvaluations.Any() && rulesetEvaluations.All(x => x.Matched))
            {
                // First successful rule wins.
                foreach (var rule in ruleset.Rules.Where(x => true).OrderBy(x => x.Priority))
                {
                    var ruleEvaluations = rule.Conditions
                        .OrderBy(x => x.Sequence)
                        .Select(c => EvaluateCondition(order, c))
                        .ToList();

                    allEvaluations.AddRange(ruleEvaluations);

                    if (ruleEvaluations.Any() && ruleEvaluations.All(x => x.Matched))
                    {
                        var reason = string.Join(", ",
                            allEvaluations
                                .Where(x => x.Matched)
                                .Select(x => $"{x.Field}{DisplayOperator(x.Operator)}{x.Expected}"));

                        return new EvaluationResult(
                            true,
                            rule.ProductionPlant,
                            ruleset.Name,
                            rule.Name,
                            reason,
                            allEvaluations);
                    }
                }

                // Matching ruleset but no rule matched: do not evaluate later rulesets.
                return new EvaluationResult(
                    false,
                    null,
                    ruleset.Name,
                    null,
                    "Ruleset matched but no rule conditions were fully satisfied.",
                    allEvaluations);
            }
        }

        return new EvaluationResult(
            false,
            null,
            null,
            null,
            "No ruleset matched the order.",
            allEvaluations);
    }

    private ConditionEvaluation EvaluateCondition(Order order, Condition condition)
    {
        var actual = OrderFieldAccessor.GetValue(order, condition.Field);
        if (actual is null)
        {
            return new ConditionEvaluation(
                condition.Field, condition.Operator, condition.Value, null, false,
                $"Field '{condition.Field}' was not present in the order.");
        }

        var matched = _operator.Compare(actual, condition.Operator, condition.Value);
        return new ConditionEvaluation(
            condition.Field, condition.Operator, condition.Value, actual.ToString(), matched,
            matched
                ? $"Condition satisfied: {condition.Field} {condition.Operator} {condition.Value}."
                : $"Condition failed: actual value '{actual}' does not satisfy {condition.Operator} '{condition.Value}'.");
    }

    private static string DisplayOperator(string op) => op switch
    {
        "Equals" => "=",
        "LessThanOrEqual" => "<=",
        "GreaterThanOrEqual" => ">=",
        _ => op
    };
}

public interface IConditionOperator
{
    bool Compare(object actual, string operatorName, string expected);
}

public sealed class ConditionOperator : IConditionOperator
{
    public bool Compare(object actual, string operatorName, string expected)
    {
        return operatorName switch
        {
            "Equals" => string.Equals(actual.ToString(), expected, StringComparison.OrdinalIgnoreCase),
            "LessThanOrEqual" => Convert.ToDecimal(actual) <= Convert.ToDecimal(expected),
            "GreaterThanOrEqual" => Convert.ToDecimal(actual) >= Convert.ToDecimal(expected),
            _ => throw new InvalidOperationException($"Unsupported operator '{operatorName}'.")
        };
    }
}

public static class OrderFieldAccessor
{
    public static object? GetValue(Order order, string field)
    {
        return field switch
        {
            "OrderId" => order.OrderId,
            "PublisherNumber" => order.PublisherNumber,
            "PublisherName" => order.PublisherName,
            "OrderMethod" => order.OrderMethod,
            "IsCountry" => order.Shipments.FirstOrDefault()?.ShipTo?.IsoCountry,
            "PrintQuantity" => order.Items.FirstOrDefault()?.PrintQuantity,
            "BindTypeCode" => order.Items
                .SelectMany(x => x.Components)
                .SelectMany(x => x.Attributes)
                .FirstOrDefault(x => x.Key.Equals("BindTypeCode", StringComparison.OrdinalIgnoreCase))
                .Value,
            _ => null
        };
    }
}
