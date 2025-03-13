using MySqlConnector;
using System.Collections.Generic;
using System.Data;

class Base
{
    
    //este metodo solo devuelve una lista de strings de 1 sola columna.
    public static List<string> EjecutarSelect(string query)
    {
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";
        List<string> resultados = new List<string>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        resultados.Add(reader.GetString(0)); // Agrega el primer campo de cada fila a la lista
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la base de datos: " + ex.Message);
            }
        }
        return resultados;
    }

    public static List<Medico> SelectAMedicos(string query)
    {
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";
        List<Medico> resultados = new List<Medico>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
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
                            duracionTurno =  reader.GetInt16("duracionTurno")
                        };

                        resultados.Add(medico); 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la base de datos: " + ex.Message);
            }
        }
        return resultados;
    }

    public static List<Turno> SelectATurnos(string query)
    {
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";
        List<Turno> resultados = new List<Turno> ();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
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
                            Medico = reader.GetString("medico"),
                            FechaTurno = reader.GetDateTime("fechaTurno"),
                            HoraTurno = reader.GetString("horaTurno"),
                            Domicilio = reader.GetString("domicilio"),
                            Email = reader.GetString("email"),
                            Telefono = reader.GetString("telefono"),
                            Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? "" : reader.GetString("notas"), // Maneja valores nulos
                            NotasInternas = reader.IsDBNull(reader.GetOrdinal("notasInternas")) ? "" : reader.GetString("notasInternas"), // Maneja valores nulos
                            Cancelado = reader.GetBoolean("cancelado"),
                            MotivoCancelacion = reader.IsDBNull(reader.GetOrdinal("motivoCancelacion")) ? "" : reader.GetString("motivoCancelacion"),
                            DocumentoPDF = reader.IsDBNull(reader.GetOrdinal("documentoPDF"))
                                                                                            ? null
                                                                                            : (byte[])reader.GetValue(reader.GetOrdinal("documentoPDF"))
                        }; 

                        resultados.Add(turno);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la base de datos: " + ex.Message);
            }
        }
        return resultados;
    }

    public static List<MedicoFechaBloqueada> SelectAMedicosFechasBloqueadas(string query)
    {
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";
        List<MedicoFechaBloqueada> resultados = new List<MedicoFechaBloqueada>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
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
            catch (Exception ex)
            {
                Console.WriteLine("Error en la base de datos: " + ex.Message);
            }
        }
        return resultados;
    }

    public static int InsertarTurno(Turno nuevoTurno, byte[] pdfBytes)
    {
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";

        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO `turnos-medicos`.`turnos` " +
                               "(`nombrePaciente`, `apellidoPaciente`, `dni`, `cobertura`, `medico`, `fechaTurno`, `horaTurno`, `notas`, `notasInternas`, `telefono`, `email`, `domicilio`, `documentoPDF`) " +
                               "VALUES (@nombrePaciente, @apellidoPaciente, @dni, @cobertura, @medico, @fechaTurno, @horaTurno, @notas, @notasInternas, @telefono, @email, @domicilio, @documentoPDF)";

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
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";

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
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";
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
