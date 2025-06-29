using AutoMapper;
using TransactionService.Models;


namespace TransactionService.Mapping
{
    public class AutoMapperProfile :Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Transaction,TransactionDto>().ReverseMap();   
           
        }
    }
}
