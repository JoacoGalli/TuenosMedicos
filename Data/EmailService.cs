using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587; // Puerto para TLS
    private string _emailFrom => GetConfiguration()["SMTP:EmailFrom"];
    private string _emailPassword => GetConfiguration()["SMTP:EmailPassword"];

    private static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return builder;
    }

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
