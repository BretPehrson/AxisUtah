namespace AxisUtah.Api.Models;

public enum UserRole
{
    Admin,
    User
}

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public List<SavedProperty> SavedProperties { get; set; } = [];

    public UserInfo? UserInfo { get; set; }

    public Agent? AgentProfile { get; set; }
}