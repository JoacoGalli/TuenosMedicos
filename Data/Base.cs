using MySqlConnector;
using System.Collections.Generic;

class Base
{

    //este metodo es mejor no usarlo para selects (aunque sirve), porque retorna un INT con un codigo 0-1. Para selects es mejor EjecutarSelect
    public static int ConexionBase(string query)
    {
        // Definir la cadena de conexión
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=victorinox72401802!;";
        int err = 0;
        // Crear una conexión
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            try
            {
                // Abrir la conexión
                connection.Open();
                
                // Aquí puedes ejecutar comandos SQL, consultas, etc.                
                MySqlCommand cmd = new MySqlCommand(query, connection);

                // Ejecutar la consulta y leer los resultados
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0)); // Imprime el primer campo de cada fila
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error proveniente de la base: " + ex.Message);
                err = 1;
            }
        }
        //Si devuelvo 0 es porque no hubo error. Si devuelvo 1 es porque algo fallo.
        return err;
    }


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
                            Notas = reader.IsDBNull(reader.GetOrdinal("notas")) ? "" : reader.GetString("notas") // Maneja valores nulos
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
}
