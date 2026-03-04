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
                string confirmarUrl = $"https://consultoriocairo.com.ar/confirmar-turno/{turno.Id}";
                //string confirmarUrl = $"https://localhost:44393/confirmar-turno/{turno.Id}";

                if (!string.IsNullOrEmpty(turno.Email))
                {
                    try 
                    {
                        EmailService nuevoEmail = new EmailService();
                        string cuerpoEmail = "<b>Estimado/a:</b><br><br>" +
                                            "Le enviamos este correo para recordarle su turno con <b>" + turno.Medico + "</b> el día <b>" +
                                            turno.FechaTurno?.ToString("dd-MM-yyyy") + "</b> a las <b>" + turno.HoraTurno + " hs</b>." +

                                            "<br><br><b>Confirmación:</b> Puede confirmar su asistencia haciendo clic aquí: " +
                                            "<a href='" + confirmarUrl + "'>Confirmar turno</a>." +

                                            "<br><br><b>Cancelación:</b> Si no puede asistir, puede cancelar su turno haciendo clic aquí: " +
                                            "<a href='" + cancelarUrl + "'>Cancelar turno</a>." +

                                            "<br><br><b>Importante:</b> En caso de no cancelar con anticipación o no asistir a su cita sin previo aviso, " +
                                            "se cobrará la consulta como si hubiera asistido, ya que el médico reserva ese horario exclusivamente para usted." +

                                            "<br><br>Muchas gracias.<br><br><b>Consultorio Cairo</b>." +

                                            "<br><br><i>Este correo es enviado automáticamente. Por favor, no responder. " +
                                            "Si desea comunicarse con el consultorio, puede escribirnos por WhatsApp al número <b>2227402738</b>.</i>";



                        await nuevoEmail.EnviarCorreoAsync(turno.Email, "Consultorio Médico - Recordatorio de turno", cuerpoEmail);
                    }
                    catch(Exception ex) 
                    {
                        Log.Error(ex, "Error al enviar el recordatorio de turno para: " + turno.Email);
                    }
                    

                }

                if (!string.IsNullOrEmpty(turno.Telefono))
                {
                    try
                    {
                        var whatsappService = new WhatsAppService();

                        var contentSid = "HX28a37c2da5c4b7b52d5c4dd42415ab68"; // <- SID real de recordatorio_turno

                        var variables = new Dictionary<string, string>
                        {
                            { "1", $"{turno.NombrePaciente}" },
                            { "2", turno.Medico },
                            { "3", turno.FechaTurno?.ToString("dd/MM/yyyy") ?? "" },
                            { "4", turno.HoraTurno },
                            { "6", confirmarUrl },
                            { "5", cancelarUrl }
                        };

                        whatsappService.EnviarMensajePlantilla(turno.Telefono, contentSid, variables);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al enviar el WhatsApp para: " + turno.Telefono);
                    }
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