using System;
using System.Collections.Generic;

namespace FinalAspNetProj.Models;

public partial class SurveyQuestion
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int MaxRating { get; set; }
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
}
