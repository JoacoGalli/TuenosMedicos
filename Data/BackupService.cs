using Serilog;
using System.Text;

public class BackupService 
{
    public async Task CrearBackupAsync() 
    {
        try 
        {
            string query = "SELECT * from turnos";
            List<Turno> turnos = Base.SelectATurnos(query);

            if (turnos == null || turnos.Count == 0)
            {
                Log.Information("No hay turnos para respaldar.");
                return;
            }

            string backupDirectory = Path.Combine(AppContext.BaseDirectory, "Backup");
            Directory.CreateDirectory(backupDirectory); // Crea la carpeta backup

            //Dentro de la carpeta backup, crea el archivo.
            var filePath = Path.Combine(backupDirectory, "backup_turnos.csv");
            var csvData = new StringBuilder();

            csvData.AppendLine($"Ultima actualizacion: {DateTime.Now:dd/MM/yyyy}");
            csvData.AppendLine("idturno,nombrePaciente,apellidoPaciente,dni,cobertura,numeroAfiliado,categoriaAfiliado,medico,fechaTurno,horaTurno," +
                                "domicilio,email,telefono,notas,notasInternas,cancelado,motivoCancelacion,tienePdf");

            foreach (Turno turno in turnos)
            {
                csvData.AppendLine($"{turno.Id},{turno.NombrePaciente},{turno.ApellidoPaciente},{turno.Dni},{turno.Cobertura},{turno.NumeroAfiliado},{turno.CategoriaAfiliado}," +
                    $"{turno.Medico},{turno.FechaTurno:yyyy-MM-dd},{turno.HoraTurno},{turno.Domicilio},{turno.Email},{turno.Telefono},{turno.Notas},{turno.NotasInternas}," +
                    $"{turno.Cancelado},{turno.MotivoCancelacion},{turno.TienePdf}");
            }

            //Sobreescribo el archivo
            await File.WriteAllTextAsync(filePath, csvData.ToString(), Encoding.UTF8);

            Log.Information("Archivo actualizado correctamente.");
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Error al generar el backup");
        }

        

    }
}