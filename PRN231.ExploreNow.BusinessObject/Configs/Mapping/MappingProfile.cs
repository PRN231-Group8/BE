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
			// Mapping between Posts and PostsResponse
			CreateMap<Posts, PostsResponse>()
				.ForMember(dest => dest.PostsId, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
				.ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreatedDate));

			// Mapping between PostsRequest and Posts
			CreateMap<PostsRequest, Posts>()
				.ForMember(dest => dest.Comments, opt => opt.Ignore()) // Assuming comments are handled separately
				.ForMember(dest => dest.Photos, opt => opt.Ignore());  // Same for photos
			CreateMap<Posts, PostsRequest>();

			// Mapping between Comments and CommentsResponse
			CreateMap<Comments, CommentsResponse>()
				.ForMember(dest => dest.CommentsId, opt => opt.MapFrom(src => src.Id));

			// Mapping between Photo and PhotoResponse
			CreateMap<Photo, PhotoResponse>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

			// Mapping between ApplicationUser and UserResponse
			CreateMap<ApplicationUser, UserReponse>()
				.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

			CreateMap<TourTimestamp, TourTimeStampResponse>().ReverseMap()
				.ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot))
				.ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

			CreateMap<TourTimeStampRequest, TourTimestamp>()
				.ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot));
			CreateMap<TourTimestamp, TourTimeStampRequest>();

			CreateMap<Location, LocationResponse>()
				.ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos != null ? src.Photos.ToList() : null));

			CreateMap<Photo, PhotoResponse>();

			CreateMap<Tour, TourResponse>();

			CreateMap<TourTrip, VNPayRequest>();

			CreateMap<Transportation, TransportationResponse>();

			// New mappings for TourPackageDetailsResponse
			CreateMap<Tour, TourPackageDetailsResponse>()
				.ForMember(dest => dest.Moods, opt => opt.MapFrom(src => src.TourMoods.Select(tm => tm.Mood)));

			CreateMap<TourTrip, TourTripDetailsResponse>()
				.ForMember(dest => dest.TourTripId, opt => opt.MapFrom(src => src.Id));

			CreateMap<Location, LocationResponse>();

			CreateMap<Transportation, TransportationResponse>();

			CreateMap<Moods, MoodResponseWithoutTours>();

			CreateMap<Photo, PhotoResponse>();

			// New mappings for TourPackageHistoryResponse
			CreateMap<Tour, TourPackageHistoryResponse>()
				.ForMember(dest => dest.Moods, opt => opt.MapFrom(src => src.TourMoods.Select(tm => tm.Mood)))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
				.ForMember(dest => dest.Transactions, opt => opt.MapFrom(src =>
					src.TourTrips.SelectMany(tt => tt.Payments)
						.Where(p => p.Transaction != null)
						.Select(p => p.Transaction)));

			CreateMap<Transaction, TransactionResponse>()
				.ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreatedDate));
		}
	}
}