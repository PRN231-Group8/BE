using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
	public class PhotoRepository : BaseRepository<Photo>, IPhotoRepository
	{
		private readonly ApplicationDbContext _context;

		public PhotoRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
