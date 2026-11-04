using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace TabelloneWPF
{
    public partial class InfoPartitaWindow : Window
    {
        private readonly ConfigurazioneService _configurazioneService = new();
        private readonly InformazioniPartitaModel _modello;

        public InfoPartitaWindow(MainWindow tabellonePrincipale)
        {
            InitializeComponent();

            _modello = _configurazioneService.Carica<InformazioniPartitaModel>(_configurazioneService.PercorsoInformazioniPartita)
                ?? CreaModelloIniziale();

            TxtNomeSquadra1.Text = tabellonePrincipale.GetNomeSquadra1();
            TxtNomeSquadra2.Text = tabellonePrincipale.GetNomeSquadra2();

            var configurazione = _configurazioneService.Carica<ConfigClassXML>(_configurazioneService.PercorsoConfigurazioneSquadre);
            if (configurazione != null)
            {
                _modello.LogoSquadra1 = configurazione.LogoSquadra1 ?? "";
                _modello.LogoSquadra2 = configurazione.LogoSquadra2 ?? "";
                ImpostaImmagine(ImgLogoSquadra1, configurazione.LogoSquadra1);
                ImpostaImmagine(ImgLogoSquadra2, configurazione.LogoSquadra2);
            }

            _modello.NomeSquadra1 = TxtNomeSquadra1.Text;
            _modello.NomeSquadra2 = TxtNomeSquadra2.Text;
            CompletaRuoliPreimpostati();
            ImpostaFotoDefaultUfficiali();

            PickerDataPartita.SelectedDate = DateTime.TryParse(_modello.DataPartita, out var data) ? data : DateTime.Today;
            TxtOraInizio.Text = string.IsNullOrWhiteSpace(_modello.OraInizio) ? "20:30" : _modello.OraInizio;
            ItemsUfficiali.ItemsSource = _modello.Ufficiali;
        }

        private static InformazioniPartitaModel CreaModelloIniziale()
        {
            var modello = new InformazioniPartitaModel();
            string[] ruoli = { "REFERTISTA", "ADDETTO ARBITRO", "SEGNAPUNTI", "PRIMO ARBITRO", "SECONDO ARBITRO" };
            modello.Ufficiali = ruoli.Select(ruolo => new UfficialePartitaModel { Ruolo = ruolo }).ToList();
            return modello;
        }

        private void CompletaRuoliPreimpostati()
        {
            string[] ruoli = { "REFERTISTA", "ADDETTO ARBITRO", "SEGNAPUNTI", "PRIMO ARBITRO", "SECONDO ARBITRO" };
            var ufficialiEsistenti = _modello.Ufficiali ?? new List<UfficialePartitaModel>();
            _modello.Ufficiali = ruoli.Select(ruolo => ufficialiEsistenti.FirstOrDefault(u => u.Ruolo == ruolo)
                ?? new UfficialePartitaModel { Ruolo = ruolo }).ToList();
        }

        private void ImpostaFotoDefaultUfficiali()
        {
            string? fotoDefault = TrovaFotoStaffDefault();
            if (fotoDefault == null) return;

            foreach (var ufficiale in _modello.Ufficiali)
            {
                if (string.IsNullOrWhiteSpace(ufficiale.PercorsoFoto) ||
                    !File.Exists(_configurazioneService.RisolviPercorsoFileConfigurazione(ufficiale.PercorsoFoto)))
                {
                    ufficiale.PercorsoFoto = fotoDefault;
                }
            }
        }

        private string? TrovaFotoStaffDefault()
        {
            string cartellaDef = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DEF");
            string[] estensioni = { ".png", ".jpg", ".jpeg" };

            foreach (string estensione in estensioni)
            {
                string percorso = Path.Combine(cartellaDef, "fotostaffdefault" + estensione);
                if (File.Exists(percorso)) return percorso;
            }

            return null;
        }

        private void BtnFotoUfficiale_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.DataContext is not UfficialePartitaModel ufficiale)
                return;

            bool fotoPersonalizzata = !string.IsNullOrWhiteSpace(ufficiale.PercorsoFoto) &&
                !EUnaFotoStaffDefault(ufficiale.PercorsoFoto) &&
                File.Exists(_configurazioneService.RisolviPercorsoFileConfigurazione(ufficiale.PercorsoFoto));

            button.Foreground = fotoPersonalizzata
                ? System.Windows.Media.Brushes.LightGreen
                : System.Windows.Media.Brushes.Gray;

            if (!string.IsNullOrWhiteSpace(ufficiale.PercorsoFoto))
                ImpostaAnteprimaTooltip(button, ufficiale.PercorsoFoto);
        }

        private void ImpostaImmagine(System.Windows.Controls.Image immagine, string percorso)
        {
            if (immagine == null || string.IsNullOrWhiteSpace(percorso)) return;

            string risolto = _configurazioneService.RisolviPercorsoFileConfigurazione(percorso);
            if (string.IsNullOrWhiteSpace(risolto)) return;

            string percorsoAssoluto = Path.GetFullPath(risolto);
            if (!File.Exists(percorsoAssoluto)) return;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(percorsoAssoluto, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            immagine.Source = bitmap;
        }

        private void BtnCaricaFotoUfficiale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.DataContext is not UfficialePartitaModel ufficiale)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "Immagini (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                ufficiale.PercorsoFoto = dialog.FileName;
                button.Foreground = System.Windows.Media.Brushes.LightGreen;
                ImpostaAnteprimaTooltip(button, dialog.FileName);
            }
        }

        private void ImpostaAnteprimaTooltip(System.Windows.Controls.Button button, string percorso)
        {
            try
            {
                string risolto = _configurazioneService.RisolviPercorsoFileConfigurazione(percorso);
                if (string.IsNullOrWhiteSpace(risolto)) return;

                string percorsoAssoluto = Path.GetFullPath(risolto);
                if (!File.Exists(percorsoAssoluto)) return;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(percorsoAssoluto, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                var immagine = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    Width = 260,
                    Height = 260,
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(8)
                };

                var tooltip = new System.Windows.Controls.ToolTip
                {
                    Content = immagine,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    // La proprietà ToolTip dei controlli WPF non supporta
                    // ToolTip con StaysOpen=false. La chiusura automatica
                    // è già gestita da ToolTipService.ShowDuration.
                    StaysOpen = true
                };

                System.Windows.Controls.ToolTipService.SetInitialShowDelay(button, 2000);
                System.Windows.Controls.ToolTipService.SetShowDuration(button, int.MaxValue);
                button.ToolTip = tooltip;
            }
            catch (Exception)
            {
                button.ToolTip = "Seleziona foto";
            }
        }

        private string ElaboraFotoUfficialeSalvataggio(string percorsoSorgente, int indice)
        {
            if (string.IsNullOrWhiteSpace(percorsoSorgente)) return "";
            if (EUnaFotoStaffDefault(percorsoSorgente)) return percorsoSorgente;

            string cartellaInfoPartita = Path.Combine(
                _configurazioneService.CartellaConfigurazione,
                "InfoPartita");
            Directory.CreateDirectory(cartellaInfoPartita);

            string percorsoSorgenteRisolto = _configurazioneService
                .RisolviPercorsoFileConfigurazione(percorsoSorgente);

            if (!File.Exists(percorsoSorgenteRisolto))
                return percorsoSorgente;

            string estensione = Path.GetExtension(percorsoSorgenteRisolto);
            string nomeFile = $"UFFICIALE_{indice}{estensione}";
            string percorsoDestinazione = Path.Combine(cartellaInfoPartita, nomeFile);
            string percorsoSorgenteAssoluto = Path.GetFullPath(percorsoSorgenteRisolto);
            string percorsoDestinazioneAssoluto = Path.GetFullPath(percorsoDestinazione);

            if (!string.Equals(percorsoSorgenteAssoluto, percorsoDestinazioneAssoluto,
                StringComparison.OrdinalIgnoreCase))
            {
                foreach (var vecchioFile in Directory.GetFiles(cartellaInfoPartita, $"UFFICIALE_{indice}.*"))
                    File.Delete(vecchioFile);

                File.Copy(percorsoSorgenteRisolto, percorsoDestinazione, true);
            }

            // Percorso relativo alla cartella lastconfig, risolto da ConfigurazioneService.
            return Path.Combine("InfoPartita", nomeFile);
        }

        private static bool EUnaFotoStaffDefault(string percorso)
        {
            string nomeFile = Path.GetFileNameWithoutExtension(percorso);
            return nomeFile.Equals("fotostaffdefault", StringComparison.OrdinalIgnoreCase) &&
                percorso.Contains(Path.DirectorySeparatorChar + "DEF" + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase);
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            _modello.NomeSquadra1 = TxtNomeSquadra1.Text.Trim();
            _modello.NomeSquadra2 = TxtNomeSquadra2.Text.Trim();
            _modello.DataPartita = PickerDataPartita.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
            _modello.OraInizio = TxtOraInizio.Text.Trim();

            for (int i = 0; i < _modello.Ufficiali.Count; i++)
                _modello.Ufficiali[i].PercorsoFoto = ElaboraFotoUfficialeSalvataggio(
                    _modello.Ufficiali[i].PercorsoFoto, i + 1);

            _configurazioneService.Salva(_configurazioneService.PercorsoInformazioniPartita, _modello);
            DialogResult = true;
            Close();
        }

        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
