using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class WhatsAppService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber = "whatsapp:+12702951192";

    public WhatsAppService()
    {
        var config = GetConfiguration();
        _accountSid = config["Twilio:AccountSID"];
        _authToken = config["Twilio:AuthToken"];
    }

    public WhatsAppService(IConfiguration configuration)
    {
        _accountSid = configuration["Twilio:AccountSID"];
        _authToken = configuration["Twilio:AuthToken"];
    }

    private static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        return builder;
    }

    /// <summary>
    /// Envía un mensaje usando una plantilla aprobada de WhatsApp.
    /// </summary>
    public void EnviarMensajePlantilla(string destinatario, string contentSid, Dictionary<string, string> variables)
    {
        TwilioClient.Init(_accountSid, _authToken);

        MessageResource.Create(
            from: new PhoneNumber(_fromNumber),
            to: new PhoneNumber($"whatsapp:{destinatario}"),
            contentSid: contentSid,
            contentVariables: JsonSerializer.Serialize(variables)
        );
    }

    public void EnviarMensajeTexto(string telefono, string mensaje)
    {
        TwilioClient.Init(_accountSid, _authToken);

        MessageResource.Create(
            from: new PhoneNumber(_fromNumber),
            to: new PhoneNumber($"whatsapp:{telefono}"),
            body: mensaje
        );
    }
}