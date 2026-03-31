using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using QuizGame.Core.Interfaces;

namespace QuizGame.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["Email:SenderName"],
            _configuration["Email:SenderEmail"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Email:SmtpHost"],
            int.Parse(_configuration["Email:SmtpPort"]!),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            _configuration["Email:SmtpUsername"],
            _configuration["Email:SmtpPassword"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendChallengeNotificationAsync(string toEmail, string challengerUsername, int challengeId)
    {
        var subject = $"{challengerUsername} has challenged you to a quiz!";
        var body = $"""
            <h2>You've been challenged!</h2>
            <p><strong>{challengerUsername}</strong> has challenged you to a quiz on QuizGame.</p>
            <p>Log in to accept or decline the challenge.</p>
            <p>Challenge ID: {challengeId}</p>
            """;

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendQuizAbandonedReminderAsync(string toEmail, string username, int quizId, bool isFinalWarning)
    {
        var subject = isFinalWarning
            ? "Your quiz will be deleted tomorrow!"
            : "You have an unfinished quiz!";

        var body = isFinalWarning
            ? $"""
                <h2>Final Warning!</h2>
                <p>Hi <strong>{username}</strong>,</p>
                <p>Your quiz (ID: {quizId}) will be permanently deleted in 24 hours.</p>
                <p>Log in now to complete it before it's gone!</p>
                <p><em>Note: Unfinished quizzes are automatically deleted after 7 days.</em></p>
                """
            : $"""
                <h2>Don't forget your quiz!</h2>
                <p>Hi <strong>{username}</strong>,</p>
                <p>You have an unfinished quiz (ID: {quizId}) waiting for you.</p>
                <p>Log in to complete it. Unfinished quizzes are deleted after 7 days.</p>
                """;

        await SendEmailAsync(toEmail, subject, body);
    }
}
