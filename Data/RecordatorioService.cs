using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

public class RecordatorioService 
{
    public async Task EnviarRecordatorioAsync() 
    {
        string mañana = DateTime.Now.AddDays(1).Date.ToString("yyyy-MM-dd");
        
        string query = "SELECT * FROM `turnos-medicos`.`turnos` WHERE recordatorioEnviado=false AND fechaTurno = @mañana";
        var parametros = new Dictionary<string, object> { { "@mañana", mañana } };

        List<Turno> turnosDeMañana = Base.SelectATurnos(query, parametros);

        foreach (Turno turno in turnosDeMañana)
        {
            //Por cada 'turno' envio un email avisando que el turno es mañana:
            
            EmailService nuevoEmail = new EmailService();
            string cuerpoEmail = "<b>Estimado/a:</b><br><br>Le enviamos este email para recordarle su turno con <b>" + turno.Medico + "</b> el dia: <b>" + turno.FechaTurno?.ToString("dd-MM-yyyy") + "</b> a las: <b>" + turno.HoraTurno + "</b> hs.<br><br>Si no es posible asistir, puede cancelarlo. <br><br> Muchas gracias.<br><br>Consultorio Médico.";
            //nuevoEmail.EnviarCorreoAsync(turno.email, "Consultorio Médico - Recordatorio de turno", cuerpoEmail);
            nuevoEmail.EnviarCorreoAsync("kevinnatalini@gmail.com", "Consultorio Médico - Recordatorio de turno", cuerpoEmail);

            
            
            //Luego de enviarlo, updateo "recordatorioEnviado"=true en `turnos`
            string query2 = "UPDATE `turnos-medicos`.turnos SET recordatorioEnviado=true WHERE idturno=@idturno;";
            var parametros2 = new Dictionary<string, object> { { "@idturno", turno.Id } };
            int actualizoEstado = Base.InsertDeleteOrUpdateABase(query2, parametros2);
        }
    }

}