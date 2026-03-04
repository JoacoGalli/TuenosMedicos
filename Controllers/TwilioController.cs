using Microsoft.AspNetCore.Mvc;
using Serilog;

[ApiController]
[Route("twilio")]
public class TwilioController : ControllerBase
{
    [HttpPost("incoming-whatsapp")]
    public IActionResult IncomingWhatsApp()
    {
        var from = Request.Form["From"].ToString();
        var body = Request.Form["Body"].ToString();
        var profileName = Request.Form["ProfileName"].ToString();

        var telefonoPaciente = from.Replace("whatsapp:", "");

        Log.Information($"WhatsApp recibido de {telefonoPaciente}: {body}");

        ForwardearMensajeAlMedico(telefonoPaciente, profileName, body);

        return Ok();
    }

    private void ForwardearMensajeAlMedico(string telefonoPaciente, string nombre, string mensaje)
    {
        try
        {
            var whatsappService = new WhatsAppService();

            string telefonoMedico = "+5492227522637"; // número del médico

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
