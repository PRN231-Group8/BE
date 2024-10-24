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
				.ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot))
				.ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

			CreateMap<TourTimeStampRequest, TourTimestamp>()
				.ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot));
			CreateMap<TourTimestamp, TourTimeStampRequest>();

			CreateMap<Location, LocationResponse>()
				.ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos != null ? src.Photos.ToList() : null));

			CreateMap<Photo, PhotoResponse>();
			CreateMap<Tour,TourResponse>();
		}
	}
}