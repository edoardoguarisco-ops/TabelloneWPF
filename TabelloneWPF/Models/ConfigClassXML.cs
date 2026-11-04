using System.Collections.Generic;
using System.Xml.Serialization;

namespace TabelloneWPF
{
    [XmlRoot("ConfigurazioneSquadre")]
    public class ConfigClassXML
    {
        public string NomeSquadra1 { get; set; } = "";
        public string LogoSquadra1 { get; set; } = "";
        public string NomeSquadra2 { get; set; } = "";
        public string LogoSquadra2 { get; set; } = "";

        [XmlArray("RosaSquadra1")]
        [XmlArrayItem("Membro")]
        public List<ItemXml> Rosa1 { get; set; } = new();

        [XmlArray("RosaSquadra2")]
        [XmlArrayItem("Membro")]
        public List<ItemXml> Rosa2 { get; set; } = new();
    }

    public class ItemXml
    {
        public string Numero { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Nome { get; set; } = "";
        public bool Visualizza { get; set; } = true;
        public bool IsStaff { get; set; }
        public string PercorsoFoto { get; set; } = "";
        public bool IsLibero { get; set; }
        public bool IsCapitano { get; set; }
    }
}
