namespace AxisUtah.Api.Features.Authentication.Contracts;

public sealed record LoginRequest(string Email, string Password);