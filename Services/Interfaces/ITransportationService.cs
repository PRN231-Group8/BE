using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface ITransportationService
    {
        Task<(List<TransportationResponse> Items, int TotalElements)> GetTransportations(int page, int pageSize, string? sortBy, string? searchTerm);
        Task<TransportationResponse> GetTransportationById(Guid id);
        Task<bool> AddTransportation(TransportationRequestModel req);
        Task<bool> UpdateTransportation(Guid id, TransportationRequestModel req);
        Task<bool> DeleteTransportation(Guid id);
    }
}
