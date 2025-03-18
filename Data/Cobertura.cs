class Cobertura
{
    public int IdCobertura {get; set;}
    public string NombreCobertura {get; set;} ="";

    public static List<Cobertura> ObtenerCoberturas()
    {
        List<Cobertura> listaADevolver = new List<Cobertura>();

        string query = "SELECT * FROM `turnos-medicos`.coberturas;";

        listaADevolver = Base.SelectACoberturas(query);
             
        return listaADevolver;
    }
}

