class Pdf
{
    public int IdTurno {get; set;}
    public byte[] Archivo {get; set;}
    public string NombreArchivo { get; set; }

    public static void InsertarPDFS(List<Pdf> pdfBytes, Turno turno) 
    {
        //Con los datos de 'nuevoTurno' que use para hacer el insert, voy a preguntarle a turnos por el ID del nuevo turno creado
        string query = "SELECT * FROM `turnos` WHERE `nombrePaciente`= @nombrePaciente AND `apellidoPaciente` = @apellidoPaciente " +
                                " AND `dni` = @dni AND `medico` = @medico AND `fechaTurno` = @fechaTurno AND `horaTurno` = @horaTurno AND `email` = @email ";

        var parametros = new Dictionary<string, object>
                    {
                        { "@nombrePaciente", turno.NombrePaciente },
                        { "@apellidoPaciente", turno.ApellidoPaciente },
                        { "@dni", turno.Dni },
                        { "@medico", turno.Medico },
                        { "@fechaTurno", turno.FechaTurno },
                        { "@horaTurno", turno.HoraTurno },
                        { "@email", turno.Email }
                    };

        List<Turno> nuevoTurnoCreado = new List<Turno>();
        nuevoTurnoCreado = Base.SelectATurnos(query, parametros);

        //Una vez que tengo el ID del nuevo turno, voy a hacer un INSERT a pdfs por cada archivo encontrado
        int idNuevoTurnoCreado;
        if (nuevoTurnoCreado != null)
        {
            idNuevoTurnoCreado = nuevoTurnoCreado[0].Id;

            foreach (Pdf pdf in pdfBytes)
            {
                string query2 = "INSERT INTO `pdfs` (`idTurno`,`archivo`, `nombreArchivo`) VALUES (@idTurno, @archivo, @nombreArchivo);";
                var parametros2 = new Dictionary<string, object>
                        {
                            { "@idTurno", idNuevoTurnoCreado },
                            { "@archivo", pdf.Archivo },
                            { "@nombreArchivo", pdf.NombreArchivo }
                        };

                int pdfInsertados = Base.InsertDeleteOrUpdateABase(query2, parametros2);

            }
           
            string query3 = "UPDATE `turnos` SET tienePDF = true WHERE idturno = @idTurno";
            var parametros3 = new Dictionary<string, object> { { "@idTurno", idNuevoTurnoCreado } };
            int turnoActualizado = Base.InsertDeleteOrUpdateABase(query3, parametros3);
        }
    }

}



