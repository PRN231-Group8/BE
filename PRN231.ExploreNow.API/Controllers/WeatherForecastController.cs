using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.API.Controllers;

[ApiController]
[Route("api/weather-forecast")]
// [ApiExplorerSettings(IgnoreApi = true)]  // Cai nay se giau tren swagger khi demo
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ICacheService _cacheService;

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var cacheData = GetKeyValues(); // lay key values
        if (cacheData.Any()) return cacheData.Values; // neu co lay du lieu tren cache

        var newData = Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();

        await Save(newData).ConfigureAwait(false);
        return newData;
    }

    [HttpGet(nameof(WeatherForecast))]
    public WeatherForecast Get(int id)
    {
        var cacheData = GetKeyValues();
        cacheData.TryGetValue(id, out var filteredData);

        return filteredData;
    }

    [HttpPost("addWeatherForecasts/{durationMinutes}")]
    public async Task<WeatherForecast[]> PostList(WeatherForecast[] values, int durationMinutes)
    {
        var cacheData = GetKeyValues();
        foreach (var value in values) cacheData[value.Id] = value;
        await Save(cacheData.Values, durationMinutes).ConfigureAwait(false);
        return values;
    }

    [HttpPost("addWeatherForecast")]
    public async Task<WeatherForecast> Post(WeatherForecast value)
    {
        var cacheData = GetKeyValues();
        cacheData[value.Id] = value;
        await Save(cacheData.Values).ConfigureAwait(false);
        return value;
    }

    [HttpPut("updateWeatherForecast")]
    public async void Put(WeatherForecast WeatherForecast)
    {
        var cacheData = GetKeyValues();
        cacheData[WeatherForecast.Id] = WeatherForecast;
        await Save(cacheData.Values).ConfigureAwait(false);
    }

    [HttpDelete("deleteWeatherForecast")]
    public async Task Delete(int id)
    {
        var cacheData = GetKeyValues();
        cacheData.Remove(id);
        await Save(cacheData.Values).ConfigureAwait(false);
    }

    [HttpDelete("ClearAll")]
    public async Task Delete()
    {
        await _cacheService.ClearAsync();
    }

    private Task<bool> Save(IEnumerable<WeatherForecast> weatherForecasts, double expireAfterSeconds = 30)
    {
        var expirationTime = DateTimeOffset.Now.AddSeconds(expireAfterSeconds); 
        return _cacheService.AddOrUpdateAsync(nameof(WeatherForecast), weatherForecasts, expirationTime); // khoi tao key hoac luu value trong key trong cache 30 giay
    }

    private Dictionary<int, WeatherForecast> GetKeyValues()
    {
        var data = _cacheService.Get<IEnumerable<WeatherForecast>>(nameof(WeatherForecast)); // dat ten key
        return data?.ToDictionary(key => key.Id, val => val) ?? new Dictionary<int, WeatherForecast>();
    }
}