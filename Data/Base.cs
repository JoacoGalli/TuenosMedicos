using MySqlConnector;
using Serilog;
using System.Data;


class Base
{
    private static readonly string connectionString;

    static Base()
    {
        // 📌 Configuración para leer appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Ubicación del proyecto
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 📌 Obtener la cadena de conexión
        connectionString = config.GetConnectionString("DefaultConnection");
    }

    public static string GetConnectionString()
    {
        return connectionString;
    }


    public static List<Medico> SelectAMedicos(string query, Dictionary<string, object> parameters = null)
    {
        List<Medico> resultados = new List<Medico>();

        using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Medico medico = new Medico
                            {
                                Id = reader.GetInt32("idMedicos"),
                                NombreMedico = reader.GetString("nombreMedico"),
                                diaTrabajo = reader.GetString("diaTrabajo"),
                                horaInicioTrabajo = reader.GetString("horaInicioTrabajo"),
                                horaFinTrabajo = reader.GetString("horaFinTrabajo"),
                                duracionTurno = reader.GetInt16("duracionTurno"),
                                duracionSobreTurno = reader.IsDBNull(reader.GetOrdinal("duracionSobreTurno"))? null: reader.GetInt16(reader.GetOrdinal("duracionSobreTurno"))
                            };

                            resultados.Add(medico);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Error en el select a medicos");
            }
        }
        return resultados;
    }


    public static List<Turno> SelectATurnos(string query, Dictionary<string, object> parameters = null)
    {        

        List<Turno> resultados = new List<Turno>();

        using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // 🔒 Agrega los parámetros de manera segura si existen
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Turno turno = new Turno
                            {
                                Id = reader.GetInt32("idturno"),
                                NombrePaciente = reader.GetString("nombrePaciente"),
                                ApellidoPaciente = reader.GetString("apellidoPaciente"),
                                Dni = reader.IsDBNull("dni") ? (int?)null : reader.GetInt32("dni"),
                                Cobertura = reader.GetString("cobertura"),
                                NumeroAfiliado = reader.IsDBNull(reader.GetOrdinal("numeroAfiliado")) ? "" : reader.GetString("numeroAfiliado"),
                                CategoriaAfiliado = reader.IsDBNull(reader.GetOrdinal("categoriaAfiliado")) ? "" : reader.GetString("categoriaAfiliado"),
                                Medico = reader.GetString("medico"),
                                FechaTurno = reader.GetDateTime("fechaTurno"),
                                HoraTurno = reader.GetString("horaTurno"),
                                Domicilio = reader.GetString("domicilio"),
                                Email = reader.GetString("email"),
                                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? "" : reader.GetString("telefono"),
                                Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? "" : reader.GetString("notas"),
                                NotasInternas = reader.IsDBNull(reader.GetOrdinal("notasInternas")) ? "" : reader.GetString("notasInternas"),
                                Cancelado = reader.GetBoolean("cancelado"),
                                MotivoCancelacion = reader.IsDBNull(reader.GetOrdinal("motivoCancelacion")) ? "" : reader.GetString("motivoCancelacion"),
                                TienePdf = reader.GetBoolean("tienePdf"),
                                PacienteAsistio = reader.GetBoolean("pacienteAsistio")
                            };

                            resultados.Add(turno);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Error en el select a turnos");
                
            }
        }
        return resultados;
    }

    public static List<MedicoFechaBloqueada> SelectAMedicosFechasBloqueadas(string query, Dictionary<string, object> parameters = null)
    {        

        List<MedicoFechaBloqueada> resultados = new List<MedicoFechaBloqueada>();

        using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // 🔒 Agrega los parámetros de manera segura si existen
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MedicoFechaBloqueada nuevaFecha = new MedicoFechaBloqueada
                            {

                                NombreMedico = reader.GetString("nombreMedico"),
                                FechaBloqueada = reader.GetDateTime("fechaBloqueada"),
                                Motivo = reader.IsDBNull(reader.GetOrdinal("motivo")) ? "" : reader.GetString("motivo"),
                                TodoElDia = reader.GetBoolean("todoElDia"),
                                HoraInicioBloqueo = reader.IsDBNull(reader.GetOrdinal("horaInicioBloqueo")) ? "" : reader.GetString("horaInicioBloqueo"), 
                                HoraFinBloqueo = reader.IsDBNull(reader.GetOrdinal("horaFinBloqueo")) ? "" : reader.GetString("horaFinBloqueo") 
                            };

                            resultados.Add(nuevaFecha);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Error en el select a fechas bloqueadas");                
            }
        }
        return resultados;
    }


    public static List<Cobertura> SelectACoberturas(string query, Dictionary<string, object> parameters = null)
    {
        List<Cobertura> resultados = new List<Cobertura>();

        using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // 🔒 Agrega los parámetros de manera segura si existen
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cobertura cobertura = new Cobertura
                            {
                                IdCobertura = reader.GetInt32("idCobertura"),
                                NombreCobertura = reader.GetString("nombreCobertura")                            
                            };
                            resultados.Add(cobertura);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Error en el select a coberturas");                
            }
        }
        return resultados;
    }


    public static List<Pdf> SelectAPDFS(string query, Dictionary<string, object> parameters = null)
    {
        List<Pdf> resultados = new List<Pdf>();

        using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // 🔒 Agrega los parámetros de manera segura si existen
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pdf nuevoArchivo = new Pdf
                            {
                                IdTurno = reader.GetInt32("idTurno"),
                                Archivo = reader.IsDBNull(reader.GetOrdinal("archivo")) ? null : (byte[])reader.GetValue(reader.GetOrdinal("archivo")),
                                NombreArchivo = reader.GetString("nombreArchivo"),
                            };
                            resultados.Add(nuevoArchivo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Error en el select a pdfs");                
            }
        }
        return resultados;
    }






    public static int InsertarTurno(Turno nuevoTurno)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
            {
                connection.Open();

                string query = "INSERT INTO `turnos` " +
                               "(`nombrePaciente`, `apellidoPaciente`, `dni`, `cobertura`, `medico`, `fechaTurno`, `horaTurno`, `notas`, `notasInternas`, `telefono`, `email`, `domicilio`, `numeroAfiliado`,`categoriaAfiliado`) " +
                               "VALUES (@nombrePaciente, @apellidoPaciente, @dni, @cobertura, @medico, @fechaTurno, @horaTurno, @notas, @notasInternas, @telefono, @email, @domicilio,  @numeroAfiliado, @categoriaAfiliado)";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // Parámetros para evitar SQL Injection
                    cmd.Parameters.AddWithValue("@nombrePaciente", nuevoTurno.NombrePaciente);
                    cmd.Parameters.AddWithValue("@apellidoPaciente", nuevoTurno.ApellidoPaciente);
                    cmd.Parameters.AddWithValue("@dni", nuevoTurno.Dni);
                    cmd.Parameters.AddWithValue("@cobertura", nuevoTurno.Cobertura);
                    cmd.Parameters.AddWithValue("@medico", nuevoTurno.Medico);
                    cmd.Parameters.AddWithValue("@fechaTurno", nuevoTurno.FechaTurno);
                    cmd.Parameters.AddWithValue("@horaTurno", nuevoTurno.HoraTurno);
                    cmd.Parameters.AddWithValue("@notas", nuevoTurno.Notas ?? ""); // Manejo de nulls
                    cmd.Parameters.AddWithValue("@notasInternas", nuevoTurno.NotasInternas ?? ""); // Manejo de nulls
                    cmd.Parameters.AddWithValue("@telefono", nuevoTurno.Telefono);
                    cmd.Parameters.AddWithValue("@email", nuevoTurno.Email);
                    cmd.Parameters.AddWithValue("@domicilio", nuevoTurno.Domicilio);                    
                    cmd.Parameters.AddWithValue("@numeroAfiliado", nuevoTurno.NumeroAfiliado);
                    cmd.Parameters.AddWithValue("@categoriaAfiliado", nuevoTurno.CategoriaAfiliado);

                    int filasAfectadas = cmd.ExecuteNonQuery(); // Retorna cuántas filas fueron insertadas
                    return filasAfectadas > 0 ? 0 : 1; // 0 si insertó correctamente, 1 si falló
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, "Error en el Base.InsertarTurno");
            return 1; // Retornar 1 indica un error
        }
    }

    public static int InsertarMedico(Medico nuevoMedico)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
            {
                connection.Open();

               
                string query = "INSERT INTO `medicos` " +
                                    "(`nombreMedico`,`diaTrabajo`,`horaInicioTrabajo`,`horaFinTrabajo`,`duracionTurno`,`duracionSobreTurno`) " +
                                "VALUES (@nombreMedico, @diaTrabajo, @horaInicioTrabajo, @horaFinTrabajo, @duracionTurno, @duracionSobreTurno)";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // Parámetros para evitar SQL Injection
                    cmd.Parameters.AddWithValue("@nombreMedico", nuevoMedico.NombreMedico);
                    cmd.Parameters.AddWithValue("@diaTrabajo", nuevoMedico.diaTrabajo);
                    cmd.Parameters.AddWithValue("@horaInicioTrabajo", nuevoMedico.horaInicioTrabajo);
                    cmd.Parameters.AddWithValue("@horaFinTrabajo", nuevoMedico.horaFinTrabajo);
                    cmd.Parameters.AddWithValue("@duracionTurno", nuevoMedico.duracionTurno);
                    cmd.Parameters.AddWithValue("@duracionSobreTurno", nuevoMedico.duracionSobreTurno);


                    int filasAfectadas = cmd.ExecuteNonQuery(); // Retorna cuántas filas fueron insertadas
                    return filasAfectadas > 0 ? 0 : 1; // 0 si insertó correctamente, 1 si falló
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, "Error en Base.InsertarMedico ");
            return 1; // Retornar 1 indica un error
        }
    }

    public static int InsertDeleteOrUpdateABase(string query, Dictionary<string, object> parametros)
    {        
        int filasAfectadas = 0;

        using (MySqlConnection connection = new MySqlConnection(Base.GetConnectionString()))
        {
            try
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // Agregar parámetros a la consulta
                    foreach (var parametro in parametros)
                    {
                        cmd.Parameters.AddWithValue(parametro.Key, parametro.Value);
                    }

                    // Ejecutar consulta UPDATE/INSERT/DELETE
                    filasAfectadas = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "Error en Base.InserDeletOUpdate ");
                return -1; // Retorna -1 en caso de error
            }
        }

        return filasAfectadas; // Retorna el número de filas afectadas
    }
}
