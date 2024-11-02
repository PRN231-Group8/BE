using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interface;

namespace PRN231.ExploreNow.Repositories.Repositories
{
    public class LocationRepository : BaseRepository<Location>, ILocationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;


        public LocationRepository(ApplicationDbContext context, IConfiguration configuration, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            var account = new Account(
                _configuration["CloudinarySetting:CloudName"],
                _configuration["CloudinarySetting:ApiKey"],
                _configuration["CloudinarySetting:ApiSecret"]);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<List<LocationResponse>> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm)
        {
            var query = GetQueryable()
                .Include(l => l.Photos)
                .Where(l => !l.IsDeleted);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.Name.Contains(searchTerm) || l.Description.Contains(searchTerm));
            }

            if (sortByStatus.HasValue)
            {
                query = query.OrderBy(l => l.Status == sortByStatus.Value);
            }

            var locations = await query.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (_mapper == null)
            {
                throw new InvalidOperationException("Mapper is not initialized");
            }

            return _mapper.Map<List<LocationResponse>>(locations);
        }
    }
}