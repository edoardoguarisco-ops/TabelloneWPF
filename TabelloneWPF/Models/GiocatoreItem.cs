using System.Windows.Media;

namespace TabelloneWPF
{
    public class GiocatoreItem
    {
        public string Numero { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Nome { get; set; } = "";
        public string TestoVisualizzato { get; set; } = "";
        public Brush ForegroundColor { get; set; } = Brushes.White;
        public bool Visualizza { get; set; } = true;
        public string PercorsoFoto { get; set; } = "";
        public bool IsStaff { get; set; }
        public bool IsLibero { get; set; }
        public bool IsCapitano { get; set; }

        public string NomeFormattatoTabellone
        {
            get
            {
                string cognome = string.IsNullOrWhiteSpace(Cognome) ? "" : Cognome.Trim();
                string nome = string.IsNullOrWhiteSpace(Nome) ? "" : Nome.Trim();

                if (cognome.Length > 0 && nome.Length > 0)
                    return $"{cognome} {nome[0]}.";

                return cognome.Length > 0 ? cognome : nome;
            }
        }

        public string NomeCompletoPresentazione
        {
            get
            {
                string nome = string.IsNullOrWhiteSpace(Nome) ? "" : Nome.Trim();
                string cognome = string.IsNullOrWhiteSpace(Cognome) ? "" : Cognome.Trim();

                if (nome.Length > 0 && cognome.Length > 0)
                    return $"{nome} {cognome}";

                return nome.Length > 0 ? nome : cognome;
            }
        }
    }
}
