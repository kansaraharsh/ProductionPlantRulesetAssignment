# Design Document — Production Plant Ruleset Evaluation

## 1. Problem

Orders can originate from a portal, ZIP/file-drop integration or client API. The payload is consistent, while routing rules vary by publisher, order method, country, binding type and print quantity.

The system automates the manual plant selection process.

## 2. Architecture

    Portal[Portal / React UI]    
    Client[External Client]
    API[ASP.NET Core API]
    App[Application Layer]
    Domain[Domain Rule Engine]
    DB[(Azure SQL / SQL Server)]
    Cache[(Memory Cache)]
    Audit[Evaluation Audit]
    Insights[Application Insights]

    Portal --> API    
    Client --> API
    API --> App
    App --> Domain
    App --> DB
    App --> Cache
    App --> Audit
    API --> Insights

### Layers

**API**
- HTTP transport
- JSON parsing
- request validation
- HTTP response mapping
- Swagger

**Application**
- Evaluation orchestration
- repository abstraction
- audit persistence

**Domain**
- ruleset/rule/condition entities
- condition evaluation
- order field extraction
- comparison logic
- no EF Core or HTTP dependencies

**Infrastructure**
- EF Core
- SQL Server/Azure SQL
- cache
- repository implementations

**React**
- lightweight operator UI for manual testing
- sends the same JSON payload accepted by the API

## 3. Evaluation algorithm

1. Load active rulesets ordered by priority.
2. For each ruleset:
   - evaluate every top-level condition.
   - if all pass, this is the only ruleset evaluated further.
3. Within the selected ruleset, order rules by priority.
4. Evaluate each rule's conditions.
5. Return the first rule for which all conditions pass.
6. If no rule passes, return no-match.
7. Persist the complete decision and condition evaluations.

This implements "first match wins".

## 4. Condition semantics

Conditions within one ruleset or rule are AND conditions.

Supported operators:

| Operator | Meaning |
|---|---|
| Equals | case-insensitive string equality |
| LessThanOrEqual | numeric <= |
| GreaterThanOrEqual | numeric >= |

The value in the database remains a string so the same condition table can support strings and numbers.

## 5. Data model

    RULESETS {
      int Id PK
      string Name
      int Priority
      bool IsActive
    }

    RULES {
      int Id PK
      int RulesetId FK
      string Name
      int Priority
      string ProductionPlant
    }

    CONDITIONS {
      int Id PK
      int RulesetId FK
      int RuleId FK
      string Field
      string Operator
      string Value
      int Sequence
    }

    EVALUATIONAUDITS {
      long Id PK
      string OrderId
      datetime EvaluatedAtUtc
      string InputJson
      bool Matched
      string ProductionPlant
      string MatchedRuleset
      string MatchedRule
      string Reason
      string ConditionsJson
    }

`Conditions` has exactly one parent: either `RulesetId` or `RuleId`.

## 6. Extensibility

Adding a new rule does not require code:

```sql
INSERT INTO dbo.Rules(...);
INSERT INTO dbo.Conditions(...);
```

The evaluator is data-driven.

Adding a new field requires adding field extraction to `OrderFieldAccessor`, because the incoming order JSON needs a deterministic mapping. An enterprise variant could replace this switch with a JSONPath-based field registry stored in the database.

Adding a brand-new operator requires an operator strategy implementation. The current assignment's operator set is fully supported.

## 7. Caching

Rulesets are cached for five minutes using `IMemoryCache`.

Advantages:
- avoids repeated DB reads
- simple deployment
- evaluator remains fast

For multiple API instances:
- use Redis/Azure Managed Redis, or
- introduce a distributed cache and explicit rule-version invalidation.

## 8. Audit logging

Every evaluation is stored in `EvaluationAudits`:

- order ID
- UTC timestamp
- complete input JSON
- match status
- production plant
- ruleset/rule
- human-readable reason
- condition-by-condition JSON

This supports troubleshooting and auditability.

## 9. Error handling

The API returns HTTP 400 for:
- invalid JSON
- null payload
- missing required order fields
- empty items

Unexpected exceptions are converted into HTTP 500 responses without exposing internal details in production.

## 10. Assumptions and source inconsistency

The assignment's sample RulesetConfig puts Rule 1 under Ruleset One, but its worked example says Ruleset Two matches and Rule 1 succeeds. It also expects sample order `1245101` to return US.

This implementation prioritizes the worked expected behavior and seeds Rule 1 under Ruleset Two. The sample configuration is included in `sample/RulesetConfig.json` with that adjustment explicitly documented.

Quantity 20 satisfies both `<= 20` and `>= 20`; priority/order makes Rule 1 win first.
