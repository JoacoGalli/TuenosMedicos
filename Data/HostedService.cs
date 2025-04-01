using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

public class HostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public HostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    //Lo usamos para ejecutar el metodo EnviarRecordatorioAsync que envia emails recordatorios.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try 
        {
            Log.Information("Iniciando hosted service...");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    try 
                    {
                        var servicio = scope.ServiceProvider.GetRequiredService<RecordatorioService>();
                        await servicio.EnviarRecordatorioAsync();
                        Log.Information("Recordatorios enviados correctamente.");
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex, "Error al enviar los recordatorios");
                    }

                    try
                    {
                        var servicio2 = scope.ServiceProvider.GetRequiredService<BackupService>();
                        await servicio2.CrearBackupAsync();
                        Log.Information("Backup creado correctamente.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al crear el backup diario.");
                    }                    
                }


                // Esperar 24 horas antes de la siguiente ejecución
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    Log.Information("Task.Delay fue cancelado, terminando el servicio.");
                    break; // Salimos del bucle para finalizar correctamente
                }

                // Esperar 5 minutos antes de la siguiente ejecución (para pruebas)
                //await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            }
        }
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            Log.Error(ex, "Error en el enviar recordatorio async");
        }
        
        
    }
}
