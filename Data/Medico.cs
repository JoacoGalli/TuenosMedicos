class Medico
{
    public int Id { get; set; }
    public string NombreMedico {get; set;} ="";
    public string diaTrabajo { get; set; } = "";
    public string horaInicioTrabajo { get; set; } = "";
    public string horaFinTrabajo { get; set; } = "";
    public int duracionTurno { get; set; }



    public static List<Medico> ObtenerMedicos()
    {
        List<Medico> medicos = new List<Medico>();


        string query = "SELECT idMedicos,nombreMedico,diaTrabajo,horaInicioTrabajo,horaFinTrabajo,duracionTurno FROM `turnos-medicos`.medicos;";
        medicos = Base.SelectAMedicos(query);

        medicos = medicos   .Select(m => m.NombreMedico) // Extrae solo los nombres
                            .Distinct() // Elimina duplicados
                            .Select(nombre => new Medico { NombreMedico = nombre }) // Crea nuevos objetos Medico con nombres únicos
                            .ToList();

        return medicos;
    }
}