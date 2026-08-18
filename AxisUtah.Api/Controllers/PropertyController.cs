namespace AxisUtah.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController(PropertyService propertyService) : ControllerBase
{
    private readonly PropertyService _propertyService = propertyService;

    [Authorize]
    [HttpGet("{propertyid}")]
    public async Task<IActionResult> GetPropertyById(int propertyid)
    {
        var property = await _propertyService.GetPropertyById(propertyid);
        if (property == null)
        {
            return NotFound();
        }
        return Ok(property);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedResult<PropertyResponseDto>>> GetProperties([FromQuery] PropertySearchRequest request)
    {
        var result = await _propertyService.SearchAsync(request);
        if (result.Items.Count == 0)
        {
            return NotFound();
        }
        return Ok(result);
    }
}