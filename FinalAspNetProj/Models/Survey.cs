using System;
using System.Collections.Generic;

namespace FinalAspNetProj.Models;

public partial class Survey
{
    public int SurveyId { get; set; }

    public string RespondentName { get; set; } = null!;

    public DateTime DateCompleted { get; set; }

    public virtual SurveyAnalysis? SurveyAnalysis { get; set; }
    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
}
