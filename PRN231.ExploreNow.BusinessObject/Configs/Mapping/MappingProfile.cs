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
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

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
		}
	}
}