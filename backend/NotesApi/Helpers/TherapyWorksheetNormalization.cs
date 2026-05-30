using NotesApi.Models;

namespace NotesApi.Helpers;

public static class TherapyWorksheetNormalization
{
    public static void EnsureAndTrim(SelfValidationEntry e)
    {
        e.Step2 ??= new SelfValidationStep2Doc();
        e.Step3 ??= new SelfValidationStep3Doc();
        e.Step4 ??= new SelfValidationStep4Doc();
        e.Step5 ??= new SelfValidationStep5Doc();

        e.Step2.WhatHappened = (e.Step2.WhatHappened ?? string.Empty).Trim();
        e.Step2.HowIFelt = (e.Step2.HowIFelt ?? string.Empty).Trim();
        e.Step2.WhatINeeded = (e.Step2.WhatINeeded ?? string.Empty).Trim();

        e.Step3.AcknowledgeFeeling = (e.Step3.AcknowledgeFeeling ?? string.Empty).Trim();
        e.Step3.SeparateIdentity = (e.Step3.SeparateIdentity ?? string.Empty).Trim();
        e.Step3.ReframeNarrative = (e.Step3.ReframeNarrative ?? string.Empty).Trim();

        e.Step4.ExternalInfluences = (e.Step4.ExternalInfluences ?? string.Empty).Trim();
        e.Step4.Boundaries = (e.Step4.Boundaries ?? string.Empty).Trim();

        e.Step5.ImmediateAction = (e.Step5.ImmediateAction ?? string.Empty).Trim();
        e.Step5.FuturePrevention = (e.Step5.FuturePrevention ?? string.Empty).Trim();
        e.Step5.AlternativePerspectives = (e.Step5.AlternativePerspectives ?? string.Empty).Trim();

        e.Notes = (e.Notes ?? string.Empty).Trim();
    }

    public static void Trim(SelfConfidenceEntry e)
    {
        e.IdentifyTrigger = (e.IdentifyTrigger ?? string.Empty).Trim();
        e.SupportingEvidence = (e.SupportingEvidence ?? string.Empty).Trim();
        e.RationalizeDoubt = (e.RationalizeDoubt ?? string.Empty).Trim();
        e.ActionPlan = (e.ActionPlan ?? string.Empty).Trim();
    }

    public static void Trim(SelfWorthEntry e)
    {
        e.IdentifyTrigger = (e.IdentifyTrigger ?? string.Empty).Trim();
        e.ImpactAnalysis = (e.ImpactAnalysis ?? string.Empty).Trim();
        e.RealityCheck = (e.RealityCheck ?? string.Empty).Trim();
        e.DefinesSelfWorth = (e.DefinesSelfWorth ?? string.Empty).Trim();
        e.DoesNotDefineSelfWorth = (e.DoesNotDefineSelfWorth ?? string.Empty).Trim();
        e.ActsOfCare = (e.ActsOfCare ?? string.Empty).Trim();
    }

    public static void Trim(SchemaAbandonmentEntry e)
    {
        e.Trigger = (e.Trigger ?? string.Empty).Trim();
        e.CoreBelief = (e.CoreBelief ?? string.Empty).Trim();
        e.EmotionalResponse = (e.EmotionalResponse ?? string.Empty).Trim();
        e.CopingBehavior = (e.CopingBehavior ?? string.Empty).Trim();
        e.HealthierAlternativeBelief = (e.HealthierAlternativeBelief ?? string.Empty).Trim();
        e.WhenUnsupported = (e.WhenUnsupported ?? string.Empty).Trim();
        e.ExpectFromOthers = (e.ExpectFromOthers ?? string.Empty).Trim();
        e.WhatActuallyHappened = (e.WhatActuallyHappened ?? string.Empty).Trim();
    }

    public static void Trim(SchemaFailureEntry e)
    {
        e.Trigger = (e.Trigger ?? string.Empty).Trim();
        e.CoreBelief = (e.CoreBelief ?? string.Empty).Trim();
        e.EmotionalResponse = (e.EmotionalResponse ?? string.Empty).Trim();
        e.CopingBehavior = (e.CopingBehavior ?? string.Empty).Trim();
        e.HealthierAlternativeBelief = (e.HealthierAlternativeBelief ?? string.Empty).Trim();
        e.EvidenceSupports = (e.EvidenceSupports ?? string.Empty).Trim();
        e.EvidenceContradicts = (e.EvidenceContradicts ?? string.Empty).Trim();
    }

    public static void Trim(SchemaPeoplePleasingEntry e)
    {
        e.Trigger = (e.Trigger ?? string.Empty).Trim();
        e.CoreBelief = (e.CoreBelief ?? string.Empty).Trim();
        e.EmotionalResponse = (e.EmotionalResponse ?? string.Empty).Trim();
        e.CopingBehavior = (e.CopingBehavior ?? string.Empty).Trim();
        e.HealthierAlternativeBelief = (e.HealthierAlternativeBelief ?? string.Empty).Trim();
        e.WhenIgnoreNeeds = (e.WhenIgnoreNeeds ?? string.Empty).Trim();
        e.AfraidWillHappen = (e.AfraidWillHappen ?? string.Empty).Trim();
    }

    public static void Trim(SchemaSuppressionEntry e)
    {
        e.Trigger = (e.Trigger ?? string.Empty).Trim();
        e.CoreBelief = (e.CoreBelief ?? string.Empty).Trim();
        e.EmotionalResponse = (e.EmotionalResponse ?? string.Empty).Trim();
        e.CopingBehavior = (e.CopingBehavior ?? string.Empty).Trim();
        e.HealthierAlternativeBelief = (e.HealthierAlternativeBelief ?? string.Empty).Trim();
        e.EmotionsAvoided = (e.EmotionsAvoided ?? string.Empty).Trim();
        e.WhySuppress = (e.WhySuppress ?? string.Empty).Trim();
    }
}
