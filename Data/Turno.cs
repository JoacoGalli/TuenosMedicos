using Microsoft.AspNetCore.Components;
using Radzen;

class Turno
{
    public int Id {get; set;} = 0;

    public string NombrePaciente {get; set;} ="";
    public string ApellidoPaciente { get; set; } = "";
    public int? Dni {get; set;}
    public string? Cobertura {get; set;}
    public string? Medico {get; set;} ="";
    public DateTime FechaTurno {get; set;}
    public string HoraTurno {get; set;} ="";
    public string? Notas { get; set; } = "";
    public string? Telefono { get; set; }
    public string Email { get; set; } = "";
    public string? Domicilio { get; set; } = "";
    public bool Cancelado { get; set; } 
    public string? MotivoCancelacion { get; set; } = "";







    public void DesactivarFechas(DateRenderEventArgs args)
    {
        //Determino que dias trabaja el medico
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


        //Verifico que fechas fueron bloqueadas por el admin
        string query2 = "SELECT `nombreMedico`,`fechaBloqueada`,`motivo` FROM `turnos-medicos`.`medicos_fechas_bloqueadas` WHERE `nombreMedico`='" + Medico+"';";
        
        List<DateTime> fechaDes = new List<DateTime>();
        
        List<MedicoFechaBloqueada> fechasBloq = Base.SelectAMedicosFechasBloqueadas(query2);
        foreach (var fecha in fechasBloq)
        {
            fechaDes.Add(fecha.FechaBloqueada);   
        }

        // Desactivo los dias que no trabaja.
        args.Disabled = !diasPermitidos.Contains(args.Date.DayOfWeek) || args.Date.Date < DateTime.Today || fechaDes.Contains(args.Date.Date);



        if (!args.Disabled)
        {
            args.Attributes.Add("style", "background-color: #41ff6d; border-color: white;");
        }
    }






    public IEnumerable<string> ObtenerHorariosDisponibles()
    {
       
        //Traigo los horarios de trabajo del medico (inicio a fin de la jornada)
        string diaSemana = FechaTurno.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));

        string query = "SELECT idMedicos,nombreMedico,diaTrabajo,horaInicioTrabajo,horaFinTrabajo,duracionTurno FROM `turnos-medicos`.medicos where nombreMedico = '" + Medico + "' and diaTrabajo='"+diaSemana+"';";
        List<Medico> consultaAMedicos = Base.SelectAMedicos(query);

        string horaDeInicio = consultaAMedicos[0].horaInicioTrabajo;
        string horaFinal = consultaAMedicos[0].horaFinTrabajo;
        int duracionTurno = consultaAMedicos[0].duracionTurno;

        DateTime inicio = DateTime.ParseExact(horaDeInicio, "HH:mm", null);
        DateTime fin = DateTime.ParseExact(horaFinal, "HH:mm", null);

        
        //En este punto, horarios tiene todas las horas entre inicio y fin, sin considerar los turnos ya reservados.
        IEnumerable<string> horarios = Enumerable.Range(0, (int)((fin - inicio).TotalMinutes / duracionTurno) + 1)
                                        .Select(i => inicio.AddMinutes(i * duracionTurno).ToString("HH:mm"));



        //aca consulto la tabla turno para ver si el medico en X dia tiene turnos ocupados.
        string fechaMostrar = FechaTurno.ToString("yyyy-MM-dd");
        string queryTurnos = "SELECT * FROM `turnos-medicos`.turnos where medico = '"+ Medico + "' and fechaTurno='"+ fechaMostrar + "';";
        List<Turno> turnosReservados = Base.SelectATurnos(queryTurnos);



        //Si encuentro turnos reservados, los quito de horarios para que no puedan ser seleccionados.
        if (turnosReservados.Count>0)
        {
            List<string> horasOcupadas = turnosReservados
                                                        .Select(t => DateTime.ParseExact(t.HoraTurno, "HH:mm", null).ToString("HH:mm"))
                                                        .ToList();

            horarios = horarios.Except(horasOcupadas);
        }


        return horarios;
    }


}