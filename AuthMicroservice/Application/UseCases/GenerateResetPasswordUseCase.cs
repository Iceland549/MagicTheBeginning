﻿using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Config;
using AuthMicroservice.Infrastructure.Persistence;
using AuthMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Sends a reset password link to a user by generating a secure token.
    /// </summary>
    public class GenerateResetPasswordUseCase
    {
        private readonly AuthDbContext _context;
        private readonly IEmailService _emailService;
        private readonly SmtpSettings _smtp;

        public GenerateResetPasswordUseCase(
            AuthDbContext context,
            IEmailService emailService,
            IOptions<SmtpSettings> smtpOptions)
        {
            _context = context;
            _emailService = emailService;
            _smtp = smtpOptions.Value;
        }

        /// <summary>
        /// Generates a reset token and sends the link to the user.
        /// </summary>
        /// <param name="email">User's email requesting a reset.</param>
        public async Task ExecuteAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new KeyNotFoundException("User not found.");

            var token = Guid.NewGuid().ToString("N");

            _context.EmailTokens.Add(new EmailToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1),
                Type = EmailTokenType.ResetPassword
            });

            await _context.SaveChangesAsync();

            var link = $"{_smtp.FrontendUrl}/reset-password?token={token}";
            var body = $"<p>Hello,<br/>Click <a href='{link}'>here</a> to reset your password.</p>";
            await _emailService.SendEmailAsync(email, "Reset your password", body);
        }
    }
}