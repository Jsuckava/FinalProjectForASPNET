using AutoMapper;
using FinalAspNetProj.DTO;
using FinalAspNetProj.Models;

namespace FinalAspNetProj.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Question_CreateDTO, Question>();
            CreateMap<Question, Question_ReadDTO>();
            CreateMap<Question, SurveyQuestion_ReadDTO>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Text));
            CreateMap<SurveyTemplate_CreateDTO, SurveyTemplate>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore());
            CreateMap<SurveyTemplate_UpdateDTO, SurveyTemplate>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore());
            CreateMap<SurveyTemplate, SurveyTemplate_ReadDTO>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src =>
                    src.Questions.Select(q => new Question_ReadDTO
                    {
                        QuestionId = q.QuestionId,
                        MaxRating = q.MaxRating,
                        QuestionText = q.Text
                    })))
                .ForMember(dest => dest.QuestionCount, opt => opt.Ignore());

            CreateMap<Survey_CreateDTO, Survey>();
            CreateMap<Survey, Survey_CreateDTO>();
            CreateMap<SurveyResponse_CreateDTO, SurveyResponse>();
            CreateMap<SurveyAnalysis, SurveyAnalysis_ReadDTO>();
            CreateMap<SurveyAnalysis_CreateDTO, SurveyAnalysis>();
            CreateMap<SurveyAnalysis_UpdateDTO, SurveyAnalysis>();
            CreateMap<DownloadableFile, DownloadableFile_ReadDTO>();
        }
    }
}