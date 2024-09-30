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
    public class LocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LocationResponse>> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm)
        {
            var query = _context.Locations
                                .Where(l => !l.IsDeleted)
                                .Include(l => l.Photos)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.Name.Contains(searchTerm) || l.Description.Contains(searchTerm));
            }

            if (sortByStatus.HasValue)
            {
                query = query.OrderBy(l => l.Status == sortByStatus.Value);
            }

            var totalRecords = await query.CountAsync();
            var locations = await query.Skip((page - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            return locations.Select(MapToDto).ToList();
        }

        public async Task<LocationResponse> GetByIdAsync(Guid id)
        {
            var location = await _context.Locations
                                         .Where(l => !l.IsDeleted && l.Id == id)
                                         .Include(l => l.Photos)
                                         .FirstOrDefaultAsync();

            return location == null ? null : MapToDto(location);
        }

        public async Task<LocationResponse> CreateAsync(Location location)
        {
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();

            return MapToDto(location);
        }

        public async Task<LocationResponse> UpdateAsync(Location location)
        {
            var existingLocation = await _context.Locations
                                                 .Include(l => l.Photos)
                                                 .FirstOrDefaultAsync(l => l.Id == location.Id);

            if (existingLocation == null)
            {
                return null;
            }

            UpdateLocationProperties(existingLocation, location);

            _context.Locations.Update(existingLocation);
            await _context.SaveChangesAsync();

            return MapToDto(existingLocation);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return false;

            location.IsDeleted = true;
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();

            return true;
        }

        private void UpdateLocationProperties(Location existingLocation, Location newLocation)
        {
            existingLocation.Name = newLocation.Name;
            existingLocation.Description = newLocation.Description;
            existingLocation.Address = newLocation.Address;
            existingLocation.Status = newLocation.Status;
            existingLocation.Temperature = newLocation.Temperature;

            existingLocation.Photos = newLocation.Photos.Select(p => new Photo
            {
                Url = p.Url,
                Alt = p.Alt,
                Code = p.Code ?? GenerateUniqueCode(),
                CreatedBy = "admin",
                CreatedDate = DateTime.Now,
                LastUpdatedBy = "admin"
            }).ToList();
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
                Photos = location.Photos.Select(p => new PhotoResponse
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
