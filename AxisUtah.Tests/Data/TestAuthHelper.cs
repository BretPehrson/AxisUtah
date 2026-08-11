namespace AxisUtah.Tests.Data;

public static class TestAuthHelper
{
    public static ControllerContext GetControllerContext(int? userId, Dictionary<string, string>? requestCookies = null)
    {
        var user = new List<Claim> 
        {
             new(JwtRegisteredClaimNames.Sub, userId?.ToString() ?? string.Empty),
             new(ClaimTypes.NameIdentifier, userId?.ToString() ?? string.Empty)
        };
        
        var identity = new ClaimsIdentity(user, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        // Set request cookies if provided
        if (requestCookies != null && requestCookies.Count > 0)
        {
            // Build cookie header string: "name1=value1; name2=value2"
            var cookieHeaderValue = string.Join("; ", requestCookies.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            httpContext.Request.Headers.Cookie = cookieHeaderValue;
        }

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    /// Extracts a cookie value from a Set-Cookie response header
    /// </summary>
    public static string ExtractCookieValue(string setCookieHeader, string cookieName)
    {
        var pattern = $"{cookieName}=";
        var startIndex = setCookieHeader.IndexOf(pattern);
        if (startIndex == -1) return string.Empty;
        
        startIndex += pattern.Length;
        var endIndex = setCookieHeader.IndexOf(";", startIndex);
        if (endIndex == -1) endIndex = setCookieHeader.Length;
        
        return setCookieHeader.Substring(startIndex, endIndex - startIndex);
    }
}