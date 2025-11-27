namespace FinalAspNetProj.Models;

public partial class SurveyResponse
{
    public int ResponseId { get; set; }

    public int SurveyId { get; set; }

    public int QuestionId { get; set; }

    public int Rating { get; set; }

    public virtual SurveyQuestion Question { get; set; } = null!;

    public virtual Survey Survey { get; set; } = null!;
}
