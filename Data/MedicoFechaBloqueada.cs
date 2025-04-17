public class MedicoFechaBloqueada
{
    public string NombreMedico { get; set; } = "";
    public DateTime FechaBloqueada {get; set;}
    public string Motivo { get; set; } = "";

    private bool _todoElDia;
    public bool TodoElDia 
    {
        get { return _todoElDia; }
        set { _todoElDia = value; }
    }
    public string HoraInicioBloqueo { get; set; } = "";

    public string HoraFinBloqueo { get; set; } = "";


}

