using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("ExpedienteAcademico")]
public class ExpedienteAcademico
{
    [XmlAttribute("periodo")]
    public string Periodo { get; set; }

    public string NumeroControl { get; set; }
    public string NombreCompleto { get; set; }
    public string Carrera { get; set; }
    public int Semestre { get; set; }

    [XmlArray("Materias")]
    [XmlArrayItem("Materia")]
    public List<Materia> Materias { get; set; } = new List<Materia>();
}

public class Materia
{
    [XmlAttribute("clave")]
    public string Clave { get; set; }

    public string NombreMateria { get; set; }
    public int Creditos { get; set; }
    public decimal Calificacion { get; set; }
    public bool Aprobada { get; set; }
}