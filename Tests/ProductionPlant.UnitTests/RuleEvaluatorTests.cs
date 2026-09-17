using ProductionPlant.Domain;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProductionPlant.UnitTests;

public sealed class RuleEvaluatorTests
{
    private readonly RuleEvaluator _sut = new(new ConditionOperator());

    private static Ruleset RulesetWith(params Rule[] rules) => new()
    {
        Id = 1,
        Name = "Ruleset Two",
        Priority = 1,
        IsActive = true,
        Conditions =
        [
            new() { Field = "PublisherNumber", Operator = "Equals", Value = "99999", Sequence = 1 },
            new() { Field = "OrderMethod", Operator = "Equals", Value = "POD", Sequence = 2 }
        ],
        Rules = rules.ToList()
    };

    private static Rule Rule(string name, int priority, string plant, params Condition[] conditions) => new()
    {
        Name = name,
        Priority = priority,
        ProductionPlant = plant,
        Conditions = conditions.ToList()
    };

    private static Order Order(string publisher = "99999", string method = "POD",
        string country = "US", int quantity = 10, string bind = "PB") => new()
    {
        OrderId = "1245101",
        PublisherNumber = publisher,
        OrderMethod = method,
        Shipments = [new Shipment { ShipTo = new ShipTo { IsoCountry = country } }],
        Items =
        [
            new OrderItem
            {
                PrintQuantity = quantity,
                Components =
                [
                    new Component
                    {
                        Attributes = new Dictionary<string, string> { ["BindTypeCode"] = bind }
                    }
                ]
            }
        ]
    };

    [Fact]
    public void Equals_condition_routes_to_US()
    {
        var ruleset = RulesetWith(
            Rule("Rule 1", 1, "US",
                new() { Field = "BindTypeCode", Operator = "Equals", Value = "PB" },
                new() { Field = "IsCountry", Operator = "Equals", Value = "US" },
                new() { Field = "PrintQuantity", Operator = "LessThanOrEqual", Value = "20" }));

        var result = _sut.Evaluate(Order(), [ruleset]);

        Assert.True(result.Matched);
        Assert.Equal("US", result.ProductionPlant);
    }

    [Fact]
    public void LessThanOrEqual_works()
    {
        var ruleset = RulesetWith(
            Rule("Rule 1", 1, "US",
                new Condition { Field = "PrintQuantity", Operator = "LessThanOrEqual", Value = "20" }));

        var result = _sut.Evaluate(Order(quantity: 20), [ruleset]);

        Assert.True(result.Matched);
    }

    [Fact]
    public void GreaterThanOrEqual_works()
    {
        var ruleset = RulesetWith(
            Rule("Rule 1", 1, "KGL",
                new Condition { Field = "PrintQuantity", Operator = "GreaterThanOrEqual", Value = "20" }));

        var result = _sut.Evaluate(Order(quantity: 21), [ruleset]);

        Assert.True(result.Matched);
        Assert.Equal("KGL", result.ProductionPlant);
    }

    [Fact]
    public void First_successful_rule_wins()
    {
        var ruleset = RulesetWith(
            Rule("Rule 1", 1, "US",
                new Condition { Field = "BindTypeCode", Operator = "Equals", Value = "PB" }),
            Rule("Rule 2", 2, "KGL",
                new Condition { Field = "BindTypeCode", Operator = "Equals", Value = "PB" }));

        var result = _sut.Evaluate(Order(), [ruleset]);

        Assert.Equal("Rule 1", result.MatchedRule);
        Assert.Equal("US", result.ProductionPlant);
    }

    [Fact]
    public void No_matching_rule_returns_unmatched()
    {
        var ruleset = RulesetWith(
            Rule("Rule 1", 1, "US",
                new Condition { Field = "BindTypeCode", Operator = "Equals", Value = "CV" }));

        var result = _sut.Evaluate(Order(), [ruleset]);

        Assert.False(result.Matched);
        Assert.Null(result.ProductionPlant);
    }

    [Fact]
    public void First_matching_ruleset_wins()
    {
        var first = RulesetWith(
            Rule("Rule A", 1, "US",
                new Condition { Field = "PublisherNumber", Operator = "Equals", Value = "99999" }));
        first.Name = "First";

        var second = RulesetWith(
            Rule("Rule B", 1, "KGL",
                new Condition { Field = "PublisherNumber", Operator = "Equals", Value = "99999" }));
        second.Name = "Second";
        second.Priority = 2;

        var result = _sut.Evaluate(Order(), [first, second]);

        Assert.Equal("First", result.MatchedRuleset);
        Assert.Equal("US", result.ProductionPlant);
    }

    [Fact]
    public void Assignment_sample_returns_US()
    {
        var ruleset = RulesetWith(
            Rule("Rule 1", 1, "US",
                new Condition { Field = "BindTypeCode", Operator = "Equals", Value = "PB" },
                new Condition { Field = "IsCountry", Operator = "Equals", Value = "US" },
                new Condition { Field = "PrintQuantity", Operator = "LessThanOrEqual", Value = "20" }),
            Rule("Rule 2", 2, "UK",
                new Condition { Field = "BindTypeCode", Operator = "Equals", Value = "CV" }),
            Rule("Rule 3", 3, "KGL",
                new Condition { Field = "PrintQuantity", Operator = "GreaterThanOrEqual", Value = "20" }));

        var result = _sut.Evaluate(Order(), [ruleset]);

        Assert.True(result.Matched);
        Assert.Equal("US", result.ProductionPlant);
        Assert.Equal("Ruleset Two", result.MatchedRuleset);
        Assert.Equal("Rule 1", result.MatchedRule);
    }
}
