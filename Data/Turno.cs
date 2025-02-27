using Microsoft.AspNetCore.Components;
using Radzen;

class Turno
{
    public int Id {get; set;} = 0;

    public string NombrePaciente {get; set;} ="";
    public string ApellidoPaciente { get; set; } = "";
    public int Dni {get; set;}
    public string? Cobertura {get; set;}

    public string Medico {get; set;} ="";

    public DateTime FechaTurno {get; set;}

    public string HoraTurno {get; set;} ="";

    public string? Notas {get; set;}



   


    
    public void DesactivarFechas(DateRenderEventArgs args)
    {
        
        string query = " SELECT diaTrabajo FROM `turnos-medicos`.medicos where nombreMedico='"+Medico+"';";
        List<string> diasTrabajo = Base.EjecutarSelect(query);

        
        // Convertimos la lista de días de trabajo a DayOfWeek
        var diasPermitidos = diasTrabajo.Select(dia =>
        {
            return dia switch
            {
                "Lunes" => DayOfWeek.Monday,
                "Martes" => DayOfWeek.Tuesday,
                "Miercoles" => DayOfWeek.Wednesday,
                "Jueves" => DayOfWeek.Thursday,
                "Viernes" => DayOfWeek.Friday,
                "Sabado" => DayOfWeek.Saturday,
                "Domingo" => DayOfWeek.Sunday,
                _ => throw new ArgumentException("Día no válido en diasTrabajo")
            };
        }).ToList();

        // Desactivar si no está en los días permitidos
        args.Disabled = !diasPermitidos.Contains(args.Date.DayOfWeek);

        if (!args.Disabled)
        {
            args.Attributes.Add("style", "background-color: #41ff6d; border-color: white;");
        }
    }






    public IEnumerable<string> ObtenerHorariosDisponibles()
    {
        string diaSemana = FechaTurno.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));

        string query = "SELECT idMedicos,nombreMedico,diaTrabajo,horaInicioTrabajo,horaFinTrabajo,duracionTurno FROM `turnos-medicos`.medicos where nombreMedico = '" + Medico + "' and diaTrabajo='"+diaSemana+"';";
        List<Medico> consultaAMedicos = Base.SelectAMedicos(query);

        string horaDeInicio = consultaAMedicos[0].horaInicioTrabajo;
        string horaFinal = consultaAMedicos[0].horaFinTrabajo;
        int duracionTurno = consultaAMedicos[0].duracionTurno;

        DateTime inicio = DateTime.ParseExact(horaDeInicio, "HH:mm", null);
        DateTime fin = DateTime.ParseExact(horaFinal, "HH:mm", null);

        

        IEnumerable<string> horarios = Enumerable.Range(0, (int)((fin - inicio).TotalMinutes / duracionTurno) + 1)
                                         .Select(i => inicio.AddMinutes(i * duracionTurno).ToString("HH:mm"));



        return horarios;
    }


}