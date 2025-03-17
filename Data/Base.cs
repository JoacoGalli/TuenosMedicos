using MySqlConnector;
using System.Collections.Generic;
using System.Data;

class Base
{
    private static readonly string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

    public static List<Medico> SelectAMedicos(string query, Dictionary<string, object> parameters = null)
    {
        List<Medico> resultados = new List<Medico>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
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
                            Medico medico = new Medico
                            {
                                Id = reader.GetInt32("idMedicos"),
                                NombreMedico = reader.GetString("nombreMedico"),
                                diaTrabajo = reader.GetString("diaTrabajo"),
                                horaInicioTrabajo = reader.GetString("horaInicioTrabajo"),
                                horaFinTrabajo = reader.GetString("horaFinTrabajo"),
                                duracionTurno = reader.GetInt16("duracionTurno")
                            };

                            resultados.Add(medico);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrió un error: " + ex.Message);
                // Logger.LogError(ex); // Usa un sistema de logging seguro como Serilog
            }
        }
        return resultados;
    }


    public static List<Turno> SelectATurnos(string query, Dictionary<string, object> parameters = null)
    {        

        List<Turno> resultados = new List<Turno>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
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
                                Dni = reader.GetInt32("dni"),
                                Cobertura = reader.GetString("cobertura"),
                                NumeroAfiliado = reader.IsDBNull(reader.GetOrdinal("numeroAfiliado")) ? "" : reader.GetString("numeroAfiliado"),
                                CategoriaAfiliado = reader.IsDBNull(reader.GetOrdinal("categoriaAfiliado")) ? "" : reader.GetString("categoriaAfiliado"),
                                Medico = reader.GetString("medico"),
                                FechaTurno = reader.GetDateTime("fechaTurno"),
                                HoraTurno = reader.GetString("horaTurno"),
                                Domicilio = reader.GetString("domicilio"),
                                Email = reader.GetString("email"),
                                Telefono = reader.GetString("telefono"),
                                Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? "" : reader.GetString("notas"),
                                NotasInternas = reader.IsDBNull(reader.GetOrdinal("notasInternas")) ? "" : reader.GetString("notasInternas"),
                                Cancelado = reader.GetBoolean("cancelado"),
                                MotivoCancelacion = reader.IsDBNull(reader.GetOrdinal("motivoCancelacion")) ? "" : reader.GetString("motivoCancelacion"),
                                DocumentoPDF = reader.IsDBNull(reader.GetOrdinal("documentoPDF")) ? null : (byte[])reader.GetValue(reader.GetOrdinal("documentoPDF"))
                            };

                            resultados.Add(turno);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrió un error: "+ex.Message);
                // Logger.LogError(ex); // Usa un sistema de logging seguro como Serilog
            }
        }
        return resultados;
    }

    public static List<MedicoFechaBloqueada> SelectAMedicosFechasBloqueadas(string query, Dictionary<string, object> parameters = null)
    {        

        List<MedicoFechaBloqueada> resultados = new List<MedicoFechaBloqueada>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
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
                                Motivo = reader.IsDBNull(reader.GetOrdinal("motivo")) ? "" : reader.GetString("motivo") // Maneja valores nulos
                            };

                            resultados.Add(nuevaFecha);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ocurrió un error: " + ex.Message);
                // Logger.LogError(ex); // Usa un sistema de logging seguro como Serilog
            }
        }
        return resultados;
    }




    public static int InsertarTurno(Turno nuevoTurno, byte[] pdfBytes)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO `turnos-medicos`.`turnos` " +
                               "(`nombrePaciente`, `apellidoPaciente`, `dni`, `cobertura`, `medico`, `fechaTurno`, `horaTurno`, `notas`, `notasInternas`, `telefono`, `email`, `domicilio`, `documentoPDF`,`numeroAfiliado`,`categoriaAfiliado`) " +
                               "VALUES (@nombrePaciente, @apellidoPaciente, @dni, @cobertura, @medico, @fechaTurno, @horaTurno, @notas, @notasInternas, @telefono, @email, @domicilio, @documentoPDF, @numeroAfiliado, @categoriaAfiliado)";

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
                    cmd.Parameters.AddWithValue("@documentoPDF", pdfBytes ?? new byte[0]); // Manejo de nulls en el PDF
                    cmd.Parameters.AddWithValue("@numeroAfiliado", nuevoTurno.NumeroAfiliado);
                    cmd.Parameters.AddWithValue("@categoriaAfiliado", nuevoTurno.CategoriaAfiliado);

                    int filasAfectadas = cmd.ExecuteNonQuery(); // Retorna cuántas filas fueron insertadas
                    return filasAfectadas > 0 ? 0 : 1; // 0 si insertó correctamente, 1 si falló
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error en la base de datos: " + ex.Message);
            return 1; // Retornar 1 indica un error
        }
    }

    public static int InsertarMedico(Medico nuevoMedico)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

               
                string query = "INSERT INTO `turnos-medicos`.`medicos` " +
                                    "(`nombreMedico`,`diaTrabajo`,`horaInicioTrabajo`,`horaFinTrabajo`,`duracionTurno`) " +
                                "VALUES (@nombreMedico, @diaTrabajo, @horaInicioTrabajo, @horaFinTrabajo, @duracionTurno)";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    // Parámetros para evitar SQL Injection
                    cmd.Parameters.AddWithValue("@nombreMedico", nuevoMedico.NombreMedico);
                    cmd.Parameters.AddWithValue("@diaTrabajo", nuevoMedico.diaTrabajo);
                    cmd.Parameters.AddWithValue("@horaInicioTrabajo", nuevoMedico.horaInicioTrabajo);
                    cmd.Parameters.AddWithValue("@horaFinTrabajo", nuevoMedico.horaFinTrabajo);
                    cmd.Parameters.AddWithValue("@duracionTurno", nuevoMedico.duracionTurno);
                    

                    int filasAfectadas = cmd.ExecuteNonQuery(); // Retorna cuántas filas fueron insertadas
                    return filasAfectadas > 0 ? 0 : 1; // 0 si insertó correctamente, 1 si falló
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error en la base de datos: " + ex.Message);
            return 1; // Retornar 1 indica un error
        }
    }

    public static int InsertDeleteOrUpdateABase(string query, Dictionary<string, object> parametros)
    {        
        int filasAfectadas = 0;

        using (MySqlConnection connection = new MySqlConnection(connectionString))
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
                Console.WriteLine("Error en la base de datos: " + ex.Message);
                return -1; // Retorna -1 en caso de error
            }
        }

        return filasAfectadas; // Retorna el número de filas afectadas
    }
}
