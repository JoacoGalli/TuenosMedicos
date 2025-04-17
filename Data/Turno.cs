using Microsoft.AspNetCore.Components;
using Radzen;
using Serilog;
using System;
using System.Collections.Generic;
using TurnosMedicos.Data;

class Turno
{
    #region propiedades
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
    public bool PacienteAsistio { get; set; }

    #endregion


    #region metodos
       
    public void DesactivarFechas(DateRenderEventArgs args, bool esAdmin)
    {
        //Obtengo los datos del medico
        string query = " SELECT * FROM medicos WHERE nombreMedico= @nombreMedico ;";
        var parametros = new Dictionary<string, object> { { "@nombreMedico", Medico } };
        List<Medico> listaMedicos = Base.SelectAMedicos(query, parametros);

        
        //Determino los dias que trabaja
        var diasPermitidos = Horarios.DiasTrabajo(Medico, listaMedicos);


        //Obtengo las fechas bloqueadas y las agrego a fechaDes
        List<DateTime> fechaDes = new List<DateTime>();
        List<MedicoFechaBloqueada> fechasBloq = Horarios.FechasBloqueadasPorAdmin(Medico);
        foreach (var fecha in fechasBloq)
        {
            fechaDes.Add(fecha.FechaBloqueada);
        }

        //Obtengo los horarios configurados del medico.
        List<Horarios> listaHorarios = Horarios.HorariosConfigurados(esAdmin, listaMedicos);

        //Obtengo los turnos reservados de hoy a seis meses.
        DateTime hoy = DateTime.Today;
        DateTime seisMeses = hoy.AddMonths(6);
        List<Turno> turnosReservados = ConsultarTurnosReservados(Medico, hoy, seisMeses);

        List<DateTime> fechasSinDisponibilidad = new List<DateTime>();

        for (DateTime fecha = hoy;  fecha <= seisMeses; fecha = fecha.AddDays(1))
        {
            foreach (var dia in diasPermitidos)
            {
                if (fecha.DayOfWeek == dia) 
                {
                    bool existeBloqueo = fechasBloq.Any(f => f.FechaBloqueada.Date == fecha.Date); //Si existeBloqueo no necesito consultar disponibilidad.
                    if (!existeBloqueo)
                    {
                        //Filto los turnos de esa fecha
                        var turnosDelDia = turnosReservados.Where(t => t.FechaTurno?.Date == fecha.Date).ToList();

                        if (turnosDelDia.Count()>0)        //En este punto (un dia que trabaja y no esta bloqueado) si no hay turnos reservados no necesito consultar disponibilidad.
                        {
                            //Filtro los horarios dependiendo "fecha" del for.
                            var diasEnEspañol = new Dictionary<DayOfWeek, string>
                            {
                                { DayOfWeek.Monday, "Lunes" },
                                { DayOfWeek.Tuesday, "Martes" },
                                { DayOfWeek.Wednesday, "Miercoles" },
                                { DayOfWeek.Thursday, "Jueves" },
                                { DayOfWeek.Friday, "Viernes" },
                                { DayOfWeek.Saturday, "Sabado" },
                                { DayOfWeek.Sunday, "Domingo" }
                            };

                            var horariosDelDia = listaHorarios
                                .Where(h => h.DiaTrabajo == diasEnEspañol[fecha.DayOfWeek])  // Comparar en español
                                .ToList();

                            bool fechaConTurnosDisponibles = VerificarDisponibilidadDeTurnos(esAdmin, turnosDelDia, horariosDelDia);

                            if (!fechaConTurnosDisponibles)
                            {
                                fechasSinDisponibilidad.Add(fecha);
                            }
                        } 
                        
                    }

                }
            }
        }

        args.Disabled = !diasPermitidos.Contains(args.Date.DayOfWeek) || args.Date.Date < DateTime.Today || fechaDes.Contains(args.Date.Date) || fechasSinDisponibilidad.Contains(args.Date.Date);

        if (!args.Disabled)
        {
            args.Attributes.Add("style", "background-color: #41ff6d; border-color: white;"); // Verde si está habilitado
        }
        else if (fechasSinDisponibilidad.Contains(args.Date.Date))
        {
            args.Attributes.Add("style", "background-color: #ff6961; border-color: white;"); // Rojo si está deshabilitado por falta de disponibilidad
        }


    }

    //Verifica la disponibilidad horaria de un dia particular. Devuelve True si tiene horarios disponibles y false si no los tiene.
    public bool VerificarDisponibilidadDeTurnos(bool esAdmin, List<Turno> turnosReservados, List<Horarios> listaDeHorarios)
    {
        var horarioEseDia = listaDeHorarios
           .Select(h => new Horarios
           {
               DiaTrabajo = h.DiaTrabajo,
               ListaHorarios = h.ListaHorarios.ToList() // Hago una copia de la lista para que no me modifique la original.
           })
           .ToList();

        // Obtengo la lista de horas reservadas (por ejemplo: "08:45", "15:30", etc.)
        var horasReservadas = turnosReservados.Select(t => t.HoraTurno).ToHashSet(); // más eficiente para búsquedas

        // Recorro horarioEseDia y filtramos las horas que se encuentren en horasReservadas, para que queden solo las libres
        foreach (var horario in horarioEseDia)
        {
            horario.ListaHorarios = horario.ListaHorarios.Where(hora => !horasReservadas.Contains(hora)).ToList(); 
        }

        if (horarioEseDia[0].ListaHorarios.Any())
        {
            return true;        //Si hay horarios devuelve true,sino false.
        }
        else
        {
            return false;
        }


    }

    public List<Turno> ConsultarTurnosReservados(string med, DateTime fechaIni, DateTime fechaFin)
    {
        // Consultar los turnos ya reservados
        string queryTurnos = "SELECT * FROM turnos WHERE medico = @medico AND fechaTurno BETWEEN @fechaIni AND @fechaFin AND cancelado=false;";
        var parametros = new Dictionary<string, object>
        {
            { "@medico", med },
            { "@fechaIni", fechaIni.ToString("yyyy-MM-dd") },
            { "@fechaFin", fechaFin.ToString("yyyy-MM-dd") }
        };

        List<Turno> turnosReservados = Base.SelectATurnos(queryTurnos, parametros);

        return turnosReservados;

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

        //Elimino cualquier posibilidad de horarios repetidos para evitar excepciones con Radzen.
        horarios = horarios.Distinct().ToList();


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



    #endregion



}