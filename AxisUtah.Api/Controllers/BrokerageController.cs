namespace AxisUtah.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrokerageController(IBrokerageService brokerageService) : ControllerBase
{
    private readonly IBrokerageService _brokerageService = brokerageService;

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<BrokerageResponseDto?> GetBrokerageByIdAsync(int id)
    {
        return await _brokerageService.GetBrokerageByIdAsync(id);
    }
}