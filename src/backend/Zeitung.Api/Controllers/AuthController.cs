using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zeitung.Api.DTOs;
using Zeitung.Api.Services;

namespace Zeitung.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMagicLinkService _magicLinkService;
        private readonly IAuthService _authService;

        public AuthController(IMagicLinkService magicLinkService, IAuthService authService)
        {
            _magicLinkService = magicLinkService;
            _authService = authService;
        }

        /// <summary>Request a magic link for passwordless authentication.</summary>
        [HttpPost("login")]
        public async Task<ActionResult> RequestMagicLink([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            var token = await _magicLinkService.GenerateMagicLinkAsync(request.Email);

            // In production, send this token via email. For tests return it.
            return Ok(new { message = "Magic link generated", token });
        }

        /// <summary>Verify magic link and receive authentication tokens.</summary>
        [HttpPost("verify")]
        public async Task<ActionResult<AuthResponse>> VerifyMagicLink([FromBody] VerifyMagicLinkRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
            {
                return BadRequest(new { error = "Token is required" });
            }

            var (isValid, email) = await _magicLinkService.ValidateMagicLinkAsync(request.Token);

            if (!isValid || email == null)
            {
                return Unauthorized();
            }

            var (accessToken, refreshToken, expiresAt) = await _authService.AuthenticateAsync(email);

            return Ok(new AuthResponse(accessToken, refreshToken, expiresAt));
        }

        /// <summary>Refresh an expired access token using refresh token.</summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token is required" });
            }

            var (isValid, accessToken, refreshToken, expiresAt) = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!isValid || accessToken == null || refreshToken == null || expiresAt == null)
            {
                return Unauthorized();
            }

            return Ok(new AuthResponse(accessToken, refreshToken, expiresAt.Value));
        }

        /// <summary>Revoke a refresh token (logout).</summary>
        [HttpPost("revoke")]
        public async Task<ActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token is required" });
            }

            await _authService.RevokeRefreshTokenAsync(request.RefreshToken);

            return Ok(new { message = "Token revoked successfully" });
        }
    }
}