using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalAspNetProj.Models
{
    public class SurveyAnalysis
    {
        public int AnalysisId { get; set; }
        public int SurveyId { get; set; }
        public int TotalScore { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal PercentageScore { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal FlawProbability { get; set; }
        [Column(TypeName = "decimal(5, 2)")]
        public decimal ReUseProbability { get; set; }
        public virtual Survey Survey { get; set; } = null!;
    }
}