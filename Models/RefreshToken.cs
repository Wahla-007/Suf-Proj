using System.ComponentModel.DataAnnotations;

namespace mess_management.Models
{
    /// <summary>
    /// Represents a refresh token stored in the database
    /// </summary>
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The actual refresh token string
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// User ID this token belongs to
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the user
        /// </summary>
        public AspNetUser? User { get; set; }

        /// <summary>
        /// When this token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// When this token was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this token was revoked (null if not revoked)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Whether this token has been revoked
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Whether this token is currently active (not expired and not revoked)
        /// </summary>
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
