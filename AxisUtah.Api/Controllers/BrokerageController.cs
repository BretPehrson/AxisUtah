namespace AxisUtah.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrokerageController(IBrokerageService brokerageService) : ControllerBase
{
    private readonly IBrokerageService _brokerageService = brokerageService;

    [Authorize]
    [HttpGet]
    public async Task<BrokerageResponseDto?> GetBrokerageByIdAsync(int id)
    {
        return await _brokerageService.GetBrokerageByIdAsync(id);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<BrokerageResponseDto?> UpdateBrokerageAsync(int id, BrokerageResponseDto brokerageDto)
    {
        return await _brokerageService.UpdateBrokerageAsync(id, brokerageDto);
    }
}