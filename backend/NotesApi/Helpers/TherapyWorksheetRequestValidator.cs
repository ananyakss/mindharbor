using NotesApi.Models;

namespace NotesApi.Helpers;

public static class TherapyWorksheetRequestValidator
{
    public static string? ValidateSelfValidation(SelfValidationEntry draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Step2.WhatHappened)
            || string.IsNullOrWhiteSpace(draft.Step2.HowIFelt)
            || string.IsNullOrWhiteSpace(draft.Step2.WhatINeeded))
        {
            return "Step 2 fields (what happened, how you felt, what you needed) are required.";
        }

        if (string.IsNullOrWhiteSpace(draft.Step3.AcknowledgeFeeling)
            || string.IsNullOrWhiteSpace(draft.Step3.SeparateIdentity)
            || string.IsNullOrWhiteSpace(draft.Step3.ReframeNarrative))
        {
            return "Step 3 validation fields are required.";
        }

        if (string.IsNullOrWhiteSpace(draft.Step4.ExternalInfluences)
            || string.IsNullOrWhiteSpace(draft.Step4.Boundaries))
        {
            return "Step 4 approach fields are required.";
        }

        if (string.IsNullOrWhiteSpace(draft.Step5.ImmediateAction)
            || string.IsNullOrWhiteSpace(draft.Step5.FuturePrevention)
            || string.IsNullOrWhiteSpace(draft.Step5.AlternativePerspectives))
        {
            return "Step 5 solution fields are required.";
        }

        // notes may be empty
        return null;
    }

    public static string? ValidateSelfConfidence(SelfConfidenceEntry draft)
    {
        if (string.IsNullOrWhiteSpace(draft.IdentifyTrigger)
            || string.IsNullOrWhiteSpace(draft.SupportingEvidence)
            || string.IsNullOrWhiteSpace(draft.RationalizeDoubt)
            || string.IsNullOrWhiteSpace(draft.ActionPlan))
        {
            return "All self-confidence worksheet fields are required.";
        }

        return null;
    }

    public static string? ValidateSelfWorth(SelfWorthEntry draft)
    {
        if (string.IsNullOrWhiteSpace(draft.IdentifyTrigger)
            || string.IsNullOrWhiteSpace(draft.ImpactAnalysis)
            || string.IsNullOrWhiteSpace(draft.RealityCheck)
            || string.IsNullOrWhiteSpace(draft.DefinesSelfWorth)
            || string.IsNullOrWhiteSpace(draft.DoesNotDefineSelfWorth)
            || string.IsNullOrWhiteSpace(draft.ActsOfCare))
        {
            return "All self-worth worksheet fields are required.";
        }

        return null;
    }

    public static string? ValidateSchemaAbandonment(SchemaAbandonmentEntry draft) =>
        ValidateSchemaCore(draft) ?? ValidateExtra(
            string.IsNullOrWhiteSpace(draft.WhenUnsupported)
            || string.IsNullOrWhiteSpace(draft.ExpectFromOthers)
            || string.IsNullOrWhiteSpace(draft.WhatActuallyHappened),
            "When unsupported, expectations, and what happened are required.");

    public static string? ValidateSchemaFailure(SchemaFailureEntry draft) =>
        ValidateSchemaCore(draft) ?? ValidateExtra(
            string.IsNullOrWhiteSpace(draft.EvidenceSupports)
            || string.IsNullOrWhiteSpace(draft.EvidenceContradicts),
            "Evidence for and against the belief are required.");

    public static string? ValidateSchemaPeoplePleasing(SchemaPeoplePleasingEntry draft) =>
        ValidateSchemaCore(draft) ?? ValidateExtra(
            string.IsNullOrWhiteSpace(draft.WhenIgnoreNeeds)
            || string.IsNullOrWhiteSpace(draft.AfraidWillHappen),
            "When you ignore needs and what you're afraid of are required.");

    public static string? ValidateSchemaSuppression(SchemaSuppressionEntry draft) =>
        ValidateSchemaCore(draft) ?? ValidateExtra(
            string.IsNullOrWhiteSpace(draft.EmotionsAvoided)
            || string.IsNullOrWhiteSpace(draft.WhySuppress),
            "Emotions avoided and reasons for suppressing are required.");

    private static string? ValidateSchemaCore(SchemaAbandonmentEntry e) =>
        CoreFieldsOk(e.Trigger, e.CoreBelief, e.EmotionalResponse, e.CopingBehavior, e.HealthierAlternativeBelief);

    private static string? ValidateSchemaCore(SchemaFailureEntry e) =>
        CoreFieldsOk(e.Trigger, e.CoreBelief, e.EmotionalResponse, e.CopingBehavior, e.HealthierAlternativeBelief);

    private static string? ValidateSchemaCore(SchemaPeoplePleasingEntry e) =>
        CoreFieldsOk(e.Trigger, e.CoreBelief, e.EmotionalResponse, e.CopingBehavior, e.HealthierAlternativeBelief);

    private static string? ValidateSchemaCore(SchemaSuppressionEntry e) =>
        CoreFieldsOk(e.Trigger, e.CoreBelief, e.EmotionalResponse, e.CopingBehavior, e.HealthierAlternativeBelief);

    private static string? CoreFieldsOk(
        string trigger,
        string core,
        string emotional,
        string coping,
        string healthier) =>
        string.IsNullOrWhiteSpace(trigger)
        || string.IsNullOrWhiteSpace(core)
        || string.IsNullOrWhiteSpace(emotional)
        || string.IsNullOrWhiteSpace(coping)
        || string.IsNullOrWhiteSpace(healthier)
            ? "Trigger, core belief, emotional response, coping behavior, and healthier belief are required."
            : null;

    private static string? ValidateExtra(bool invalid, string message) => invalid ? message : null;
}
