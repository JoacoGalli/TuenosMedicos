using Serilog;

namespace TurnosMedicos.Data
{
    public class Horarios
    {
        public string DiaTrabajo { get; set; } //Puede ser 'Lunes', 'Viernes', etc.
        public IEnumerable<string> ListaHorarios { get; set; }

        #region metodos

        //Determina los dias que trabaja el medico. Por ej: 'Viernes','Lunes'
        public static List<DayOfWeek> DiasTrabajo(string med,List<Medico> listaMedico = null)
        {
            List<string> diasTrabajo = new List<string>();
            if (listaMedico.Count() == 0)
            {
                //Determino que dias trabaja el medico
                string query = " SELECT * FROM medicos WHERE nombreMedico= @nombreMedico ;";
                var parametros = new Dictionary<string, object> { { "@nombreMedico", med } };

                listaMedico = Base.SelectAMedicos(query, parametros);
            }

            foreach (var medico in listaMedico)
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

            return diasPermitidos;
        }

        //La idea de este metodo es llamarlo una sola vez, porque se entiende que el medico trabaja (cambiando el dia) siempre en el mismo horario.
        public static List<Horarios> HorariosConfigurados(bool esAdmin, List<Medico> consultaAMedicos)
        {
            List<Horarios> listaHorarios = new List<Horarios>();
            
           
            if (consultaAMedicos.Count() == 0)
            {
                Log.Warning("El medico no tiene horarios establecidos.");
            }
            else
            {
                foreach (Medico med in consultaAMedicos)
                {
                    string diaTrabajo = med.diaTrabajo;
                    string horaDeInicio = med.horaInicioTrabajo;
                    string horaFinal = med.horaFinTrabajo;
                    int duracionTurno = med.duracionTurno;
                    int duracionSobreTurno = med.duracionSobreTurno ?? 0;


                    DateTime inicioLaboral = DateTime.ParseExact(horaDeInicio, "HH:mm", null);
                    DateTime finLaboral = DateTime.ParseExact(horaFinal, "HH:mm", null);

                    List<string> horarios = new List<string>();

                    // Turnos normales dentro del horario laboral
                    horarios.AddRange(
                        Enumerable.Range(0, (int)((finLaboral - inicioLaboral).TotalMinutes / duracionTurno))
                            .Select(i => inicioLaboral.AddMinutes(i * duracionTurno).ToString("HH:mm"))
                    );

                    //Si es admin, se agregan los sobreturno antes y despues del horario establecido:
                    if (esAdmin)
                    {
                        DateTime inicioExt = DateTime.ParseExact("06:00", "HH:mm", null);
                        DateTime finExt = DateTime.ParseExact("23:59", "HH:mm", null);

                        if (duracionSobreTurno <= 0)
                        {
                            Log.Error("DuracionSobreTurno invalida: " + duracionSobreTurno);

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

                    horarios.OrderBy(h => h); // para devolverlos ordenados

                    listaHorarios.Add(new Horarios() { DiaTrabajo = diaTrabajo, ListaHorarios = horarios });
                }

            }

            return listaHorarios;
        }


        public static List<MedicoFechaBloqueada> FechasBloqueadasPorAdmin(string med)
        {
            //Verifico que fechas fueron bloqueadas por el admin
            string query2 = "SELECT * FROM `medicos_fechas_bloqueadas` WHERE `nombreMedico`= @nombreMedico AND fechaBloqueada >= @hoy AND todoElDia=true ;";
            var parametros2 = new Dictionary<string, object> { { "@nombreMedico", med }, {"@hoy", DateTime.Today.ToString("yyyy-MM-dd") } };

            List<MedicoFechaBloqueada> fechasBloq = Base.SelectAMedicosFechasBloqueadas(query2, parametros2);

            return fechasBloq;
        }
        #endregion
    }
}
