using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using TaskManagement.Core.Common;
using TaskManagement.Core.DTO.Auth;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {

        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;

        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if(!_passwordHasher.verifyPassword(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for deactivated account: {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("User account is deactivated");
            }

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            
            return await GenerateAuthResponse(user, ipAddress);

        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var tokenHash = HashToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == tokenHash);

            if(storedToken == null)
            {
                _logger.LogWarning("Refresh token not found");
                throw new UnauthorizedAccessException("Invalid refresh token");
            }
            if (!storedToken.IsActive)
            {
                _logger.LogWarning("Inactive refresh token used for user: {UserId}", storedToken.UserId);
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var user = storedToken.User;

            // Revoking old refresh token
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;

            // Getting new token
            var newAuthResponse = await GenerateAuthResponse(user, ipAddress);

            // Putting old token as replaced
            storedToken.ReplacedByToken = HashToken(newAuthResponse.RefreshToken);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token rotated for user: {Email}", user.Email);

            return newAuthResponse;
        }



        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email || u.UserName == registerDto.UserName);

            if(existingUser != null)
            {
                if (existingUser.Email == registerDto.Email)
                    throw new InvalidOperationException("A user with this email already exists");
                throw new InvalidOperationException("A user with username already exists");
            }

            var passwordHash = _passwordHasher.HashPassword(registerDto.Password);


            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email}", user.Email);

            _ = _emailService.SendWelcomeEmailAsync(user.Email, user.UserName);

            return await GenerateAuthResponse(user, ipAddress);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);

            if(user == null)
            {
                _logger.LogInformation("Password reset requested for non existant email");
                return true;
            }

            // Invalidating any existing reset tokens for specific user
            var existingTokens = await _context.PasswordResetTokens
                  .Where(t => t.UserId == user.Id && t.IsValid)
                  .ToListAsync();

            foreach( var token in existingTokens)
            {
                token.UsedAt = DateTime.UtcNow;
            }

            var resetToken = GenerateSecureToken();
            var tokenHash = HashToken(resetToken);

            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _context.PasswordResetTokens.Add(passwordResetToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset token generated for user: {Email}", user.Email);

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return true;
        }



        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var tokenHash = HashToken(resetPasswordDto.Token);

            var resetToken = await _con
        }

        public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            var tokenHash = HashToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == tokenHash);

            if (storedToken == null)
            {
                throw new InvalidOperationException("Invalid refresh token");
            }
            if (!storedToken.IsActive)
            {
                throw new InvalidOperationException("Token is already rvoked");
            }

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked for user: {UserId}", storedToken.UserId);
        }


        private async Task<AuthResponseDto> GenerateAuthResponse(User user, object ipAddress)
        {
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
        }
        private static string GenerateSecureToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        private static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);

        }

    }
}
