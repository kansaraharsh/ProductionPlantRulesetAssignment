USE ProductionPlantDb;
GO

/* =========================================================
   CLEAR EXISTING DATA
   ========================================================= */

DELETE FROM dbo.EvaluationAudits;
DELETE FROM dbo.Conditions;
DELETE FROM dbo.Rules;
DELETE FROM dbo.Rulesets;
GO


/* =========================================================
   RULESETS
   ========================================================= */

SET IDENTITY_INSERT dbo.Rulesets ON;

INSERT INTO dbo.Rulesets
(
    Id,
    Name,
    Priority,
    IsActive
)
VALUES
(
    1,
    N'Ruleset One',
    10,
    1
),
(
    2,
    N'Ruleset Two',
    20,
    1
);

SET IDENTITY_INSERT dbo.Rulesets OFF;
GO


/* =========================================================
   RULESET ONE CONDITIONS
   ========================================================= */

INSERT INTO dbo.Conditions
(
    RulesetId,
    RuleId,
    Field,
    Operator,
    Value,
    Sequence
)
VALUES
(
    1,
    NULL,
    N'PublisherNumber',
    N'Equals',
    N'99990',
    1
),
(
    1,
    NULL,
    N'OrderMethod',
    N'Equals',
    N'POD',
    2
);
GO


/* =========================================================
   RULESET TWO CONDITIONS
   ========================================================= */

INSERT INTO dbo.Conditions
(
    RulesetId,
    RuleId,
    Field,
    Operator,
    Value,
    Sequence
)
VALUES
(
    2,
    NULL,
    N'PublisherNumber',
    N'Equals',
    N'99999',
    1
),
(
    2,
    NULL,
    N'OrderMethod',
    N'Equals',
    N'POD',
    2
);
GO


/* =========================================================
   RULES
   ========================================================= */

INSERT INTO dbo.Rules
(
    RulesetId,
    Name,
    Priority,
    ProductionPlant
)
VALUES
(
    1,
    N'Rule 1',
    10,
    N'US'
),
(
    2,
    N'Rule 1',
    10,
    N'US'
),
(
    2,
    N'Rule 2',
    20,
    N'UK'
),
(
    2,
    N'Rule 3',
    30,
    N'KGL'
);
GO


/* =========================================================
   GET RULE IDS
   ========================================================= */

DECLARE @RulesetOneRule1 INT;
DECLARE @RulesetTwoRule1 INT;
DECLARE @RulesetTwoRule2 INT;
DECLARE @RulesetTwoRule3 INT;

SELECT @RulesetOneRule1 = Id
FROM dbo.Rules
WHERE RulesetId = 1
AND Name = N'Rule 1';

SELECT @RulesetTwoRule1 = Id
FROM dbo.Rules
WHERE RulesetId = 2
AND Name = N'Rule 1';

SELECT @RulesetTwoRule2 = Id
FROM dbo.Rules
WHERE RulesetId = 2
AND Name = N'Rule 2';

SELECT @RulesetTwoRule3 = Id
FROM dbo.Rules
WHERE RulesetId = 2
AND Name = N'Rule 3';


/* =========================================================
   RULE 1 - RULESET ONE
   ========================================================= */

INSERT INTO dbo.Conditions
(
    RuleId,
    Field,
    Operator,
    Value,
    Sequence
)
VALUES
(
    @RulesetOneRule1,
    N'BindTypeCode',
    N'Equals',
    N'PB',
    1
),
(
    @RulesetOneRule1,
    N'IsCountry',
    N'Equals',
    N'US',
    2
),
(
    @RulesetOneRule1,
    N'PrintQuantity',
    N'LessThanOrEqual',
    N'20',
    3
);


/* =========================================================
   RULE 1 - RULESET TWO
   Added to satisfy the worked example in the assignment
   ========================================================= */

INSERT INTO dbo.Conditions
(
    RuleId,
    Field,
    Operator,
    Value,
    Sequence
)
VALUES
(
    @RulesetTwoRule1,
    N'BindTypeCode',
    N'Equals',
    N'PB',
    1
),
(
    @RulesetTwoRule1,
    N'IsCountry',
    N'Equals',
    N'US',
    2
),
(
    @RulesetTwoRule1,
    N'PrintQuantity',
    N'LessThanOrEqual',
    N'20',
    3
);


/* =========================================================
   RULE 2 - RULESET TWO
   ========================================================= */

INSERT INTO dbo.Conditions
(
    RuleId,
    Field,
    Operator,
    Value,
    Sequence
)
VALUES
(
    @RulesetTwoRule2,
    N'BindTypeCode',
    N'Equals',
    N'CV',
    1
),
(
    @RulesetTwoRule2,
    N'IsCountry',
    N'Equals',
    N'UK',
    2
),
(
    @RulesetTwoRule2,
    N'PrintQuantity',
    N'LessThanOrEqual',
    N'20',
    3
);


/* =========================================================
   RULE 3 - RULESET TWO
   ========================================================= */

INSERT INTO dbo.Conditions
(
    RuleId,
    Field,
    Operator,
    Value,
    Sequence
)
VALUES
(
    @RulesetTwoRule3,
    N'BindTypeCode',
    N'Equals',
    N'PB',
    1
),
(
    @RulesetTwoRule3,
    N'IsCountry',
    N'Equals',
    N'US',
    2
),
(
    @RulesetTwoRule3,
    N'PrintQuantity',
    N'GreaterThanOrEqual',
    N'20',
    3
);
GO