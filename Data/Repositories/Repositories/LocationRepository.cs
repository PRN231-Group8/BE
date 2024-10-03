using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interface;

namespace PRN231.ExploreNow.Repositories.Repositories
{
    public class LocationRepository : BaseRepository<Location>, ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
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
            return locations.Select(MapToDto).ToList();
        }

        public async Task<LocationResponse> GetByIdAsync(Guid id)
        {
            var location = await GetQueryable(l => l.Id == id && !l.IsDeleted)
                                .Include(l => l.Photos)
                                .FirstOrDefaultAsync();
            return location == null ? null : MapToDto(location);
        }

        public async Task<LocationResponse> CreateAsync(Location location)
        {
            Add(location);
            await SaveChangesAsync();
            return MapToDto(location);
        }

        public async Task<LocationResponse> UpdateAsync(Location location)
        {
            var existingLocation = await GetQueryable(l => l.Id == location.Id && !l.IsDeleted)
                .Include(l => l.Photos)
                .FirstOrDefaultAsync();
            if (existingLocation == null)
            {
                return null;
            }
            UpdateLocationProperties(existingLocation, location);
            Update(existingLocation);
            await SaveChangesAsync();
            return MapToDto(existingLocation);
        }

        private void UpdateLocationProperties(Location existingLocation, Location newLocation)
        {
            existingLocation.Name = newLocation.Name;
            existingLocation.Description = newLocation.Description;
            existingLocation.Address = newLocation.Address;
            existingLocation.Status = newLocation.Status;
            existingLocation.Temperature = newLocation.Temperature;
            if (newLocation.Photos != null && newLocation.Photos.Any())
            {
                foreach (var photo in existingLocation.Photos)
                {
                    photo.IsDeleted = true;
                }
                //_context.Photos.RemoveRange(existingLocation.Photos);
                existingLocation.Photos = newLocation.Photos.Select(p => new Photo
                {
                    Url = p.Url,
                    Alt = p.Alt,
                    Code = p.Code ?? GenerateUniqueCode(),
                    CreatedBy = "admin",
                    CreatedDate = DateTime.Now,
                    LastUpdatedBy = "admin",
                    IsDeleted = false
                }).ToList();
            }
        }

        private LocationResponse MapToDto(Location location)
        {
            return new LocationResponse
            {
                Id = location.Id,
                Name = location.Name,
                Description = location.Description,
                Address = location.Address,
                Status = location.Status.ToString(),
                Temperature = location.Temperature,
                Photos = location.Photos
                    .Where(p => !p.IsDeleted)
                    .Select(p => new PhotoResponse
                {
                    Id = p.Id,
                    Url = p.Url,
                    Alt = p.Alt
                }).ToList()
            };
        }

        private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    }
}