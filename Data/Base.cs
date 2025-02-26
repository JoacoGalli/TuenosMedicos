using MySqlConnector;

class Base
{
    public static void ConexionBase(string query)
    {
        // Definir la cadena de conexión
        string connectionString = "Server=localhost;Database=medicshifts;User ID=root;Password=7240;";

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
            }
        }
    }

     public static bool VerificarCredencialesUsuario(string username, string password)
    {
        string connectionString = "Server=localhost;Database=medicshifts;User ID=root;Password=7240;";

        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM usuarios WHERE NombreUsuario = @username AND ContraseñaUsuario = @password";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    var result = Convert.ToInt32(cmd.ExecuteScalar());
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al conectar: " + ex.Message);
                return false;
            }
        }
    }
}
