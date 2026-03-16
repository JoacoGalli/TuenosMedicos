using Microsoft.AspNetCore.Mvc;
using Twilio.Security;
using Serilog;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly IConfiguration _config;

    public WhatsAppWebhookController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Incoming()
    {
        var form = await Request.ReadFormAsync();

        // -------- Validación de firma Twilio --------
        var authToken = _config["Twilio:AuthToken"];
        var signature = Request.Headers["X-Twilio-Signature"].FirstOrDefault() ?? "";

        var validator = new RequestValidator(authToken);

        var formDict = new Dictionary<string, string>();
        foreach (var kv in form)
            formDict[kv.Key] = kv.Value;

        var url = $"{Request.Scheme}://{Request.Host}{Request.Path}";

        if (!validator.Validate(url, formDict, signature))
        {
            Log.Warning("Webhook Twilio: firma inválida.");
            return Forbid();
        }

        // -------- Datos del mensaje --------
        var fromRaw = form["From"].ToString();
        var body = form["Body"].ToString().ToLowerInvariant();
        var messageSid = form["MessageSid"].ToString();

        var phone = NormalizePhone(fromRaw);

        Log.Information("WhatsApp inbound {MessageSid} from {Phone}: {Body}",
                        messageSid, phone, body);

        // -------- Detectar palabras de confirmación --------
        if (!ContainsConfirmKeyword(body))
        {
            Log.Information("Mensaje sin palabra de confirmación.");
            return Ok();
        }

        // -------- Buscar turno pendiente --------
        string query = @"
        SELECT * FROM turnos
        WHERE telefono=@telefono
        AND cancelado=false
        AND confirmado=false
        AND fechaTurno >= CURDATE()
        ORDER BY fechaTurno ASC
        LIMIT 1";

        var parametros = new Dictionary<string, object>
        {
            { "@telefono", phone }
        };

        var turnos = Base.SelectATurnos(query, parametros);

        if (turnos == null || turnos.Count == 0)
        {
            Log.Warning("No se encontró turno pendiente para {Phone}", phone);

            var w1 = new WhatsAppService();
            w1.EnviarMensajeTexto(phone,
                "No encontramos un turno pendiente asociado a este número. Si crees que es un error, comunicate con el consultorio.");

            return Ok();
        }

        var turno = turnos.First();

        // -------- Confirmar turno --------
        string update = "UPDATE turnos SET confirmado=true WHERE idTurno=@idTurno";

        var p2 = new Dictionary<string, object>
        {
            { "@idTurno", turno.Id }
        };

        int updated = Base.InsertDeleteOrUpdateABase(update, p2);

        if (updated > 0)
        {
            Log.Information("Turno {Id} confirmado automáticamente por WhatsApp.", turno.Id);

            var whatsapp = new WhatsAppService();

            whatsapp.EnviarMensajeTexto(phone,
                $"Perfecto 👍 Tu turno del {turno.FechaTurno:dd/MM/yyyy} a las {turno.HoraTurno} quedó confirmado.");

            return Ok();
        }
        else
        {
            Log.Error("Error al actualizar turno {Id}", turno.Id);
            return StatusCode(500);
        }
    }

    // -------- Normalizar teléfono --------
    private static string NormalizePhone(string from)
    {
        if (string.IsNullOrEmpty(from)) return from;

        var s = from.Replace("whatsapp:", "").Trim();

        if (!s.StartsWith("+"))
            s = "+" + s;

        // Ajuste típico para móviles argentinos
        if (s.StartsWith("+54") && !s.StartsWith("+549"))
            s = s.Insert(3, "9");

        return s;
    }

    // -------- Detectar palabras de confirmación --------
    private static bool ContainsConfirmKeyword(string text)
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
}