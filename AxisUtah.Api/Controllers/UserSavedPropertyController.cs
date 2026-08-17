namespace AxisUtah.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserSavedPropertyController(PropertyService propertyService) : ControllerBase
{
    private readonly PropertyService _propertyService = propertyService;

    private int? GetCurrentUserId() =>
    int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
        ? userId : null;

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertyResponseDto>>> GetAllSavedProperties()
    {
        int? userId = GetCurrentUserId();
        if (userId == null || userId <= 0)
            return Unauthorized();

        var properties = await _propertyService.GetAllUserSavedProperties(userId);

        return Ok(properties);
    }

    [Authorize]
    [HttpGet("{propertyId}")]
    public async Task<ActionResult<PropertyResponseDto?>> GetSavedPropertyById(int propertyId)
    {
        int? userId = GetCurrentUserId();
        if (userId == null || userId <= 0)
            return Unauthorized();

        var property = await _propertyService.GetUserSavedPropertyById(userId, propertyId);
        if (property == null)
            return NotFound();

        return Ok(property);
    }

    [Authorize]
    [HttpPost("{propertyId}")]
    public async Task<ActionResult> SaveProperty(int propertyId)
    {
        int? userId = GetCurrentUserId();
        if (userId == null || userId <= 0)
            return Unauthorized();

        var result = await _propertyService.SaveUserProperty(userId, propertyId);

        if (!result)
            return BadRequest();

        var savedProperty = await _propertyService.GetUserSavedPropertyById(userId, propertyId);
        if (savedProperty == null)
            return NotFound();

        return Ok(savedProperty);
    }

    [Authorize]
    [HttpPut("{propertyId}")]
    public async Task<ActionResult> DeleteProperty(int propertyId)
    {
        int? userId = GetCurrentUserId();
        if (userId == null || userId <= 0)
            return Unauthorized();

        var result = await _propertyService.DeleteUserSavedProperty(userId, propertyId);

        if (!result)
            return BadRequest();

        return NoContent();
    }
}