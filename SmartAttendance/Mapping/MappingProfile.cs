using AutoMapper;
using SmartAttendance.DTOs.Admin;
using SmartAttendance.Models;

namespace SmartAttendance.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Student, CreateStudentDto>().ReverseMap();
            CreateMap<Student, UpdateStudentDto>().ReverseMap();
            CreateMap<Student, StudentListDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : ""))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.ClassName));
            CreateMap<Subject, CreateSubjectDto>().ReverseMap();
            CreateMap<Subject, UpdateSubjectDto>().ReverseMap();
            CreateMap<Subject, GetSubjectDto>().ReverseMap();
            CreateMap<Teacher, CreateTeacherDto>().ReverseMap();
            CreateMap<Teacher, UpdateTeacherDto>().ReverseMap();
        }
    }
}
