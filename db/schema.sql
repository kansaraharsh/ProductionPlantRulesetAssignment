IF DB_ID(N'ProductionPlantDb') IS NULL
BEGIN
    CREATE DATABASE ProductionPlantDb;
END
GO

USE ProductionPlantDb;
GO

/* =========================================================
   DROP EXISTING OBJECTS
   ========================================================= */

IF OBJECT_ID(N'dbo.EvaluationAudits', N'U') IS NOT NULL
    DROP TABLE dbo.EvaluationAudits;

IF OBJECT_ID(N'dbo.Conditions', N'U') IS NOT NULL
    DROP TABLE dbo.Conditions;

IF OBJECT_ID(N'dbo.Rules', N'U') IS NOT NULL
    DROP TABLE dbo.Rules;

IF OBJECT_ID(N'dbo.Rulesets', N'U') IS NOT NULL
    DROP TABLE dbo.Rulesets;
GO


/* =========================================================
   RULESETS
   ========================================================= */

CREATE TABLE dbo.Rulesets
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Rulesets PRIMARY KEY,

    Name NVARCHAR(200) NOT NULL,

    Priority INT NOT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_Rulesets_IsActive DEFAULT 1,

    CONSTRAINT UQ_Rulesets_Name
        UNIQUE(Name)
);
GO


/* =========================================================
   RULES
   ========================================================= */

CREATE TABLE dbo.Rules
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Rules PRIMARY KEY,

    RulesetId INT NOT NULL,

    Name NVARCHAR(200) NOT NULL,

    Priority INT NOT NULL,

    ProductionPlant NVARCHAR(50) NOT NULL,

    CONSTRAINT FK_Rules_Rulesets
        FOREIGN KEY(RulesetId)
        REFERENCES dbo.Rulesets(Id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION
);
GO


/* =========================================================
   CONDITIONS
   ========================================================= */

CREATE TABLE dbo.Conditions
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Conditions PRIMARY KEY,

    RulesetId INT NULL,

    RuleId INT NULL,

    Field NVARCHAR(100) NOT NULL,

    Operator NVARCHAR(50) NOT NULL,

    Value NVARCHAR(500) NOT NULL,

    Sequence INT NOT NULL
        CONSTRAINT DF_Conditions_Sequence DEFAULT 1,

    CONSTRAINT FK_Conditions_Rulesets
        FOREIGN KEY(RulesetId)
        REFERENCES dbo.Rulesets(Id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT FK_Conditions_Rules
        FOREIGN KEY(RuleId)
        REFERENCES dbo.Rules(Id)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,

    CONSTRAINT CK_Conditions_OneParent
        CHECK
        (
            (RulesetId IS NOT NULL AND RuleId IS NULL)
            OR
            (RulesetId IS NULL AND RuleId IS NOT NULL)
        )
);
GO


/* =========================================================
   EVALUATION AUDIT
   ========================================================= */

CREATE TABLE dbo.EvaluationAudits
(
    Id BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_EvaluationAudits PRIMARY KEY,

    OrderId NVARCHAR(100) NOT NULL,

    EvaluatedAtUtc DATETIMEOFFSET(7) NOT NULL,

    InputJson NVARCHAR(MAX) NOT NULL,

    Matched BIT NOT NULL,

    ProductionPlant NVARCHAR(50) NULL,

    MatchedRuleset NVARCHAR(200) NULL,

    MatchedRule NVARCHAR(200) NULL,

    Reason NVARCHAR(2000) NOT NULL,

    ConditionsJson NVARCHAR(MAX) NOT NULL
);
GO


/* =========================================================
   INDEXES
   ========================================================= */

CREATE INDEX IX_Rulesets_Active_Priority
ON dbo.Rulesets(IsActive, Priority);
GO

CREATE INDEX IX_Rules_Ruleset_Priority
ON dbo.Rules(RulesetId, Priority);
GO

CREATE INDEX IX_Conditions_Ruleset_Rule_Sequence
ON dbo.Conditions(RulesetId, RuleId, Sequence);
GO

CREATE INDEX IX_EvaluationAudits_OrderId_EvaluatedAt
ON dbo.EvaluationAudits(OrderId, EvaluatedAtUtc DESC);
GO