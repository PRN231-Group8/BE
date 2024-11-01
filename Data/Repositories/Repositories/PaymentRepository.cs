using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
	public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
	{
		private readonly ApplicationDbContext _context;

		public PaymentRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
