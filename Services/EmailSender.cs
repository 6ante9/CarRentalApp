using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Simulacija slanja emaila (trenutno samo ispisuje u konzolu)
        Console.WriteLine($"[EmailSender] Email sent to {email} with subject: {subject}");
        return Task.CompletedTask;
    }
}
