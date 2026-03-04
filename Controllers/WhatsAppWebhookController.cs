using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(ILogger<WhatsAppWebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost("inbound")]
    public async Task<IActionResult> ReceiveMessage()
    {
        var form = await Request.ReadFormAsync();
        string from = form["From"];
        string body = form["Body"];

        _logger.LogInformation($"📩 Mensaje entrante de {from}: {body}");

        // Podés agregar lógica para guardar en DB, responder, etc.

        return Ok(); // Twilio espera un 200 OK
    }
}
