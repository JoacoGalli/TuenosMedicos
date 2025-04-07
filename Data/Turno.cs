using Microsoft.AspNetCore.Components;
using Radzen;
using Serilog;
using System;

class Turno
{
    public int Id {get; set;} = 0;

    public string NombrePaciente {get; set;} ="";
    public string ApellidoPaciente { get; set; } = "";
    public int? Dni {get; set;}
    public string? Cobertura {get; set;}
    public string? NumeroAfiliado { get; set; }
    public string? CategoriaAfiliado { get; set; }
    public string? Medico {get; set;} ="";
    public DateTime? FechaTurno {get; set;}
    public string HoraTurno {get; set;} ="";
    public string? Notas { get; set; } = "";
    public string? NotasInternas { get; set; } = "";
    public string? Telefono { get; set; }
    public string Email { get; set; } = "";
    public string? Domicilio { get; set; } = "";
    public bool Cancelado { get; set; } 
    public string? MotivoCancelacion { get; set; } = "";
    public bool TienePdf { get; set; }







    public void DesactivarFechas(DateRenderEventArgs args)
    {
        List<string> diasTrabajo = new List<string>();
        //Determino que dias trabaja el medico
        string query = " SELECT * FROM medicos WHERE nombreMedico= @nombreMedico ;";
        var parametros = new Dictionary<string, object> { { "@nombreMedico", Medico } };        

        List<Medico> listaMedicos = Base.SelectAMedicos(query, parametros);
        foreach (var medico in listaMedicos)
        {
            if (!string.IsNullOrEmpty(medico.diaTrabajo))  
            {
                diasTrabajo.Add(medico.diaTrabajo);
            }
        }

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

        List<DateTime> fechaDes = new List<DateTime>();

        //Verifico que fechas fueron bloqueadas por el admin
        string query2 = "SELECT * FROM `medicos_fechas_bloqueadas` WHERE `nombreMedico`= @nombreMedico AND todoElDia=true ;";
        var parametros2 = new Dictionary<string, object> { { "@nombreMedico", Medico } };

        List<MedicoFechaBloqueada> fechasBloq = Base.SelectAMedicosFechasBloqueadas(query2,parametros2);
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


    public IEnumerable<string> ObtenerHorariosDisponibles(bool esAdmin)
    {
        // Traigo los horarios de trabajo del medico
        string query = "SELECT idMedicos,nombreMedico,diaTrabajo,horaInicioTrabajo,horaFinTrabajo,duracionTurno,duracionSobreTurno FROM medicos " +
                       " WHERE nombreMedico = @nombreMedico and diaTrabajo = @diaTrabajo ;";

        var param = new Dictionary<string, object>
    {
        { "@nombreMedico", Medico },
        { "@diaTrabajo", FechaTurno?.ToString("dddd", new System.Globalization.CultureInfo("es-ES")) }
    };

        List<Medico> consultaAMedicos = Base.SelectAMedicos(query, param);

        if (consultaAMedicos.Count == 0)
        {
            Log.Information($"No se encontró horario para el médico '{Medico}' en '{FechaTurno?.ToString("dddd")}'");
            return new List<string>(); // o podés devolver null si querés distinguirlo después
        }

        string horaDeInicio = consultaAMedicos[0].horaInicioTrabajo;
        string horaFinal = consultaAMedicos[0].horaFinTrabajo;
        int duracionTurno = consultaAMedicos[0].duracionTurno;
        int duracionSobreTurno = consultaAMedicos[0].duracionSobreTurno ?? 0;


        DateTime inicioLaboral = DateTime.ParseExact(horaDeInicio, "HH:mm", null);
        DateTime finLaboral = DateTime.ParseExact(horaFinal, "HH:mm", null);

        List<string> horarios = new List<string>();

        // Turnos normales dentro del horario laboral
        horarios.AddRange(
            Enumerable.Range(0, (int)((finLaboral - inicioLaboral).TotalMinutes / duracionTurno))
                .Select(i => inicioLaboral.AddMinutes(i * duracionTurno).ToString("HH:mm"))
        );

        if (esAdmin)
        {
            DateTime inicioExt = DateTime.ParseExact("06:00", "HH:mm", null);
            DateTime finExt = DateTime.ParseExact("23:59", "HH:mm", null);

            if (duracionSobreTurno <= 0)
            {
                Log.Error("DuracionSobreTurno invalida: " + duracionSobreTurno);
                return new List<string>();
            }

            // Turnos antes del horario laboral
            if (inicioLaboral > inicioExt)
            {
                horarios.AddRange(
                    Enumerable.Range(0, (int)((inicioLaboral - inicioExt).TotalMinutes / duracionSobreTurno))
                        .Select(i => inicioExt.AddMinutes(i * duracionSobreTurno).ToString("HH:mm"))
                );
            }

            // Turnos después del horario laboral
            if (finExt > finLaboral)
            {
                horarios.AddRange(
                    Enumerable.Range(0, (int)((finExt - finLaboral).TotalMinutes / duracionSobreTurno))
                        .Select(i => finLaboral.AddMinutes(i * duracionSobreTurno).ToString("HH:mm"))
                );
            }
        }

        // Consultar los turnos ya reservados
        string queryTurnos = "SELECT * FROM turnos WHERE medico = @medico AND fechaTurno=@fechaTurno AND cancelado=false;";
        var parametros = new Dictionary<string, object>
    {
        { "@medico", Medico },
        { "@fechaTurno", FechaTurno?.ToString("yyyy-MM-dd") }
    };

        List<Turno> turnosReservados = Base.SelectATurnos(queryTurnos, parametros);

        if (turnosReservados.Count > 0)
        {
            List<string> horasOcupadas = turnosReservados
                .Select(t => DateTime.ParseExact(t.HoraTurno, "HH:mm", null).ToString("HH:mm"))
                .ToList();

            horarios = horarios.Except(horasOcupadas).ToList();
        }

        //Aca debo consultar a medicos_fechas_bloquedas si ese medico en esa fecha tiene un bloqueo temporal.
        string queryFechasBloqueadas = "SELECT * FROM medicos_fechas_bloqueadas WHERE nombreMedico = @medico AND fechaBloqueada = @fechaTurno AND todoElDia = false LIMIT 1 ";
        var parametros2 = new Dictionary<string, object>
        {
            { "@medico", Medico },
            { "@fechaTurno", FechaTurno?.ToString("yyyy-MM-dd") }
        };

        List<MedicoFechaBloqueada> fechaBloqueadas = Base.SelectAMedicosFechasBloqueadas(queryFechasBloqueadas, parametros2);

        if (fechaBloqueadas.Count > 0)
        {
            if (!string.IsNullOrEmpty(fechaBloqueadas[0].HoraInicioBloqueo) && !string.IsNullOrEmpty(fechaBloqueadas[0].HoraFinBloqueo)) 
            {
                DateTime inicioBloqueo = DateTime.ParseExact(fechaBloqueadas[0].HoraInicioBloqueo, "HH:mm", null);
                DateTime finBloqueo = DateTime.ParseExact(fechaBloqueadas[0].HoraFinBloqueo, "HH:mm", null);

                List<string> horariosBloqueados = new List<string>();

                horariosBloqueados.AddRange(
                    Enumerable.Range(0, (int)((finBloqueo - inicioBloqueo).TotalMinutes / duracionTurno))
                        .Select(i => inicioBloqueo.AddMinutes(i * duracionTurno).ToString("HH:mm")));

                horarios = horarios.Except(horariosBloqueados).ToList();
                Log.Information("Se ocultaron los horarios bloqueados.");
            }
           
        }

        return horarios.OrderBy(h => h); // para devolverlos ordenados
    }



}