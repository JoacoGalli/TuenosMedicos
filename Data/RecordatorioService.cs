using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Serilog;

public class RecordatorioService 
{
    public async Task EnviarRecordatorioAsync() 
    {
        string mañana = DateTime.Now.AddDays(1).Date.ToString("yyyy-MM-dd");
        List<Turno> turnosDeMañana = new List<Turno>();
        try 
        {
            Log.Information("Buscando los turnos de mañana...");
            string query = "SELECT * FROM turnos WHERE cancelado=false AND recordatorioEnviado=false AND fechaTurno = @mañana";
            var parametros = new Dictionary<string, object> { { "@mañana", mañana } };

            turnosDeMañana = Base.SelectATurnos(query, parametros);
        }
        catch (Exception ex) 
        {
            Log.Error(ex, "Error al intentar seleccionar los turnos de mañana");
        }

        foreach (Turno turno in turnosDeMañana)
        {
            try 
            {
                Log.Information("Se encontraron "+ turnosDeMañana.Count +" turnos. Enviando emails de notificaciòn.");
                //Por cada 'turno' envio un email avisando que el turno es mañana:
                string cancelarUrl = $"https://consultoriocairo.com.ar/cancelar-turno/{turno.Id}";

                if (!string.IsNullOrEmpty(turno.Email))
                {
                    EmailService nuevoEmail = new EmailService();
                    string cuerpoEmail = "<b>Estimado/a:</b><br><br>Le enviamos este email para recordarle su turno con <b>" + turno.Medico + "</b> el dia: <b>" +
                                            turno.FechaTurno?.ToString("dd-MM-yyyy") + "</b> a las: <b>" + turno.HoraTurno
                                            + "</b> hs.<br><br>Si no es posible asistir, puedes cancelarlo haciendo click aquí: " + cancelarUrl
                                            + " . <br><br> Muchas gracias.<br><br>Consultorio Médico.";

                    await nuevoEmail.EnviarCorreoAsync(turno.Email, "Consultorio Médico - Recordatorio de turno", cuerpoEmail);

                }


                //Luego de enviarlo, updateo "recordatorioEnviado"=true en `turnos`
                string query2 = "UPDATE turnos SET recordatorioEnviado=true WHERE idturno=@idturno;";
                var parametros2 = new Dictionary<string, object> { { "@idturno", turno.Id } };
                int actualizoEstado = Base.InsertDeleteOrUpdateABase(query2, parametros2);
            }
            catch(Exception ex)
            {
                Log.Error("Error al enviar los emails recordatorios de turno");
            }
            
        }
    }

}