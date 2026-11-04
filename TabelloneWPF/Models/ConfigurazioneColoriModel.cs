namespace TabelloneWPF
{
    public class ConfigurazioneColoriModel
    {
        public string SfondoGenerale { get; set; }
        public string SfondoSquadre { get; set; }
        public string SfondoNomeSq1 { get; set; }
        public string SfondoNomeSq2 { get; set; }
        public string ColoreCaratterePunti { get; set; }
        public string ColoriCarattereSet { get; set; }
        public string ColoreCarattereStorico { get; set; }
        public string ColoreCarattereSquadra1 { get; set; }
        public string ColoreCarattereSquadra2 { get; set; }
        public string ColoreGiocatori { get; set; }
        public string ColoreLiberi { get; set; }
        public string ColoreCapitano { get; set; }
        public string ColoreStaff { get; set; } = "#FFFFFF";

        public bool BordoPunti { get; set; }
        public bool BordoSet { get; set; }
        public bool BordoStorico { get; set; }
        public bool BordoSq1 { get; set; }
        public bool BordoSq2 { get; set; }
        public bool BordoGiocatori { get; set; }

        public double DimensioneGiocatori { get; set; } = 16.0;
        public double DimensioneStaff { get; set; } = 15.0;
        public double DimensioneNomiSquadre { get; set; } = 22.0;
        public double DimensionePunti { get; set; } = 80.0;
        public double DimensioneSet { get; set; } = 28.0;
        public double DimensioneStorico { get; set; } = 13.0;
        public double DimensioneSetCorrente { get; set; } = 18.0;

        public double PosizioneVerticalePunti { get; set; }
        public double PosizioneVerticaleSet { get; set; }
        public double PosizioneVerticaleStorico { get; set; }
        public double PosizioneVerticaleSetCorrente { get; set; }
    }
}
