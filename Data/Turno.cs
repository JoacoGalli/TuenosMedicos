using Radzen;

class Turno
{
    public int Id {get; set;} = 0;

    public string NombrePaciente {get; set;} ="";
    public int Dni {get; set;}
    public string? Cobertura {get; set;}

    public string Medico {get; set;} ="";

    public DateTime FechaTurno {get; set;}

    public string HoraTurno {get; set;} ="";

    public string? Notas {get; set;}


    //Aca haria una consulta a la base con las fechas disponibles. Por ahora desactiva el dia anterior y el siguiente a hoy.
    IEnumerable<DateTime> fechasDisponibles = new DateTime[] { DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1) };

    public void ObtenerFechasDisponibles(DateRenderEventArgs args)
    {
        var special = fechasDisponibles.Select(d => d.Date).Contains(args.Date.Date);
        if (special)
        {
            args.Attributes.Add("style", "background-color: #ff6d41; border-color: white;");
        }

        args.Disabled = special || args.Disabled || args.Date.DayOfWeek == DayOfWeek.Sunday || args.Date.DayOfWeek == DayOfWeek.Saturday;
    }

    public static IEnumerable<string> ObtenerHorariosDisponibles()
    {
        IEnumerable<string> horarios = new List<string> { "08:00", "09:00", "10:00", "11:00" };

        return horarios;
    }


}