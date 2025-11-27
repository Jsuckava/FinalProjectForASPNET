using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FinalAspNetProj.DTO
{
    public class SurveyResponse_CreateDTO
    {
        public int QuestionId { get; set; }
        [Range(1, 5, ErrorMessage = "Rating must be between 1 (Lowest) and 5 (Highest).")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
    public class Survey_CreateDTO
    {
        public string RespondentName { get; set; } = null!;

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