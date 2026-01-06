namespace mess_management.Models
{
    /// <summary>
    /// Response model for login endpoint
    /// </summary>
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserInfo User { get; set; } = new UserInfo();
    }

    /// <summary>
    /// Response model for token refresh endpoint
    /// </summary>
    public class RefreshResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// User information included in authentication responses
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }

    /// <summary>
    /// Request model for token refresh
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for token revocation
    /// </summary>
    public class RevokeTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
