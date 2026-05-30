namespace NotesApi.Helpers;

public static class BehaviorRules
{
    public static string DeriveActivityLevel(bool exerciseDone, int exerciseDurationMinutes)
    {
        if (!exerciseDone || exerciseDurationMinutes <= 0)
        {
            return "low";
        }

        if (exerciseDurationMinutes < 30)
        {
            return "moderate";
        }

        return "high";
    }
}
