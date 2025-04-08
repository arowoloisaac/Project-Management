using AutoMapper;
using Task_Management_System.Models;

namespace Task_Management_System.Configurations
{
    public class AutoMapperConfiguration: Profile
    {
        public AutoMapperConfiguration()
        {
            //       source  destination  
            CreateMap<User, GetProfileDto>();
        }
    }
}
