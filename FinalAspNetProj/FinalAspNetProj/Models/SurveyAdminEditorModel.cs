using System.ComponentModel.DataAnnotations;

namespace FinalAspNetProj.Models
{
    public class SurveyTemplate
    {
        public int SurveyTemplateID { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        public int SurveyTemplateID { get; set; }
        public string Text { get; set; } = null!;
        public int MaxRating { get; set; } = 5;
        public virtual SurveyTemplate SurveyTemplate { get; set; } = null!;       
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();

    }
}