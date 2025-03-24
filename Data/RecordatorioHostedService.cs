using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RecordatorioHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public RecordatorioHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    //Lo usamos para ejecutar el metodo EnviarRecordatorioAsync que envia emails recordatorios.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try 
        {
            Log.Information("Iniciando envio de emails periodicos..");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var servicio = scope.ServiceProvider.GetRequiredService<RecordatorioService>();
                    await servicio.EnviarRecordatorioAsync();
                }

                // Esperar 24 horas antes de la siguiente ejecución
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                // Esperar 5 minutos antes de la siguiente ejecución (para pruebas)
                //await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error en el enviar recordatorio async");
        }
        
        
    }
}
