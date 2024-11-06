using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.Repositories.Interface;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks;

public class UnitOfWork : BaseUnitOfWork<ApplicationDbContext>, IUnitOfWork
{
    public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }
    public IUserRepository UserRepository => GetRepository<IUserRepository>();
    public ITourRepository TourRepository => GetRepository<ITourRepository>();
    public IMoodRepository MoodRepository => GetRepository<IMoodRepository>();
    public ILocationRepository LocationRepository => GetRepository<ILocationRepository>();
    public ITourTimeStampRepository TourTimeStampRepository => GetRepository<ITourTimeStampRepository>();
    public IPostsRepository PostsRepository => GetRepository<IPostsRepository>();
    public ITransportationRepository TransportationRepository => GetRepository<ITransportationRepository>();
	public ICommentRepository CommentRepository => GetRepository<ICommentRepository>();
	public IPhotoRepository PhotoRepository => GetRepository<IPhotoRepository>();
}