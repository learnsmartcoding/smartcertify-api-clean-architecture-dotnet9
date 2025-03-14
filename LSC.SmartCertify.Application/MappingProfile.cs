using AutoMapper;
using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Domain.Entities;

namespace LSC.SmartCertify.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<CreateCourseDto, Course>();
            CreateMap<UpdateCourseDto, Course>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            //CreateMap<Question, QuestionDto>().ReverseMap();
            CreateMap<CreateQuestionDto, Question>();
            CreateMap<UpdateQuestionDto, Question>();

            CreateMap<Choice, ChoiceDto>().ReverseMap();
            CreateMap<CreateChoiceDto, Choice>();
            CreateMap<UpdateChoiceDto, Choice>();

            CreateMap<UpdateUserQuestionChoiceDto, ExamQuestion>().ReverseMap();
            CreateMap<UserExamQuestionsDto, ExamQuestion>().ReverseMap();

            CreateMap<Exam, ExamResponseDto>()
           .ForMember(dest => dest.Questions, opt => opt.Ignore()); // Populate manually
                                                                    // 
                                                                    // Map Question to QuestionDto and vice versa
            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.Choices, opt => opt.MapFrom(src => src.Choices));

            CreateMap<QuestionDto, Question>()
                .ForMember(dest => dest.Choices, opt => opt.Ignore()); // Ignore to handle manually

            // Map Choice to ChoiceDto and vice versa
            //CreateMap<Choice, ChoiceDto>().ReverseMap();
        }
    }

}
