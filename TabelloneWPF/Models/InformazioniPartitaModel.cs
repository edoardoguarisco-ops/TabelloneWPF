using System.Collections.Generic;
using System.Xml.Serialization;

namespace TabelloneWPF
{
    [XmlRoot("InformazioniPartita")]
    public class InformazioniPartitaModel
    {
        public string NomeSquadra1 { get; set; } = "";
        public string LogoSquadra1 { get; set; } = "";
        public string NomeSquadra2 { get; set; } = "";
        public string LogoSquadra2 { get; set; } = "";
        public string DataPartita { get; set; } = "";
        public string OraInizio { get; set; } = "";

        [XmlArray("UfficialiPartita")]
        [XmlArrayItem("Ufficiale")]
        public List<UfficialePartitaModel> Ufficiali { get; set; } = new();
    }

    public class UfficialePartitaModel
    {
        [XmlAttribute]
        public string Ruolo { get; set; } = "";

        public string Cognome { get; set; } = "";
        public string Nome { get; set; } = "";
        public string PercorsoFoto { get; set; } = "";
        public bool Visualizza { get; set; } = true;
    }
}
