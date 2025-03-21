using Microsoft.Extensions.Hosting;
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
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var servicio = scope.ServiceProvider.GetRequiredService<RecordatorioService>();
                await servicio.EnviarRecordatorioAsync();
            }

            // Esperar 24 horas antes de la siguiente ejecución
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

            // Esperar 10 minutos antes de la siguiente ejecución
            //await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

        }
    }
}
