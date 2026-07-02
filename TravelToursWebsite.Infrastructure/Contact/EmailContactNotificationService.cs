using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using TravelToursWebsite.Application.Features.Contact;
using TravelToursWebsite.Domain.Configuration;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.Contact;

public sealed class EmailContactNotificationService(
    ApplicationDbContext context,
    IOptions<EmailSettings> options,
    ILogger<EmailContactNotificationService> logger)
    : IContactNotificationService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendContactInquiryConfirmationAsync(
        ContactInquiry inquiry,
        CancellationToken cancellationToken = default)
    {
        if (!CanSendEmail())
        {
            logger.LogInformation("Skipping contact inquiry confirmation email because SMTP settings are incomplete.");
            return;
        }

        var senderEmail = await GetSettingValueAsync("Contact.Email", _settings.SmtpUsername, cancellationToken);
        var companyName = await GetSettingValueAsync("Contact.CompanyName", "Viaitalia SRL", cancellationToken);
        var subject = $"Thank You for Your Inquiry - {inquiry.Subject}";
        var body = BuildContactInquiryEmail(inquiry, companyName);

        await SendEmailAsync(inquiry.Email, subject, body, senderEmail, cancellationToken);
    }

    public async Task SendBookingRequestConfirmationAsync(
        BookingRequest booking,
        CancellationToken cancellationToken = default)
    {
        if (!CanSendEmail())
        {
            logger.LogInformation("Skipping booking confirmation email because SMTP settings are incomplete.");
            return;
        }

        var senderEmail = await GetSettingValueAsync("Contact.Email", _settings.SmtpUsername, cancellationToken);
        var companyName = await GetSettingValueAsync("Contact.CompanyName", "Viaitalia SRL", cancellationToken);
        var tourTitle = booking.Tour?.Translations.FirstOrDefault()?.Title ?? $"Tour #{booking.TourId}";
        var subject = $"Tour Booking Confirmation - {tourTitle}";
        var body = BuildBookingEmail(booking, companyName, tourTitle);

        await SendEmailAsync(booking.Email, subject, body, senderEmail, cancellationToken);
    }

    private bool CanSendEmail()
    {
        return !string.IsNullOrWhiteSpace(_settings.SmtpServer)
            && !string.IsNullOrWhiteSpace(_settings.SmtpUsername)
            && !string.IsNullOrWhiteSpace(_settings.SmtpPassword);
    }

    private async Task<string> GetSettingValueAsync(
        string key,
        string fallback,
        CancellationToken cancellationToken)
    {
        return await context.SiteSettings
            .AsNoTracking()
            .Where(setting => setting.IsActive && setting.Key == key)
            .Select(setting => setting.Value)
            .FirstOrDefaultAsync(cancellationToken)
            ?? fallback;
    }

    private async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        string from,
        CancellationToken cancellationToken)
    {
        using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        message.To.Add(new MailAddress(to));
        await client.SendMailAsync(message, cancellationToken);
    }

    private static string BuildContactInquiryEmail(ContactInquiry inquiry, string companyName)
    {
        return $"""
            <!doctype html>
            <html lang="en">
            <body style="font-family:Arial,sans-serif;line-height:1.6;color:#263238">
              <h1>Thank You for Contacting Us</h1>
              <p>Dear {Escape(inquiry.Name)},</p>
              <p>We received your inquiry and will get back to you soon.</p>
              <h2>Your Inquiry Details</h2>
              <p><strong>Subject:</strong> {Escape(inquiry.Subject)}</p>
              <p><strong>Email:</strong> {Escape(inquiry.Email)}</p>
              <p><strong>Phone:</strong> {Escape(inquiry.Phone)}</p>
              <p><strong>Message:</strong></p>
              <p>{Escape(inquiry.Message).Replace(Environment.NewLine, "<br>")}</p>
              <p>Regards,<br>{Escape(companyName)}</p>
            </body>
            </html>
            """;
    }

    private static string BuildBookingEmail(BookingRequest booking, string companyName, string tourTitle)
    {
        var estimatedTotal = booking.EstimatedTotal.HasValue
            ? booking.EstimatedTotal.Value.ToString("0.##")
            : "N/A";

        return $"""
            <!doctype html>
            <html lang="en">
            <body style="font-family:Arial,sans-serif;line-height:1.6;color:#263238">
              <h1>Tour Booking Confirmation</h1>
              <p>Dear {Escape(booking.Name)},</p>
              <p>Thank you for your booking request. We will contact you soon to confirm the details.</p>
              <h2>Booking Details</h2>
              <p><strong>Tour:</strong> {Escape(tourTitle)}</p>
              <p><strong>Travelers:</strong> {booking.NumberOfTravelers}</p>
              <p><strong>Preferred Travel Date:</strong> {booking.PreferredTravelDate:yyyy-MM-dd}</p>
              <p><strong>Estimated Total:</strong> {estimatedTotal}</p>
              <p><strong>Special Requests:</strong> {Escape(booking.SpecialRequests)}</p>
              <p>Regards,<br>{Escape(companyName)}</p>
            </body>
            </html>
            """;
    }

    private static string Escape(string? value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
