using AutoMapper;
using LearnApi.Model;
using LearnApi.Repos.Models;

namespace LearnApi.Helper
{
    public class AutoMapperHandler : Profile
    {
        public AutoMapperHandler()
        {
            CreateMap<Student, CustomerModel>().ForMember(item => item.StatusName, opt =>
            opt.MapFrom(x => x.Age > 20 ? "In-Active" : "Active")).ReverseMap();
        }
    }
}
