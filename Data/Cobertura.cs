class Cobertura
{
    public int Id {get; set;}
    public string NombreCobertura {get; set;} ="";

    public static List<Cobertura> ObtenerCoberturas()
    {
        List<Cobertura> listaADevolver = new List<Cobertura>();

        //aca haria una consulta a la base e iria agregando los objetos a la lista "coberturas" declarada arriba.
        listaADevolver.Add(new Cobertura(){
            Id= 1,
            NombreCobertura= "IOMA"
        });

        listaADevolver.Add(new Cobertura(){
            Id= 2,
            NombreCobertura= "OSDE"
        });

        listaADevolver.Add(new Cobertura(){
            Id= 2,
            NombreCobertura= "OTRA"
        });

        return listaADevolver;
    }
}

