using System.ComponentModel.DataAnnotations;

namespace FinalAspNetProj.DTO
{
    public class SurveyResponse_CreateDTO
    {
        public int QuestionId { get; set; }
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class Survey_CreateDTO
    {
        public string RespondentName { get; set; } = null!;
        public string? Comment { get; set; }

        [Required]
        public List<SurveyResponse_CreateDTO> Responses { get; set; } = new List<SurveyResponse_CreateDTO>();
    }

    public class SurveyQuestion_ReadDTO
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int MaxRating { get; set; }
    }
}