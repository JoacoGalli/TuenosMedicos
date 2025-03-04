using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587; // Puerto para TLS
    private readonly string _emailFrom = Environment.GetEnvironmentVariable("SMTP_EMAIL");
    private readonly string _emailPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje)
    {
        using var smtp = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_emailFrom, _emailPassword),
            EnableSsl = true
        };

        var mail = new MailMessage(_emailFrom, destinatario, asunto, mensaje)
        {
            IsBodyHtml = true // Si deseas enviar HTML
        };

        await smtp.SendMailAsync(mail);
    }
}
