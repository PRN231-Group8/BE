using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.Repositories.Interface;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
	IUserRepository UserRepository { get; }
	ILocationRepository LocationRepository { get; }
	ITourTimeStampRepository TourTimeStampRepository { get; }
	ITourRepository TourRepository { get; }
	IPostsRepository PostsRepository { get; }
	IPaymentRepository PaymentRepository { get; }
	ITourTripRepository TourTripRepository { get; }
	ITransactionRepository TransactionRepository { get; }
	IMoodRepository MoodRepository { get; }
}