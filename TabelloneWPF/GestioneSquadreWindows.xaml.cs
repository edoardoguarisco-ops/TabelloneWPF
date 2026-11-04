using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace TabelloneWPF
{
    public class GiocatoreInputVM
    {
        public string Numero { get; set; }
        public string Cognome { get; set; } // <--- Aggiungi questa se manca!
        public string Nome { get; set; }
        public bool Visualizza { get; set; }
        public bool IsStaff { get; set; }
        public bool IsLibero { get; set; }
        public bool IsCapitano { get; set; }
        public string PercorsoFoto { get; set; }
        public Brush ColoreFoto { get; set; } = Brushes.White;
    }

    public partial class GestioneSquadreWindow : Window
    {
        private MainWindow _tabellone;
        private readonly ConfigurazioneService _configurazioneService = new();
        public ObservableCollection<GiocatoreInputVM> ListaInput1 { get; set; } = new ObservableCollection<GiocatoreInputVM>();
        public ObservableCollection<GiocatoreInputVM> ListaInput2 { get; set; } = new ObservableCollection<GiocatoreInputVM>();

        private string percorsoLogo1 = "";
        private string percorsoLogo2 = "";

        public GestioneSquadreWindow(MainWindow tabellone, string nome1, string nome2, List<GiocatoreItem> rosa1, List<GiocatoreItem> rosa2, string logo1 = "", string logo2 = "")
        {
            InitializeComponent();
            _tabellone = tabellone;

            string percorsoXml = _configurazioneService.PercorsoConfigurazioneSquadre;
            bool caricatoDaFile = false;

            if (File.Exists(percorsoXml))
            {
                try
                {
                    ConfigClassXML model = _configurazioneService.Carica<ConfigClassXML>(percorsoXml);
                    if (model == null) throw new InvalidDataException("Configurazione squadre non valida.");

                    TxtNomeSquadra1.Text = model.NomeSquadra1 ?? nome1;
                    TxtNomeSquadra2.Text = model.NomeSquadra2 ?? nome2;

                    percorsoLogo1 = OttieniPercorsoAssoluto(model.LogoSquadra1) ?? logo1;
                    percorsoLogo2 = OttieniPercorsoAssoluto(model.LogoSquadra2) ?? logo2;

                    var rosaCaricata1 = new List<GiocatoreItem>();
                    if (model.Rosa1 != null)
                    {
                        foreach (var item in model.Rosa1)
                        {
                            rosaCaricata1.Add(new GiocatoreItem
                            {
                                Numero = item.Numero,
                                Cognome = item.Cognome,
                                Nome = item.Nome,
                                Visualizza = item.Visualizza,
                                IsLibero = item.IsLibero,       
                                IsCapitano = item.IsCapitano,   
                                PercorsoFoto = OttieniPercorsoAssoluto(item.PercorsoFoto),
                                IsStaff = item.IsStaff,
                                TestoVisualizzato = $"{item.Numero} {item.Nome}".Trim()
                            });
                        }
                    }

                    var rosaCaricata2 = new List<GiocatoreItem>();
                    if (model.Rosa2 != null)
                    {
                        foreach (var item in model.Rosa2)
                        {
                            rosaCaricata2.Add(new GiocatoreItem
                            {
                                Numero = item.Numero,
                                Cognome = item.Cognome,
                                Nome = item.Nome,
                                Visualizza = item.Visualizza,
                                IsLibero = item.IsLibero,       
                                IsCapitano = item.IsCapitano,  
                                PercorsoFoto = OttieniPercorsoAssoluto(item.PercorsoFoto),
                                IsStaff = item.IsStaff,
                                TestoVisualizzato = $"{item.Numero} {item.Nome}".Trim()
                            });
                        }
                    }

                    PopolaSlotRosa(ListaInput1, rosaCaricata1);
                    PopolaSlotRosa(ListaInput2, rosaCaricata2);
                    caricatoDaFile = true;
                }
                catch
                {
                    // Fallback in caso di errore
                }
            }

            if (!caricatoDaFile)
            {
                TxtNomeSquadra1.Text = nome1;
                TxtNomeSquadra2.Text = nome2;
                percorsoLogo1 = logo1 ?? "";
                percorsoLogo2 = logo2 ?? "";

                PopolaSlotRosa(ListaInput1, rosa1);
                PopolaSlotRosa(ListaInput2, rosa2);
            }

            ItemsRosa1.ItemsSource = ListaInput1;
            ItemsRosa2.ItemsSource = ListaInput2;
        }

        private string OttieniPercorsoAssoluto(string percorsoSalvato)
        {
            if (string.IsNullOrEmpty(percorsoSalvato)) return "";

            // 1. Controlla all'interno della cartella lastconfig (visto che ora salviamo solo il nome del file)
            string percorsoInLastConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastconfig", percorsoSalvato);
            if (File.Exists(percorsoInLastConfig)) return percorsoInLastConfig;

            // 2. Controlla se è un percorso assoluto esistente
            if (File.Exists(percorsoSalvato)) return percorsoSalvato;

            // 3. Fallback generico
            string percorsoUnito = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, percorsoSalvato);
            if (File.Exists(percorsoUnito)) return percorsoUnito;

            return percorsoSalvato;
        }

        private void PopolaSlotRosa(ObservableCollection<GiocatoreInputVM> targetList, List<GiocatoreItem> rosaEsistente)
        {
            var giocatoriList = new List<GiocatoreItem>();
            var staffList = new List<GiocatoreItem>();

            if (rosaEsistente != null)
            {
                foreach (var item in rosaEsistente)
                {
                    bool eStaff = item.IsStaff || (item.TestoVisualizzato != null &&
                                  (item.TestoVisualizzato.StartsWith("1°") ||
                                   item.TestoVisualizzato.StartsWith("2°") ||
                                   item.TestoVisualizzato.StartsWith("DRG")));

                    if (eStaff)
                    {
                        staffList.Add(item);
                    }
                    else
                    {
                        giocatoriList.Add(item);
                    }
                }
            }

            // POPOLIAMO I 18 SLOT DI GIOCO RIGOROSAMENTE 1 A 1 IN SEQUENZA DAL FILE XML
            for (int i = 0; i < 18; i++)
            {
                string num = (i + 1).ToString();
                string cogn = "";
                string nom = "";
                bool vis = true;
                bool lib = false;
                bool cap = false;
                string foto = "";

                if (i < giocatoriList.Count && giocatoriList[i] != null)
                {
                    var item = giocatoriList[i];

                    num = !string.IsNullOrEmpty(item.Numero) ? item.Numero : (i + 1).ToString();
                    cogn = item.Cognome ?? "";
                    nom = item.Nome ?? "";
                    vis = item.Visualizza;
                    lib = item.IsLibero;
                    cap = item.IsCapitano;
                    foto = item.PercorsoFoto ?? "";

                    if (string.IsNullOrEmpty(nom) && !string.IsNullOrEmpty(item.TestoVisualizzato))
                    {
                        var parti = item.TestoVisualizzato.Split(new char[] { ' ' }, 2);
                        if (parti.Length > 1) nom = parti[1];
                    }
                }

                Brush colorePulsante = FotoEsistente(foto) ? Brushes.LightGreen : Brushes.White;

                targetList.Add(new GiocatoreInputVM
                {
                    Numero = num,
                    Cognome = cogn,
                    Nome = nom,
                    IsStaff = false,
                    Visualizza = vis,
                    IsLibero = lib,
                    IsCapitano = cap,
                    PercorsoFoto = foto,
                    ColoreFoto = colorePulsante
                });
            }

            // POPOLAMENTO DELLO STAFF
            string[] etichetteStaff = { "1° AL", "2° AL", "DRG" };
            foreach (var etichetta in etichetteStaff)
            {
                string cognomStaff = "";
                string nomeStaff = "";
                bool visStaff = true;
                string fotoStaff = "";

                foreach (var item in staffList)
                {
                    if (item.Numero == etichetta || (item.TestoVisualizzato != null && item.TestoVisualizzato.StartsWith(etichetta)))
                    {
                        visStaff = item.Visualizza;
                        fotoStaff = item.PercorsoFoto ?? "";
                        cognomStaff = item.Cognome ?? "";

                        if (!string.IsNullOrEmpty(item.Nome) && item.Nome != etichetta)
                        {
                            nomeStaff = item.Nome;
                        }
                        else if (item.TestoVisualizzato != null && item.TestoVisualizzato.Length > etichetta.Length)
                        {
                            nomeStaff = item.TestoVisualizzato.Substring(etichetta.Length).Trim();
                        }
                        break;
                    }
                }

                Brush colorePulsanteStaff = FotoEsistente(fotoStaff) ? Brushes.LightGreen : Brushes.White;

                targetList.Add(new GiocatoreInputVM
                {
                    Numero = etichetta,
                    Cognome = cognomStaff,
                    Nome = nomeStaff,
                    IsStaff = true,
                    Visualizza = visStaff,
                    IsLibero = false,
                    IsCapitano = false,
                    PercorsoFoto = fotoStaff,
                    ColoreFoto = colorePulsanteStaff
                });
            }
        }

        private void Capitano_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is GiocatoreInputVM giocatore)
            {
                if (cb.IsChecked == true)
                {
                    var listaCorrente = ListaInput1.Contains(giocatore) ? ListaInput1 : ListaInput2;

                    foreach (var g in listaCorrente)
                    {
                        if (g != giocatore)
                        {
                            g.IsCapitano = false;
                        }
                    }
                }
            }
        }

        private void Libero_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is GiocatoreInputVM giocatore)
            {
                var listaCorrente = ListaInput1.Contains(giocatore) ? ListaInput1 : ListaInput2;

                int countLiberi = 0;
                foreach (var g in listaCorrente)
                {
                    if (g.IsLibero) countLiberi++;
                }

                if (countLiberi > 2)
                {
                    giocatore.IsLibero = false;
                    cb.IsChecked = false;
                    MessageBox.Show("Non è possibile selezionare più di 2 liberi per squadra.", "Limite Liberi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private bool FotoEsistente(string percorso)
        {
            if (string.IsNullOrWhiteSpace(percorso))
                return false;

            string percorsoRisolto = _configurazioneService
                .RisolviPercorsoFileConfigurazione(percorso);
            return File.Exists(percorsoRisolto);
        }

        private string TrovaFotoAtletaDefault()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] cartelleDaCercare =
            {
                Path.Combine(baseDirectory, "DATA", "DEF7fotoatleta"),
                Path.Combine(baseDirectory, "DEF7fotoatleta"),
                Path.Combine(baseDirectory, "DATA", "DEF"),
                Path.Combine(baseDirectory, "DEF")
            };

            foreach (string cartellaDefault in cartelleDaCercare)
            {
                if (!Directory.Exists(cartellaDefault))
                    continue;

                foreach (string file in Directory.GetFiles(
                    cartellaDefault, "*.*", SearchOption.AllDirectories))
                {
                    string estensione = Path.GetExtension(file).ToLowerInvariant();
                    if (estensione == ".png" || estensione == ".jpg" || estensione == ".jpeg")
                        return file;
                }
            }

            return "";
        }

        private void ImpostaAnteprimaTooltip(Button button, string percorso)
        {
            try
            {
                string percorsoRisolto = _configurazioneService
                    .RisolviPercorsoFileConfigurazione(percorso);
                if (!File.Exists(percorsoRisolto))
                {
                    button.ToolTip = "Seleziona foto";
                    return;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Path.GetFullPath(percorsoRisolto), UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                var immagine = new Image
                {
                    Source = bitmap,
                    Width = 260,
                    Height = 260,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(8)
                };

                var tooltip = new ToolTip
                {
                    Content = immagine,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    StaysOpen = true
                };

                ToolTipService.SetInitialShowDelay(button, 2000);
                ToolTipService.SetShowDuration(button, int.MaxValue);
                button.ToolTip = tooltip;
            }
            catch
            {
                button.ToolTip = "Seleziona foto";
            }
        }

        private void BtnCaricaFoto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is GiocatoreInputVM giocatore)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Immagini (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
                if (openFileDialog.ShowDialog() == true)
                {
                    giocatore.PercorsoFoto = openFileDialog.FileName;
                    giocatore.ColoreFoto = Brushes.LightGreen;
                    btn.Foreground = Brushes.LightGreen;
                    ImpostaAnteprimaTooltip(btn, giocatore.PercorsoFoto);
                }
            }
        }

        private void BtnFoto_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not GiocatoreInputVM giocatore)
                return;

            bool presente = FotoEsistente(giocatore.PercorsoFoto);
            giocatore.ColoreFoto = presente ? Brushes.LightGreen : Brushes.White;
            button.Foreground = giocatore.ColoreFoto;

            if (presente)
                ImpostaAnteprimaTooltip(button, giocatore.PercorsoFoto);
        }

        private void BtnLogo_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            string percorso = button == BtnLogo1 ? percorsoLogo1 : percorsoLogo2;
            button.Foreground = FotoEsistente(percorso) ? Brushes.LightGreen : Brushes.White;

            if (FotoEsistente(percorso))
                ImpostaAnteprimaTooltip(button, percorso);
        }

        private void BtnCaricaLogo1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Immagini (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                percorsoLogo1 = openFileDialog.FileName;
                if (sender is Button btn)
                {
                    btn.Foreground = Brushes.LightGreen;
                    ImpostaAnteprimaTooltip(btn, percorsoLogo1);
                }
            }
        }

        private void BtnCaricaLogo2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Immagini (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                percorsoLogo2 = openFileDialog.FileName;
                if (sender is Button btn)
                {
                    btn.Foreground = Brushes.LightGreen;
                    ImpostaAnteprimaTooltip(btn, percorsoLogo2);
                }
            }
        }

        private string ElaboraLogoSalvataggio(string percorsoSorgente, string nomeDefault)
        {
            if (string.IsNullOrEmpty(percorsoSorgente)) return "";

            string cartellaInfoPartita = Path.Combine(
                _configurazioneService.CartellaConfigurazione,
                "InfoPartita");
            Directory.CreateDirectory(cartellaInfoPartita);

            string sorgenteRisolta = _configurazioneService
                .RisolviPercorsoFileConfigurazione(percorsoSorgente);
            if (!File.Exists(sorgenteRisolta))
                return percorsoSorgente;

            string estensione = Path.GetExtension(sorgenteRisolta);
            string nomeFile = nomeDefault + estensione;
            string destinazione = Path.Combine(cartellaInfoPartita, nomeFile);

            if (!string.Equals(Path.GetFullPath(sorgenteRisolta), Path.GetFullPath(destinazione),
                StringComparison.OrdinalIgnoreCase))
            {
                foreach (var vecchioFile in Directory.GetFiles(cartellaInfoPartita, nomeDefault + ".*"))
                {
                    try { File.Delete(vecchioFile); } catch { }
                }

                File.Copy(sorgenteRisolta, destinazione, true);
            }

            return Path.Combine("InfoPartita", nomeFile);
        }

        private string ElaboraFotoSalvataggio(string percorsoSorgente, string nomeFileCustom, int numeroSquadra)
        {
            if (string.IsNullOrEmpty(percorsoSorgente)) return "";

            string cartellaSquadra = Path.Combine(
                _configurazioneService.CartellaConfigurazione,
                $"Squadra{numeroSquadra}");
            Directory.CreateDirectory(cartellaSquadra);

            string sorgenteRisolta = _configurazioneService
                .RisolviPercorsoFileConfigurazione(percorsoSorgente);
            if (!File.Exists(sorgenteRisolta))
                return percorsoSorgente;

            string estensione = Path.GetExtension(sorgenteRisolta);
            string nomeFile = nomeFileCustom + estensione;
            string destinazione = Path.Combine(cartellaSquadra, nomeFile);

            if (!string.Equals(Path.GetFullPath(sorgenteRisolta), Path.GetFullPath(destinazione),
                StringComparison.OrdinalIgnoreCase))
            {
                foreach (var vecchioFile in Directory.GetFiles(cartellaSquadra, nomeFileCustom + ".*"))
                {
                    try { File.Delete(vecchioFile); } catch { }
                }

                File.Copy(sorgenteRisolta, destinazione, true);
            }

            return Path.Combine($"Squadra{numeroSquadra}", nomeFile);
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            string nuovoNome1 = TxtNomeSquadra1.Text;
            string nuovoNome2 = TxtNomeSquadra2.Text;

            string logo1Elaborato = ElaboraLogoSalvataggio(percorsoLogo1, "LOGO1");
            string logo2Elaborato = ElaboraLogoSalvataggio(percorsoLogo2, "LOGO2");

            List<GiocatoreItem> nuovaRosa1 = CostruisciRosaDaLista(ListaInput1, 1);
            List<GiocatoreItem> nuovaRosa2 = CostruisciRosaDaLista(ListaInput2, 2);

            _tabellone.AggiornaConfigurazioneSquadre(nuovoNome1, nuovoNome2, nuovaRosa1, nuovaRosa2, logo1Elaborato, logo2Elaborato);

            MessageBox.Show("Squadre e rose aggiornate con successo!", "Gestione Squadre", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private List<GiocatoreItem> CostruisciRosaDaLista(ObservableCollection<GiocatoreInputVM> listaInput, int numeroSquadra)
        {
            List<GiocatoreItem> nuovaRosa = new List<GiocatoreItem>();

            Brush coloreGiocatoriDaFile = Brushes.White;
            try
            {
                string percorsoColori = _configurazioneService.PercorsoConfigurazioneColori;
                if (File.Exists(percorsoColori))
                {
                    ConfigurazioneColoriModel modelCol = _configurazioneService.Carica<ConfigurazioneColoriModel>(percorsoColori);
                    if (modelCol == null) throw new InvalidDataException("Configurazione colori non valida.");
                    if (!string.IsNullOrEmpty(modelCol.ColoreGiocatori))
                    {
                        var col = (Color)ColorConverter.ConvertFromString(modelCol.ColoreGiocatori);
                        coloreGiocatoriDaFile = new SolidColorBrush(col);
                    }
                }
            }
            catch { }

            if (listaInput != null)
            {
                int indiceGiocatore = 1;
                int indiceStaff = 1;
                string fotoAtletaDefault = TrovaFotoAtletaDefault();

                foreach (var g in listaInput)
                {
                    string testoFinale = $"{g.Numero} {g.Nome}".Trim();
                    string nomeFileFoto;
                    int indiceFoto;

                    if (g.IsStaff)
                    {
                        indiceFoto = indiceStaff++;
                        nomeFileFoto = $"S{numeroSquadra}_{indiceFoto}";
                    }
                    else
                    {
                        indiceFoto = indiceGiocatore++;
                        nomeFileFoto = $"G{numeroSquadra}_{indiceFoto}";
                    }

                    bool atletaCompilato =
                        !string.IsNullOrWhiteSpace(g.Cognome) ||
                        !string.IsNullOrWhiteSpace(g.Nome);

                    string fotoDaSalvare = FotoEsistente(g.PercorsoFoto)
                        ? g.PercorsoFoto
                        : atletaCompilato ? fotoAtletaDefault : "";

                    string fotoElaborata = ElaboraFotoSalvataggio(
                        fotoDaSalvare,
                        nomeFileFoto,
                        numeroSquadra);

                    nuovaRosa.Add(new GiocatoreItem
                    {
                        Numero = g.Numero ?? "",
                        Cognome = g.Cognome ?? "",    
                        Nome = g.Nome ?? "",          
                        TestoVisualizzato = testoFinale,
                        Visualizza = g.Visualizza,
                        IsLibero = g.IsLibero,
                        IsCapitano = g.IsCapitano,
                        ForegroundColor = g.IsStaff
                            ? Brushes.White
                            : coloreGiocatoriDaFile,
                        PercorsoFoto = fotoElaborata ?? "",
                        IsStaff = g.IsStaff
                    });
                }
            }
            return nuovaRosa;
        }
    }
}