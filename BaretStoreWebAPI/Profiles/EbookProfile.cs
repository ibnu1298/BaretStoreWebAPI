using AutoMapper;
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;

namespace BaretStoreWebAPI.Profiles
{
    public class EbookProfile : Profile
    {
        public EbookProfile()
        {
            CreateMap<MultipleEbookDTO, Ebook>();

            CreateMap<Ebook,EbookDTO> ();
            CreateMap<EbookDTO, Ebook>();
        }
    }
}
