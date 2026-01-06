namespace mess_management.Models
{
    /// <summary>
    /// JWT configuration settings loaded from appsettings.json
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key used for signing JWT tokens
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Token issuer (typically the application name)
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Token audience (typically the application name or client identifier)
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time in minutes (default: 60 minutes)
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Refresh token expiration time in days (default: 7 days)
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
