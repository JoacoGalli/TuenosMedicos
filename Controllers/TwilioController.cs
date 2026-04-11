using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.RegularExpressions;

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
            var body = Request.Form["Body"].ToString().ToLowerInvariant();
            var profileName = Request.Form["ProfileName"].FirstOrDefault() ?? "Paciente";

            var telefonoPaciente = from.Replace("whatsapp:", "");

            Log.Information($"WhatsApp recibido de {telefonoPaciente}: {body}");

            // -------- Detectar confirmación/cancelacion --------
            if (ContainsConfirmKeyword(body))
            {
                ConfirmarTurnoAutomaticamente(telefonoPaciente);
            }
            else if (ContainsCancelKeyword(body))
            {
                CancelarTurnoAutomaticamente(telefonoPaciente);
            }
            else
            {
                EnviarMensajeSoloBot(telefonoPaciente);
            }

            // -------- Forward al médico (sigue funcionando igual) --------
            ForwardearMensajeAlMedico(telefonoPaciente, profileName, body);

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error en webhook Twilio");
            return StatusCode(500);
        }
    }

    private void ConfirmarTurnoAutomaticamente(string telefono)
    {
        try
        {
            string query = @"
            SELECT * FROM turnos
            WHERE telefono=@telefono
            AND cancelado=false
            AND confirmado=false 
            AND confirmadoPorLink=false
            AND fechaTurno >= CURDATE()
            ORDER BY fechaTurno ASC
            LIMIT 1";

            var parametros = new Dictionary<string, object>
            {
                { "@telefono", telefono }
            };

            var turnos = Base.SelectATurnos(query, parametros);

            if (turnos == null || turnos.Count == 0)
            {
                Log.Information("No hay turnos pendientes para confirmar para {Telefono}", telefono);

                var w = new WhatsAppService();
                w.EnviarMensajeTexto(telefono,
                    "No encontramos un turno pendiente asociado a este número.");

                return;
            }

            var turno = turnos.First();

            string update = "UPDATE turnos SET confirmadoPorLink=true WHERE idTurno=@idTurno";

            var p2 = new Dictionary<string, object>
            {
                { "@idTurno", turno.Id }
            };

            int updated = Base.InsertDeleteOrUpdateABase(update, p2);

            if (updated > 0)
            {
                try
                {
                    var whatsappService = new WhatsAppService();

                    var contentSid = "HX76b8aba9378dda6c750985f18e1bf8f3"; // <- SID real de confirmacion_turno

                    var variables = new Dictionary<string, string>
                        {
                            { "1", $"{turno.NombrePaciente}" },
                            { "2", turno.Medico },
                            { "3", turno.FechaTurno?.ToString("dd/MM/yyyy") ?? "" },
                            { "4", turno.HoraTurno },
                        };

                    whatsappService.EnviarMensajePlantilla(telefono, contentSid, variables);
                    Log.Information("Turno {Id} confirmado automáticamente por WhatsApp.", turno.Id);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al enviar el WhatsApp para: " + telefono);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error confirmando turno automáticamente");
        }
    }

    private bool ContainsConfirmKeyword(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        var palabras = new[]
        {
            "confirmo",
            "confirmar",
            "confirmado",
            "confirmación",
            "confirmar turno"
        };

        foreach (var p in palabras)
        {
            if (Regex.IsMatch(text,
                $@"\b{Regex.Escape(p)}\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return true;
        }

        return false;
    }

    private void CancelarTurnoAutomaticamente(string telefono)
    {
        try
        {
            string query = @"
        SELECT * FROM turnos
        WHERE telefono=@telefono
        AND cancelado=false
        AND fechaTurno >= CURDATE()
        ORDER BY fechaTurno ASC
        LIMIT 1";

            var parametros = new Dictionary<string, object>
        {
            { "@telefono", telefono }
        };

            var turnos = Base.SelectATurnos(query, parametros);

            if (turnos == null || turnos.Count == 0)
            {
                var w = new WhatsAppService();
                w.EnviarMensajeTexto(telefono,
                    "No encontramos un turno activo para cancelar.");

                return;
            }

            var turno = turnos.First();

            string update = @"
        UPDATE turnos 
        SET cancelado=true,
            motivoCancelacion='Cancelado por WhatsApp'
        WHERE idTurno=@idTurno";

            var p2 = new Dictionary<string, object>
        {
            { "@idTurno", turno.Id }
        };

            int updated = Base.InsertDeleteOrUpdateABase(update, p2);

            if (updated > 0)
            {
                try
                {
                    var whatsappService = new WhatsAppService();

                    var contentSid = "HX49f55b2d0b0224e9d9fe3e7b48c0546f"; // <- SID real de confirmacion_turno

                    var variables = new Dictionary<string, string>
                        {
                            { "1", $"{turno.NombrePaciente}" },
                            { "2", turno.Medico },
                            { "3", turno.FechaTurno?.ToString("dd/MM/yyyy") ?? "" },
                            { "4", turno.HoraTurno },
                        };

                    whatsappService.EnviarMensajePlantilla(telefono, contentSid, variables);
                    Log.Information("Turno {Id} confirmado automáticamente por WhatsApp.", turno.Id);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al enviar el WhatsApp para: " + telefono);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error cancelando turno automáticamente");
        }
    }

    private bool ContainsCancelKeyword(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        var palabras = new[]
        {
        "cancelo",
        "cancelar",
        "cancelado",
        "cancelación",
        "cancelar turno"
    };

        foreach (var p in palabras)
        {
            if (Regex.IsMatch(text,
                $@"\b{Regex.Escape(p)}\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return true;
        }

        return false;
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
    private void EnviarMensajeSoloBot(string telefono)
    {
        try
        {
            var whatsappService = new WhatsAppService();

            var contentSid = "HXe751cfe5e99aab17b7f7df1e40b58672";

            var variables = new Dictionary<string, string>
        {
            { "1", "+5492227402738" }
        };

            whatsappService.EnviarMensajePlantilla(telefono, contentSid, variables);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error enviando mensaje de bot");
        }
    }
}

