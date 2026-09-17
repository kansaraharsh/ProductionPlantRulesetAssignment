namespace ProductionPlant.Domain;

public sealed class Ruleset
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Condition> Conditions { get; set; } = [];
    public List<Rule> Rules { get; set; } = [];
}

public sealed class Rule
{
    public int Id { get; set; }
    public int RulesetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ProductionPlant { get; set; } = string.Empty;
    public List<Condition> Conditions { get; set; } = [];
}

public sealed class Condition
{
    public int Id { get; set; }
    public int? RulesetId { get; set; }
    public int? RuleId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int Sequence { get; set; }
}
