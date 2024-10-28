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

			CreateMap<TourTrip, VNPayRequest>();

			CreateMap<Transportation, TransportationResponse>();

			// New mappings for TourPackageDetailsResponse
			CreateMap<Tour, TourPackageDetailsResponse>()
				.ForMember(dest => dest.Moods, opt => opt.MapFrom(src => src.TourMoods.Select(tm => tm.Mood)));

			CreateMap<TourTrip, TourTripDetailsResponse>()
				.ForMember(dest => dest.TourTripId, opt => opt.MapFrom(src => src.Id));

			CreateMap<Location, LocationResponse>()
				.ForMember(dest => dest.Photos, opt => opt.Ignore());

			CreateMap<Transportation, TransportationResponse>();

			CreateMap<Moods, MoodResponse>()
				.ForMember(dest => dest.TourMoods, opt => opt.Ignore());

			CreateMap<Photo, PhotoResponse>();

			// New mappings for TourPackageHistoryResponse
			CreateMap<Tour, TourPackageHistoryResponse>()
				.ForMember(dest => dest.Moods, opt => opt.MapFrom(src => src.TourMoods.Select(tm => tm.Mood)))
				.ForMember(dest => dest.Transactions, opt => opt.MapFrom(src =>
					src.TourTrips.SelectMany(tt => tt.Payments)
						.Where(p => p.Transaction != null)
						.Select(p => p.Transaction)));

			CreateMap<Transaction, TransactionResponse>()
				.ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreatedDate));
		}
	}
}