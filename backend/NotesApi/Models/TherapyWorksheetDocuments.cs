using MongoDB.Bson.Serialization.Attributes;

namespace NotesApi.Models;

#region Self-validation

public class SelfValidationStep2Doc
{
    [BsonElement("whatHappened")]
    public string WhatHappened { get; set; } = string.Empty;

    [BsonElement("howIFelt")]
    public string HowIFelt { get; set; } = string.Empty;

    [BsonElement("whatINeeded")]
    public string WhatINeeded { get; set; } = string.Empty;
}

public class SelfValidationStep3Doc
{
    [BsonElement("acknowledgeFeeling")]
    public string AcknowledgeFeeling { get; set; } = string.Empty;

    [BsonElement("separateIdentity")]
    public string SeparateIdentity { get; set; } = string.Empty;

    [BsonElement("reframeNarrative")]
    public string ReframeNarrative { get; set; } = string.Empty;
}

public class SelfValidationStep4Doc
{
    [BsonElement("externalInfluences")]
    public string ExternalInfluences { get; set; } = string.Empty;

    [BsonElement("boundaries")]
    public string Boundaries { get; set; } = string.Empty;
}

public class SelfValidationStep5Doc
{
    [BsonElement("immediateAction")]
    public string ImmediateAction { get; set; } = string.Empty;

    [BsonElement("futurePrevention")]
    public string FuturePrevention { get; set; } = string.Empty;

    [BsonElement("alternativePerspectives")]
    public string AlternativePerspectives { get; set; } = string.Empty;
}

public class SelfValidationEntry : WorksheetEntryBase
{
    [BsonElement("step2")]
    public SelfValidationStep2Doc Step2 { get; set; } = new();

    [BsonElement("step3")]
    public SelfValidationStep3Doc Step3 { get; set; } = new();

    [BsonElement("step4")]
    public SelfValidationStep4Doc Step4 { get; set; } = new();

    [BsonElement("step5")]
    public SelfValidationStep5Doc Step5 { get; set; } = new();

    [BsonElement("notes")]
    public string Notes { get; set; } = string.Empty;
}

#endregion

#region Self-confidence

public class SelfConfidenceEntry : WorksheetEntryBase
{
    [BsonElement("identifyTrigger")]
    public string IdentifyTrigger { get; set; } = string.Empty;

    [BsonElement("supportingEvidence")]
    public string SupportingEvidence { get; set; } = string.Empty;

    [BsonElement("rationalizeDoubt")]
    public string RationalizeDoubt { get; set; } = string.Empty;

    [BsonElement("actionPlan")]
    public string ActionPlan { get; set; } = string.Empty;
}

#endregion

#region Self-worth

public class SelfWorthEntry : WorksheetEntryBase
{
    [BsonElement("identifyTrigger")]
    public string IdentifyTrigger { get; set; } = string.Empty;

    [BsonElement("impactAnalysis")]
    public string ImpactAnalysis { get; set; } = string.Empty;

    [BsonElement("realityCheck")]
    public string RealityCheck { get; set; } = string.Empty;

    [BsonElement("definesSelfWorth")]
    public string DefinesSelfWorth { get; set; } = string.Empty;

    [BsonElement("doesNotDefineSelfWorth")]
    public string DoesNotDefineSelfWorth { get; set; } = string.Empty;

    [BsonElement("actsOfCare")]
    public string ActsOfCare { get; set; } = string.Empty;
}

#endregion

#region Schema worksheets

public class SchemaAbandonmentEntry : WorksheetEntryBase
{
    [BsonElement("trigger")]
    public string Trigger { get; set; } = string.Empty;

    [BsonElement("coreBelief")]
    public string CoreBelief { get; set; } = string.Empty;

    [BsonElement("emotionalResponse")]
    public string EmotionalResponse { get; set; } = string.Empty;

    [BsonElement("copingBehavior")]
    public string CopingBehavior { get; set; } = string.Empty;

    [BsonElement("healthierAlternativeBelief")]
    public string HealthierAlternativeBelief { get; set; } = string.Empty;

    [BsonElement("whenUnsupported")]
    public string WhenUnsupported { get; set; } = string.Empty;

    [BsonElement("expectFromOthers")]
    public string ExpectFromOthers { get; set; } = string.Empty;

    [BsonElement("whatActuallyHappened")]
    public string WhatActuallyHappened { get; set; } = string.Empty;
}

public class SchemaFailureEntry : WorksheetEntryBase
{
    [BsonElement("trigger")]
    public string Trigger { get; set; } = string.Empty;

    [BsonElement("coreBelief")]
    public string CoreBelief { get; set; } = string.Empty;

    [BsonElement("emotionalResponse")]
    public string EmotionalResponse { get; set; } = string.Empty;

    [BsonElement("copingBehavior")]
    public string CopingBehavior { get; set; } = string.Empty;

    [BsonElement("healthierAlternativeBelief")]
    public string HealthierAlternativeBelief { get; set; } = string.Empty;

    [BsonElement("evidenceSupports")]
    public string EvidenceSupports { get; set; } = string.Empty;

    [BsonElement("evidenceContradicts")]
    public string EvidenceContradicts { get; set; } = string.Empty;
}

public class SchemaPeoplePleasingEntry : WorksheetEntryBase
{
    [BsonElement("trigger")]
    public string Trigger { get; set; } = string.Empty;

    [BsonElement("coreBelief")]
    public string CoreBelief { get; set; } = string.Empty;

    [BsonElement("emotionalResponse")]
    public string EmotionalResponse { get; set; } = string.Empty;

    [BsonElement("copingBehavior")]
    public string CopingBehavior { get; set; } = string.Empty;

    [BsonElement("healthierAlternativeBelief")]
    public string HealthierAlternativeBelief { get; set; } = string.Empty;

    [BsonElement("whenIgnoreNeeds")]
    public string WhenIgnoreNeeds { get; set; } = string.Empty;

    [BsonElement("afraidWillHappen")]
    public string AfraidWillHappen { get; set; } = string.Empty;
}

public class SchemaSuppressionEntry : WorksheetEntryBase
{
    [BsonElement("trigger")]
    public string Trigger { get; set; } = string.Empty;

    [BsonElement("coreBelief")]
    public string CoreBelief { get; set; } = string.Empty;

    [BsonElement("emotionalResponse")]
    public string EmotionalResponse { get; set; } = string.Empty;

    [BsonElement("copingBehavior")]
    public string CopingBehavior { get; set; } = string.Empty;

    [BsonElement("healthierAlternativeBelief")]
    public string HealthierAlternativeBelief { get; set; } = string.Empty;

    [BsonElement("emotionsAvoided")]
    public string EmotionsAvoided { get; set; } = string.Empty;

    [BsonElement("whySuppress")]
    public string WhySuppress { get; set; } = string.Empty;
}

#endregion
