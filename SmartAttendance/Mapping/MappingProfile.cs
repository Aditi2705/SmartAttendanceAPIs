using AutoMapper;
using SmartAttendance.DTOs.Student;
using SmartAttendance.DTOs.Subject;
using SmartAttendance.DTOs.Teacher;
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
            CreateMap<Models.Attendance, DTOs.Attendance.AttendanceRecordDto>()
                .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.StudentName))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year.ToString()))
                .ForMember(dest => dest.Semester, opt => opt.MapFrom(src => src.Semester.ToString()))
                .ForMember(dest => dest.IsPresent, opt => opt.MapFrom(src => src.IsPresent));
        }
    }
}
