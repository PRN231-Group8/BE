using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
	{
		private readonly ApplicationDbContext _context;

		public TransactionRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
