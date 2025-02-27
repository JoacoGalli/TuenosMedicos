using MySqlConnector;

class Base
{
    public static int ConexionBase(string query)
    {
        // Definir la cadena de conexión
        string connectionString = "Server=localhost;Database=turnos-medicos;User ID=dotnet;Password=Quelocura3!;";
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
}
