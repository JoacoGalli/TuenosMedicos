using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class WhatsAppService
{
    private string _accountSid => GetConfiguration()["Twilio:AccountSID"];
    private string _authToken => GetConfiguration()["Twilio:AuthToken"];
    private string _fromNumber => "whatsapp:+12702951192"; // Numero alquilado en Twilio

    private static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        return builder;
    }

    /// <summary>
    /// Envía un mensaje usando una plantilla aprobada de WhatsApp.
    /// </summary>
    public void EnviarMensajePlantilla(string destinatario, string contentSid, Dictionary<string, string> variables)
    {
        TwilioClient.Init(_accountSid, _authToken);

        var message = MessageResource.Create(
            from: new PhoneNumber(_fromNumber),
            to: new PhoneNumber($"whatsapp:{destinatario}"),
            contentSid: contentSid,
            contentVariables: JsonSerializer.Serialize(variables)
        );
    }

    public void EnviarMensajeTexto(string telefono, string mensaje)
    {
        MessageResource.Create(
            from: new Twilio.Types.PhoneNumber("whatsapp:+12702951192"),
            to: new Twilio.Types.PhoneNumber($"whatsapp:{telefono}"),
            body: mensaje
        );
    }

}
