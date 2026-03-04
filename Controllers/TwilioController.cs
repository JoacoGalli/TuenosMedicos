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

            string mensajeForward =
                $"📩 Mensaje de paciente\n\n" +
                $"Nombre: {nombre}\n" +
                $"Tel: {telefonoPaciente}\n\n" +
                $"Mensaje:\n{mensaje}";

            whatsappService.EnviarMensajeTexto(telefonoMedico, mensajeForward);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error forwardeando mensaje al médico");
        }
    }
}