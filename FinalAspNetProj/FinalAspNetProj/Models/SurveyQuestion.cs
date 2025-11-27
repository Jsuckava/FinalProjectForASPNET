//using System.ComponentModel.DataAnnotations;

//namespace FinalAspNetProj.Models
//{    
//    public class Question
//    {
//        [Key]
//        public int QuestionId { get; set; }
//        public int SurveyTemplateID { get; set; }
//        public string Text { get; set; } = null!;
//        public int MaxRating { get; set; } = 5;

//        public virtual SurveyTemplate SurveyTemplate { get; set; } = null!;
//        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; } = new List<SurveyResponse>();
//    }
//}