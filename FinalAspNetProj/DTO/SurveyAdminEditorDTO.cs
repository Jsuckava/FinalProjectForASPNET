using System.ComponentModel.DataAnnotations;

namespace FinalAspNetProj.DTO
{
    public class Question_CreateDTO
    {
        public string Text { get; set; } = null!;
        public int MaxRating { get; set; } = 5;
    }

    public class SurveyTemplate_CreateDTO
    {
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<Question_CreateDTO> Questions { get; set; } = new List<Question_CreateDTO>();
    }

    public class SurveyTemplate_UpdateDTO : SurveyTemplate_CreateDTO
    {
    }

    public class Question_ReadDTO
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = null!;
        public int MaxRating { get; set; }
    }

    public class SurveyTemplate_ReadDTO
    {
        public int SurveyTemplateID { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int QuestionCount { get; set; }
        public List<Question_ReadDTO> Questions { get; set; } = new List<Question_ReadDTO>();
    }
}