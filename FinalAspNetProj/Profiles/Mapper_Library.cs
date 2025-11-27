using AutoMapper;
using FinalAspNetProj.DTO;
using FinalAspNetProj.Models;

namespace FinalAspNetProj.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SurveyQuestion, SurveyQuestion_ReadDTO>();
            CreateMap<SurveyAnalysis, SurveyAnalysis_ReadDTO>();
            CreateMap<SurveyAnalysis_CreateDTO, SurveyAnalysis>();
            CreateMap<SurveyAnalysis_UpdateDTO, SurveyAnalysis>();
            CreateMap<Survey, Survey_CreateDTO>();
            CreateMap<DownloadableFile, DownloadableFile_ReadDTO>();
            CreateMap<Question_CreateDTO, Question>();
            CreateMap<Question, Question_ReadDTO>();
            CreateMap<SurveyTemplate_CreateDTO, SurveyTemplate>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore());
            CreateMap<SurveyTemplate_UpdateDTO, SurveyTemplate>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore());
            CreateMap<SurveyTemplate, SurveyTemplate_ReadDTO>()
                .ForMember(dest => dest.QuestionCount, opt => opt.Ignore());
        }
    }
}