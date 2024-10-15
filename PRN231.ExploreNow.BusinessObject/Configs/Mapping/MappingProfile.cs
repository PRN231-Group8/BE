using AutoMapper;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.BusinessObject.Configs.Mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<TourTimestamp, TourTimeStampResponse>().ReverseMap()
				.ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot));

			CreateMap<TourTimeStampRequest, TourTimestamp>().ReverseMap()
				.ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot));
		}
	}
}