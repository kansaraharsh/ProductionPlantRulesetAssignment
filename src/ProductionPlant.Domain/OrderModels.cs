namespace ProductionPlant.Domain;

public sealed class Order
{
    public string OrderId { get; set; } = string.Empty;
    public string PublisherNumber { get; set; } = string.Empty;
    public string PublisherName { get; set; } = string.Empty;
    public string OrderMethod { get; set; } = string.Empty;
    public List<Shipment> Shipments { get; set; } = [];
    public List<OrderItem> Items { get; set; } = [];
}

public sealed class Shipment
{
    public ShipTo ShipTo { get; set; } = new();
}

public sealed class ShipTo
{
    public string IsoCountry { get; set; } = string.Empty;
}

public sealed class OrderItem
{
    public string Sku { get; set; } = string.Empty;
    public int PrintQuantity { get; set; }
    public List<Component> Components { get; set; } = [];
}

public sealed class Component
{
    public string Code { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record EvaluationResult(
    bool Matched,
    string? ProductionPlant,
    string? MatchedRuleset,
    string? MatchedRule,
    string Reason,
    IReadOnlyList<ConditionEvaluation> ConditionEvaluations);

public sealed record ConditionEvaluation(
    string Field,
    string Operator,
    string Expected,
    string? Actual,
    bool Matched,
    string Message);
