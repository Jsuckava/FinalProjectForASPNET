namespace FinalAspNetProj.Models
{
    public class Survey
    {
        public int SurveyId { get; set; }
        public string RespondentName { get; set; } = null!;
        public string? Comment { get; set; }
        public DateTime DateCompleted { get; set; }
        public virtual SurveyAnalysis SurveyAnalysis { get; set; } = null!;
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
    }
}