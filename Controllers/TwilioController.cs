using Microsoft.AspNetCore.Mvc;
using Serilog;

[ApiController]
[Route("twilio")]
public class TwilioController : ControllerBase
{
    [HttpPost("incoming-whatsapp")]
    public IActionResult IncomingWhatsApp()
    {
        try
        {
            Log.Information("Webhook de Twilio llamado");

            var from = Request.Form["From"].ToString();
            var body = Request.Form["Body"].ToString();
            var profileName = Request.Form["ProfileName"].FirstOrDefault() ?? "Paciente";

            var telefonoPaciente = from.Replace("whatsapp:", "");

            Log.Information($"WhatsApp recibido de {telefonoPaciente}: {body}");

            ForwardearMensajeAlMedico(telefonoPaciente, profileName, body);

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error en webhook Twilio");
            return StatusCode(500);
        }
    }

    private void ForwardearMensajeAlMedico(string telefonoPaciente, string nombre, string mensaje)
    {
        try
        {
            var whatsappService = new WhatsAppService();

            string telefonoMedico = "+5492227402738";

            var contentSid = "HX418bbdb5d3fbc1d4f14c0f8a296560c1";

            var variables = new Dictionary<string, string>
        {
            { "1", nombre },
            { "2", telefonoPaciente },
            { "3", mensaje }
        };

            whatsappService.EnviarMensajePlantilla(telefonoMedico, contentSid, variables);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error forwardeando mensaje al médico");
        }
    }
}