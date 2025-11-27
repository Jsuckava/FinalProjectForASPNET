namespace FinalAspNetProj.DTO
{
    public class SurveyAnalysis_ReadDTO
    {
        public int AnalysisId { get; set; }
        public int SurveyId { get; set; }
        public int TotalScore { get; set; }
        public decimal PercentageScore { get; set; }
        public decimal FlawProbability { get; set; }
        public decimal ReUseProbability { get; set; }
    }
    public class SurveyAnalysis_CreateDTO
    {
        public int SurveyId { get; set; }
        public int TotalScore { get; set; }
        public decimal PercentageScore { get; set; }
        public decimal FlawProbability { get; set; }
        public decimal ReUseProbability { get; set; }
    }
    public class SurveyAnalysis_UpdateDTO
    {
        public int TotalScore { get; set; }
        public decimal PercentageScore { get; set; }
        public decimal FlawProbability { get; set; }
        public decimal ReUseProbability { get; set; }
    }
}