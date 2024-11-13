using AutoMapper;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.OtherObjects;
using System.Net;

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
            CreateMap<Comments, CommentResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // Mapping between Photo and PhotoResponse
            CreateMap<Photo, PhotoResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            // Mapping between ApplicationUser and UserResponse
            CreateMap<ApplicationUser, UserResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
            CreateMap<ApplicationUser, UserPostResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            CreateMap<TourTimestamp, TourTimeStampResponse>().ReverseMap()
                .ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

            CreateMap<TourTimeStampRequest, TourTimestamp>()
                .ForMember(dest => dest.PreferredTimeSlot, opt => opt.MapFrom(src => src.PreferredTimeSlot));
            CreateMap<TourTimestamp, TourTimeStampRequest>();

            CreateMap<Location, LocationResponse>()
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos != null ? src.Photos.ToList() : null));
            CreateMap<Photo, PhotoResponseForLocation>();
            CreateMap<Photo, PhotoResponseForPosts>();

            CreateMap<Tour, TourResponse>()
                .ForMember(dest => dest.Transportations, opt => opt.MapFrom(src => src.Transportations))
                .ForMember(dest => dest.TourTimestamps, opt => opt.MapFrom(src => src.TourTimestamps))
                .ForMember(dest => dest.LocationInTours, opt => opt.MapFrom(src => src.LocationInTours.Select(lit => lit.Location)))
                .ForMember(dest => dest.TourMoods, opt => opt.MapFrom(src => src.TourMoods.Select(tm => tm.Mood)))
                .ForMember(dest => dest.TourTrips, opt => opt.MapFrom(src => src.TourTrips));

            CreateMap<TourRequestModel, Tour>()
                .ForMember(dest => dest.TourMoods, opt => opt.Ignore())
                .ForMember(dest => dest.LocationInTours, opt => opt.Ignore())
                .ForMember(dest => dest.Transportations, opt => opt.Ignore())
                .ForMember(dest => dest.TourTrips, opt => opt.Ignore())
                .ForMember(dest => dest.TourTimestamps, opt => opt.Ignore());

            CreateMap<TourTrip, VNPayRequest>();

            CreateMap<Transportation, TransportationResponse>();

            // New mappings for TourPackageDetailsResponse
            CreateMap<Tour, TourPackageDetailsResponse>()
                .ForMember(dest => dest.Moods, opt => opt.MapFrom(src => src.TourMoods.Select(tm => tm.Mood)));

            CreateMap<Tour, TourDetailsResponse>()
                .ForMember(dest => dest.TourTrips, opt => opt.MapFrom(src => src.TourTrips.OrderBy(tt => tt.TripDate)));

            CreateMap<Tour, TourTimeStampDetailsResponse>();

            CreateMap<TourTrip, TourTripDetailsResponse>()
                .ForMember(dest => dest.TripStatus, opt => opt.MapFrom(src => src.TripStatus.ToString()))
                .ForMember(dest => dest.TourTripId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Location, LocationResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address.FullAddress))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Address.Longitude))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Address.Latitude))
                .ForMember(dest => dest.Temperature, opt => opt.MapFrom(src => src.Temperature))
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos));

            CreateMap<LocationsRequest, Location>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new AddressInfo
                {
                    FullAddress = src.Address,
                    Longitude = src.Longitude,
                    Latitude = src.Latitude
                }));

            CreateMap<LocationCreateRequest, Location>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new AddressInfo
                {
                    FullAddress = src.Address,
                    Longitude = src.Longitude,
                    Latitude = src.Latitude
                }));

            CreateMap<Transportation, TransportationResponse>();

            CreateMap<Moods, MoodResponseWithoutTours>();

            CreateMap<Moods, MoodRequest>();
            CreateMap<MoodRequest, Moods>();

            CreateMap<Moods, MoodResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MoodTag, opt => opt.MapFrom(src => src.MoodTag))
                .ForMember(dest => dest.IconName, opt => opt.MapFrom(src => src.IconName));

            CreateMap<TourTrip, TourTripResponse>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.TourTripId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TripStatus, opt => opt.MapFrom(src => src.TripStatus.ToString()));

            CreateMap<TourTripRequest, TourTrip>();

            // New mappings for TourPackageHistoryResponse
            CreateMap<Tour, TourPackageHistoryResponse>()
                .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src =>
                    src.TourTrips.SelectMany(tt => tt.Payments)
                        .Where(p => p.Transaction != null)
                        .Select(p => p.Transaction)));

            CreateMap<Transaction, TransactionResponse>()
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreatedDate));

            CreateMap<Transportation, TransportationResponse>().ReverseMap();
            CreateMap<Transportation, TransportationRequestModel>().ReverseMap();


            // New mappings for ChatBox
            CreateMap<ChatMessage, ChatMessageResponse>()
                .ForMember(dest => dest.ChatRoomId, opt => opt.MapFrom(src => src.ChatRoom.Id))
                .ForMember(dest => dest.SenderName,
                    opt => opt.MapFrom(src => src.Sender.UserName));

            CreateMap<ChatRoom, ChatRoomResponse>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.UserName))
                .ForMember(dest => dest.ModeratorName,
                    opt => opt.MapFrom(src => src.Moderator.UserName));
        }
    }
}