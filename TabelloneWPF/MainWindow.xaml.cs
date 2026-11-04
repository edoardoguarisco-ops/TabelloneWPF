using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace TabelloneWPF
{
    public partial class MainWindow : Window
    {
        private ConsoleControllo _consoleWindow;
        private readonly ConfigurazioneService _configurazioneService = new();
        private readonly PunteggioService _punteggioService = new();
        private readonly AnimazioniService _animazioniService = new();
        private Brush _coloreGiocatori = Brushes.White;
        private Brush _coloreSquadra1 = Brushes.White;
        private Brush _coloreSquadra2 = Brushes.White;

        private List<GiocatoreItem> _rosaCompleta1 = new List<GiocatoreItem>();
        private List<GiocatoreItem> _rosaCompleta2 = new List<GiocatoreItem>();

        public bool IsSponsorRunning() => _animazioniService.IsSponsorRunning;
        public bool IsPresentazioneRunning() => _animazioniService.IsPresentazioneRunning;
        public List<GiocatoreItem> GetRosa1() => _rosaCompleta1;
        public List<GiocatoreItem> GetRosa2() => _rosaCompleta2;
        public string GetNomeSquadra1() => TxtSquadra1.Text;
        public string GetNomeSquadra2() => TxtSquadra2.Text;

        public MainWindow()
        {
            InitializeComponent();
            FinestraConfig.InizializzaSponsorAutomatici();
            CaricaUltimaConfigurazioneDaFile();
            CaricaColoriDaFile();
            CaricaDimensioniDaFile();
            AggiornaInterfaccia();

            _consoleWindow = new ConsoleControllo(this);
            _consoleWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            _consoleWindow.Left = 100;
            _consoleWindow.Top = 150;
            _consoleWindow.Show();

            _consoleWindow.AggiornaTestiPulsanti(TxtSquadra1.Text, TxtSquadra2.Text);
        }

        // Metodo collegato al pulsante ROSE (avvia o interrompe la presentazione)
        private void BtnRose_Click(object sender, RoutedEventArgs e)
        {
            if (_animazioniService.IsPresentazioneRunning)
            {
                InterrompiPresentazione();
            }
            else
            {
                _ = AvviaPresentazioneRoseAsync();
            }
        }

        public async Task AvviaPresentazionePartitaAsync()
        {
            InformazioniPartitaModel? informazioni = _configurazioneService
                .Carica<InformazioniPartitaModel>(_configurazioneService.PercorsoInformazioniPartita);

            string nomeSquadra1 = string.IsNullOrWhiteSpace(informazioni?.NomeSquadra1)
                ? TxtSquadra1.Text
                : informazioni.NomeSquadra1;
            string nomeSquadra2 = string.IsNullOrWhiteSpace(informazioni?.NomeSquadra2)
                ? TxtSquadra2.Text
                : informazioni.NomeSquadra2;
            string oraInizio = string.IsNullOrWhiteSpace(informazioni?.OraInizio)
                ? ""
                : informazioni.OraInizio;

            var sponsor = new List<string>();
            foreach (string percorso in FinestraConfig.PercorsiSponsor)
            {
                string percorsoRisolto = Path.IsPathRooted(percorso)
                    ? percorso
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, percorso);

                if (File.Exists(percorsoRisolto))
                    sponsor.Add(percorsoRisolto);
            }

            try
            {
                await _animazioniService.EseguiPresentazioneAsync(async token =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        GridPresentazionePartita.Visibility = Visibility.Visible;
                        TxtNomePresentazionePartita1.Text = nomeSquadra1;
                        TxtNomePresentazionePartita2.Text = nomeSquadra2;
                        TxtOraPresentazionePartita.Text = oraInizio;
                        ImgLogoPresentazionePartita1.Source = ImgLogoSquadra1.Source;
                        ImgLogoPresentazionePartita2.Source = ImgLogoSquadra2.Source;

                        if (!string.IsNullOrWhiteSpace(informazioni?.LogoSquadra1))
                        {
                            string logo1 = _configurazioneService
                                .RisolviPercorsoFileConfigurazione(informazioni.LogoSquadra1);
                            if (File.Exists(logo1))
                                ImpostaLogo(ImgLogoPresentazionePartita1, logo1);
                        }

                        if (!string.IsNullOrWhiteSpace(informazioni?.LogoSquadra2))
                        {
                            string logo2 = _configurazioneService
                                .RisolviPercorsoFileConfigurazione(informazioni.LogoSquadra2);
                            if (File.Exists(logo2))
                                ImpostaLogo(ImgLogoPresentazionePartita2, logo2);
                        }

                        ImgSponsorPresentazionePartitaA.Opacity = 0;
                        ImgSponsorPresentazionePartitaA.Source = null;
                        ImgSponsorPresentazionePartitaA.RenderTransform = null;
                        ImgSponsorPresentazionePartitaB.Opacity = 0;
                        ImgSponsorPresentazionePartitaB.Source = null;
                        ImgSponsorPresentazionePartitaB.RenderTransform = null;
                    });

                    // Lascia il tempo di visualizzare prima l'intestazione partita.
                    await Task.Delay(2500, token);

                    if (sponsor.Count == 0)
                    {
                        await Task.Delay(3000, token);
                        return;
                    }

                    Image? immagineCorrente = null;
                    bool usaPrimaImmagine = true;

                    foreach (string percorso in sponsor)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(percorso, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            Image immagineSuccessiva = usaPrimaImmagine
                                ? ImgSponsorPresentazionePartitaA
                                : ImgSponsorPresentazionePartitaB;

                            Dispatcher.Invoke(() =>
                            {
                                immagineSuccessiva.Source = bitmap;
                                immagineSuccessiva.RenderTransform = new TranslateTransform();
                                immagineSuccessiva.Opacity = 0;
                            });

                            if (immagineCorrente == null)
                            {
                                await AnimaIngressoSponsorAsync(immagineSuccessiva, token);
                            }
                            else
                            {
                                await Task.WhenAll(
                                    AnimaIngressoSponsorAsync(immagineSuccessiva, token),
                                    AnimaUscitaSponsorAsync(immagineCorrente, token));
                            }

                            immagineCorrente = immagineSuccessiva;
                            usaPrimaImmagine = !usaPrimaImmagine;
                            await Task.Delay(2500, token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                        catch
                        {
                            // Ignora il singolo sponsor non caricabile e continua.
                        }
                    }
                });
            }
            catch (TaskCanceledException)
            {
                // Presentazione interrotta dall'utente.
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    GridPresentazionePartita.Visibility = Visibility.Collapsed;
                    ImgSponsorPresentazionePartitaA.Source = null;
                    ImgSponsorPresentazionePartitaA.Opacity = 0;
                    ImgSponsorPresentazionePartitaA.RenderTransform = null;
                    ImgSponsorPresentazionePartitaB.Source = null;
                    ImgSponsorPresentazionePartitaB.Opacity = 0;
                    ImgSponsorPresentazionePartitaB.RenderTransform = null;
                });
            }
        }

        private Task AnimaIngressoSponsorAsync(Image immagine, System.Threading.CancellationToken token)
        {
            double posizioneIniziale = BorderSponsorPresentazionePartita.ActualWidth;
            if (posizioneIniziale <= 0)
                posizioneIniziale = ActualWidth > 0 ? ActualWidth : 1200;

            return AnimaTraslazioneSponsorAsync(immagine, posizioneIniziale, 0, token, true);
        }

        private Task AnimaUscitaSponsorAsync(Image immagine, System.Threading.CancellationToken token)
        {
            double posizioneFinale = -(immagine.Width + 40);
            return AnimaTraslazioneSponsorAsync(immagine, 0, posizioneFinale, token, false);
        }

        private async Task AnimaTraslazioneSponsorAsync(
            Image immagine,
            double da,
            double a,
            System.Threading.CancellationToken token,
            bool mostrare)
        {
            var trasformazione = immagine.RenderTransform as TranslateTransform
                ?? new TranslateTransform();
            trasformazione.X = da;
            immagine.RenderTransform = trasformazione;
            immagine.Opacity = mostrare ? 1 : immagine.Opacity;

            var completamento = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var animazione = new DoubleAnimation
            {
                From = da,
                To = a,
                Duration = TimeSpan.FromSeconds(1.5)
            };

            animazione.Completed += (_, _) => completamento.TrySetResult(true);
            trasformazione.BeginAnimation(TranslateTransform.XProperty, animazione);

            using (token.Register(() =>
            {
                trasformazione.BeginAnimation(TranslateTransform.XProperty, null);
                completamento.TrySetCanceled(token);
            }))
            {
                await completamento.Task;
            }

            if (!mostrare)
                immagine.Opacity = 0;
        }

        public async Task AvviaPresentazioneRoseAsync()
        {
            try
            {
                await _animazioniService.EseguiPresentazioneAsync(async token =>
                {
                    await PresentaRosaSquadraAsync(TxtSquadra1.Text, _rosaCompleta1, true, token);

                    if (token.IsCancellationRequested) return;
                    await Task.Delay(2000, token);

                    await PresentaRosaSquadraAsync(TxtSquadra2.Text, _rosaCompleta2, false, token);
                });
            }
            catch (TaskCanceledException)
            {
                // Presentazione interrotta
            }
            finally
            {
                Dispatcher.Invoke(() => {
                    if (GridPresentazione != null) GridPresentazione.Visibility = Visibility.Collapsed;
                });
            }
        }

        private async Task PresentaRosaSquadraAsync(string nomeSquadra, List<GiocatoreItem> rosaCompleta, bool èSquadra1, System.Threading.CancellationToken token)
        {
            if (rosaCompleta == null || rosaCompleta.Count == 0) return;

            var giocatoriVisibili = new List<GiocatoreItem>();
            var staffVisibili = new List<GiocatoreItem>();

            foreach (var g in rosaCompleta)
            {
                if (g.Visualizza)
                {
                    if (g.IsStaff) staffVisibili.Add(g);
                    else giocatoriVisibili.Add(g);
                }
            }

            if (giocatoriVisibili.Count == 0 && staffVisibili.Count == 0) return;

            Dispatcher.Invoke(() =>
            {
                if (GridPresentazione != null) GridPresentazione.Visibility = Visibility.Visible;
                if (TxtNomeSquadraPresentazione != null) TxtNomeSquadraPresentazione.Text = nomeSquadra;
                    if (TxtNomeSquadraPresentazione != null)
                        TxtNomeSquadraPresentazione.Foreground = èSquadra1 ? _coloreSquadra1 : _coloreSquadra2;

                if (èSquadra1)
                {
                    if (ListPresentazioneRosa1 != null) { ListPresentazioneRosa1.ItemsSource = giocatoriVisibili; ListPresentazioneRosa1.Visibility = Visibility.Visible; }
                    if (ItemsStaffPres1 != null) { ItemsStaffPres1.ItemsSource = staffVisibili; ItemsStaffPres1.Visibility = Visibility.Visible; }

                    if (ListPresentazioneRosa2 != null) ListPresentazioneRosa2.Visibility = Visibility.Collapsed;
                    if (ItemsStaffPres2 != null) ItemsStaffPres2.Visibility = Visibility.Collapsed;

                    if (ImgLogoPresSquadra1 != null)
                    {
                        ImgLogoPresSquadra1.Source = ImgLogoSquadra1.Source;
                        ImgLogoPresSquadra1.Visibility = Visibility.Visible;
                    }
                     if (TxtNumeroLogoPresSquadra1 != null) TxtNumeroLogoPresSquadra1.Visibility = Visibility.Visible;
                     if (TxtNumeroLogoPresSquadra2 != null) TxtNumeroLogoPresSquadra2.Visibility = Visibility.Collapsed;
                    if (ImgLogoPresSquadra2 != null) ImgLogoPresSquadra2.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (ListPresentazioneRosa2 != null) { ListPresentazioneRosa2.ItemsSource = giocatoriVisibili; ListPresentazioneRosa2.Visibility = Visibility.Visible; }
                    if (ItemsStaffPres2 != null) { ItemsStaffPres2.ItemsSource = staffVisibili; ItemsStaffPres2.Visibility = Visibility.Visible; }

                    if (ListPresentazioneRosa1 != null) ListPresentazioneRosa1.Visibility = Visibility.Collapsed;
                    if (ItemsStaffPres1 != null) ItemsStaffPres1.Visibility = Visibility.Collapsed;

                    if (ImgLogoPresSquadra2 != null)
                    {
                        ImgLogoPresSquadra2.Source = ImgLogoSquadra2.Source;
                        ImgLogoPresSquadra2.Visibility = Visibility.Visible;
                    }
                     if (TxtNumeroLogoPresSquadra2 != null) TxtNumeroLogoPresSquadra2.Visibility = Visibility.Visible;
                     if (TxtNumeroLogoPresSquadra1 != null) TxtNumeroLogoPresSquadra1.Visibility = Visibility.Collapsed;
                    if (ImgLogoPresSquadra1 != null) ImgLogoPresSquadra1.Visibility = Visibility.Collapsed;
                }
            });

            var elementiPresentazione = new List<GiocatoreItem>(giocatoriVisibili);
            elementiPresentazione.AddRange(staffVisibili);

            // Presenta prima i giocatori e poi lo staff nella stessa sequenza
            foreach (var giocatore in elementiPresentazione)
            {
                if (token.IsCancellationRequested) break;

                if (!giocatore.IsStaff)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (èSquadra1 && ListPresentazioneRosa1 != null)
                        {
                            ListPresentazioneRosa1.SelectedItem = giocatore;
                            ListPresentazioneRosa1.ScrollIntoView(giocatore);
                        }
                        else if (!èSquadra1 && ListPresentazioneRosa2 != null)
                        {
                            ListPresentazioneRosa2.SelectedItem = giocatore;
                            ListPresentazioneRosa2.ScrollIntoView(giocatore);
                        }
                    });
                }

                string percorsoFotoEffettivo = OttieniPercorsoFotoPresentazione(giocatore.PercorsoFoto);
                string cognomeAtleta = giocatore.Cognome?.Trim() ?? "";
                string nomeAtleta = giocatore.Nome?.Trim() ?? "";
                string testoAtleta = cognomeAtleta.Length > 0 && nomeAtleta.Length > 0
                    ? $"{cognomeAtleta}\n{nomeAtleta}"
                    : cognomeAtleta.Length > 0 ? cognomeAtleta : nomeAtleta;

                Dispatcher.Invoke(() =>
                {
                    string testoNumero = giocatore.IsStaff
                        ? giocatore.Numero.Trim().ToUpperInvariant() switch
                        {
                            "1° AL" => "PRIMO\nALLENATORE",
                            "2° AL" => "SECONDO\nALLENATORE",
                            "DRG" => "DIRIGENTE",
                            _ => giocatore.Numero.Trim()
                        }
                        : string.IsNullOrWhiteSpace(giocatore.Numero) ? "" : $"#{giocatore.Numero.Trim()}";

                    if (èSquadra1 && TxtNumeroLogoPresSquadra1 != null)
                        TxtNumeroLogoPresSquadra1.Text = testoNumero;
                    else if (!èSquadra1 && TxtNumeroLogoPresSquadra2 != null)
                        TxtNumeroLogoPresSquadra2.Text = testoNumero;

                    double dimensioneNumero = giocatore.IsStaff ? 42 : 144;
                    if (TxtNumeroLogoPresSquadra1 != null) TxtNumeroLogoPresSquadra1.FontSize = dimensioneNumero;
                    if (TxtNumeroLogoPresSquadra2 != null) TxtNumeroLogoPresSquadra2.FontSize = dimensioneNumero;

                    if (TxtPresentazione != null)
                    {
                        TxtPresentazione.Text = testoAtleta;
                        TxtPresentazione.Foreground = _coloreGiocatori;
                    }

                    if (ImgPresentazione != null && !string.IsNullOrEmpty(percorsoFotoEffettivo) && File.Exists(percorsoFotoEffettivo))
                    {
                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(percorsoFotoEffettivo, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            ImgPresentazione.Source = bitmap;
                        }
                        catch
                        {
                            ImgPresentazione.Source = null;
                        }
                    }
                    else if (ImgPresentazione != null)
                    {
                        ImgPresentazione.Source = null;
                    }
                });

                try
                {
                    await Task.Delay(2500, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            Dispatcher.Invoke(() =>
            {
                if (GridPresentazione != null) GridPresentazione.Visibility = Visibility.Collapsed;
            });
        }

        private string OttieniPercorsoFotoPresentazione(string percorsoFotoGiocatore)
        {
            // Usa lo stesso risolutore del caricamento della configurazione:
            // supporta percorsi relativi come Squadra1/G1_1.jpg e Squadra2/G2_1.jpg.
            if (!string.IsNullOrWhiteSpace(percorsoFotoGiocatore))
            {
                string percorsoAssoluto = _configurazioneService
                    .RisolviPercorsoFileConfigurazione(percorsoFotoGiocatore);

                if (File.Exists(percorsoAssoluto))
                {
                    return Path.GetFullPath(percorsoAssoluto);
                }
            }

            // 2. Fallback: cerca un'immagine di default nella cartella DATA/DEF7fotoatleta
            string cartellaDefault = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DATA", "DEF7fotoatleta");
            if (Directory.Exists(cartellaDefault))
            {
                var fileDisponibili = Directory.GetFiles(cartellaDefault, "*.*");
                foreach (var file in fileDisponibili)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        return file; // Restituisce la prima immagine di default trovata
                    }
                }
            }

            return string.Empty;
        }

        public void InterrompiPresentazione()
        {
            _animazioniService.InterrompiPresentazione();

            GridPresentazionePartita.Visibility = Visibility.Collapsed;

            Dispatcher.Invoke(() =>
            {
                if (ListRosa1 != null) ListRosa1.SelectedItem = null;
                if (ListRosa2 != null) ListRosa2.SelectedItem = null;
            });
        }

        // Metodo pubblico per personalizzare i colori e la visibilità dei bordi dal pannello opzioni (16 parametri)
        public void ImpostaColoriPersonalizzati(
                    Color sfondoGenerale,
                    Color sfondoSquadre,
                    Color sfondoNomeSq1,
                    Color sfondoNomeSq2,
                    Color coloreCaratterePunti,
                    Color coloriCarattereSet,
                    Color coloreCarattereStorico,
                    Color coloreCarattereSquadra1,
                    Color coloreCarattereSquadra2,
                    Color coloreGiocatori,
                    Color coloreLiberi,       
                    Color coloreCapitano,
                    Color coloreStaff,
                    bool bordoPunti,
                    bool bordoSet,
                    bool bordoStorico,
                    bool bordoSq1,
                    bool bordoSq2,
                    bool bordoGiocatori)
        {
            // 1. Sfondo generale e blocco centrale
            var brushGenerale = new SolidColorBrush(sfondoGenerale);
            this.Background = brushGenerale;
            if (GridCentro != null) GridCentro.Background = brushGenerale;
            if (GridFill != null) GridFill.Background = brushGenerale;

            // 2. Sfondo riquadri giocatori
            var brushSquadre = new SolidColorBrush(sfondoSquadre);
            if (GridSquadra1 != null) GridSquadra1.Background = brushSquadre;
            if (GridSquadra2 != null) GridSquadra2.Background = brushSquadre;

            // 3. Sfondo nomi squadre
            if (GridNomeSquadra1 != null) GridNomeSquadra1.Background = new SolidColorBrush(sfondoNomeSq1);
            if (GridNomeSquadra2 != null) GridNomeSquadra2.Background = new SolidColorBrush(sfondoNomeSq2);

            // 4. Carattere Punti e Bordo
            if (TxtPunti1 != null) TxtPunti1.Foreground = new SolidColorBrush(coloreCaratterePunti);
            if (TxtPunti2 != null) TxtPunti2.Foreground = new SolidColorBrush(coloreCaratterePunti);
            if (BorderPunti != null) BorderPunti.BorderBrush = bordoPunti ? Brushes.Black : Brushes.Transparent;
            if (TxtSeparatorePunti != null) TxtSeparatorePunti.Foreground = new SolidColorBrush(coloreCaratterePunti);

            // 5. Carattere Set e Bordo
            Brush brushSet = new SolidColorBrush(coloriCarattereSet);
            if (TxtSetS1 != null) TxtSetS1.Foreground = brushSet;
            if (TxtSetS2 != null) TxtSetS2.Foreground = brushSet;
            if (TxtSetCorrente != null) TxtSetCorrente.Foreground = brushSet;
            if (TxtSeparatoreSet != null) TxtSeparatoreSet.Foreground = brushSet;
            if (BorderSet != null) BorderSet.BorderBrush = bordoSet ? Brushes.Black : Brushes.Transparent;

            // 6. Carattere Storico e Bordo
            if (TxtStoricoSet != null) TxtStoricoSet.Foreground = new SolidColorBrush(coloreCarattereStorico);
            if (BorderStorico != null) BorderStorico.BorderBrush = bordoStorico ? Brushes.Black : Brushes.Transparent;

            // 7. Carattere Nomi Squadre 1 e 2 e Bordi
            if (TxtSquadra1 != null) TxtSquadra1.Foreground = new SolidColorBrush(coloreCarattereSquadra1);
            if (BorderSq1 != null) BorderSq1.BorderBrush = bordoSq1 ? Brushes.Black : Brushes.Transparent;

            if (TxtSquadra2 != null) TxtSquadra2.Foreground = new SolidColorBrush(coloreCarattereSquadra2);
            if (BorderSq2 != null) BorderSq2.BorderBrush = bordoSq2 ? Brushes.Black : Brushes.Transparent;

            // 8. Colori Giocatori, Liberi, Capitano e Staff
            Brush brushGiocatori = new SolidColorBrush(coloreGiocatori);
            Brush brushLiberi = new SolidColorBrush(coloreLiberi);
            Brush brushCapitano = new SolidColorBrush(coloreCapitano);
            Brush brushStaff = new SolidColorBrush(coloreStaff);
            _coloreGiocatori = brushGiocatori;
            _coloreSquadra1 = new SolidColorBrush(coloreCarattereSquadra1);
            _coloreSquadra2 = new SolidColorBrush(coloreCarattereSquadra2);

            if (GridPresentazione != null) GridPresentazione.Background = brushGenerale;
            if (GridPresentazioneSinistra != null) GridPresentazioneSinistra.Background = brushSquadre;
            if (GridPresentazioneDestra != null) GridPresentazioneDestra.Background = brushSquadre;
            if (BorderFotoPresentazione != null) BorderFotoPresentazione.Background = brushSquadre;

            Action<List<GiocatoreItem>> aggiornaColoriRosa = (rosa) =>
            {
                if (rosa == null) return;
                foreach (var g in rosa)
                {
                    if (g.IsStaff)
                    {
                        g.ForegroundColor = brushStaff; 
                    }
                    else if (g.IsLibero)
                    {
                        g.ForegroundColor = brushLiberi;
                    }
                    else if (g.IsCapitano)
                    {
                        g.ForegroundColor = brushCapitano;
                    }
                    else
                    {
                        g.ForegroundColor = brushGiocatori;
                    }
                }
            };
            aggiornaColoriRosa(_rosaCompleta1);
            aggiornaColoriRosa(_rosaCompleta2);

            ImpostaRosaConStaff(_rosaCompleta1, ListRosa1, ItemsStaff1);
            ImpostaRosaConStaff(_rosaCompleta2, ListRosa2, ItemsStaff2);

            // Gestione dei bordi dei giocatori (es. con colore personalizzato o nero/bianco)
            if (BorderGiocatori1 != null) BorderGiocatori1.BorderBrush = bordoGiocatori ? brushGiocatori : Brushes.Transparent;
            if (BorderGiocatori2 != null) BorderGiocatori2.BorderBrush = bordoGiocatori ? brushGiocatori : Brushes.Transparent;
        }

        public void ImpostaDimensioniEPosizioni(
            double dimGiocatori, double dimStaff, double dimNomiSquadre,
            double dimPunti, double dimSet, double dimStorico,
            double dimSetCorrente,
            double posYPunti, double posYSet, double posYStorico, double posYSetCorrente)

        {
            if (ListRosa1 != null) ListRosa1.FontSize = dimGiocatori;
            if (ListRosa2 != null) ListRosa2.FontSize = dimGiocatori;

            if (ListPresentazioneRosa1 != null) ListPresentazioneRosa1.FontSize = dimGiocatori;
            if (ListPresentazioneRosa2 != null) ListPresentazioneRosa2.FontSize = dimGiocatori;

            if (ItemsStaff1 != null) ItemsStaff1.FontSize = dimStaff;
            if (ItemsStaff2 != null) ItemsStaff2.FontSize = dimStaff;
            if (ItemsStaffPres1 != null) ItemsStaffPres1.FontSize = dimStaff;
            if (ItemsStaffPres2 != null) ItemsStaffPres2.FontSize = dimStaff;
            if (TxtSquadra1 != null) TxtSquadra1.FontSize = dimNomiSquadre;
            if (TxtSquadra2 != null) TxtSquadra2.FontSize = dimNomiSquadre;


            // Punti e separatore punti (stessa dimensione e colore)
            if (TxtPunti1 != null) TxtPunti1.FontSize = dimPunti;
            if (TxtPunti2 != null) TxtPunti2.FontSize = dimPunti;
            if (TxtSeparatorePunti != null) TxtSeparatorePunti.FontSize = dimPunti;

            // Set e separatore set (stessa dimensione)
            if (TxtSetS1 != null) TxtSetS1.FontSize = dimSet;
            if (TxtSetS2 != null) TxtSetS2.FontSize = dimSet;
            if (TxtSeparatoreSet != null) TxtSeparatoreSet.FontSize = dimSet;

            // Set Corrente indipendente
            if (TxtSetCorrente != null) TxtSetCorrente.FontSize = dimSetCorrente;
            if (TxtStoricoSet != null) TxtStoricoSet.FontSize = dimStorico;

            // Posizioni verticali (Margini offset)
            if (BorderSet != null)
                BorderSet.Margin = new Thickness(BorderSet.Margin.Left, 10 + posYSet, BorderSet.Margin.Right, BorderSet.Margin.Bottom);

            if (BorderPunti != null)
                BorderPunti.Margin = new Thickness(BorderPunti.Margin.Left, posYPunti, BorderPunti.Margin.Right, BorderPunti.Margin.Bottom);

            if (BorderStorico != null)
                BorderStorico.Margin = new Thickness(BorderStorico.Margin.Left, posYStorico, BorderStorico.Margin.Right, BorderStorico.Margin.Bottom);

            if (TxtSetCorrente != null)
                TxtSetCorrente.Margin = new Thickness(0, posYSetCorrente, 0, 5);
        }

        private void ImpostaRosaConStaff(List<GiocatoreItem> rosaCompleta, ListBox listBoxGiocatori, ItemsControl controlStaff)
        {
            var giocatori = new List<GiocatoreItem>();
            var staff = new List<GiocatoreItem>();

            if (rosaCompleta != null)
            {
                foreach (var item in rosaCompleta)
                {
                    if (item.Visualizza)
                    {
                        if (item.IsStaff) staff.Add(item);
                        else giocatori.Add(item);
                    }
                }
            }

            listBoxGiocatori.ItemsSource = giocatori;
            controlStaff.ItemsSource = staff;
        }

        public void AggiornaNomiSquadre(string nome1, string nome2)
        {
            TxtSquadra1.Text = nome1;
            TxtSquadra2.Text = nome2;
            _consoleWindow?.AggiornaTestiPulsanti(nome1, nome2);
        }

        public void AggiornaConfigurazioneSquadre(string nome1, string nome2, List<GiocatoreItem> rosa1, List<GiocatoreItem> rosa2, string logo1, string logo2)
        {
            TxtSquadra1.Text = nome1;
            TxtSquadra2.Text = nome2;

            _rosaCompleta1 = new List<GiocatoreItem>();
            if (rosa1 != null)
            {
                foreach (var item in rosa1)
                {
                    _rosaCompleta1.Add(new GiocatoreItem
                    {
                        Numero = item.Numero ?? "",
                        Cognome = item.Cognome ?? "",
                        Nome = item.Nome ?? "",
                        TestoVisualizzato = item.NomeFormattatoTabellone,
                        Visualizza = item.Visualizza,
                        ForegroundColor = item.ForegroundColor,
                        PercorsoFoto = item.PercorsoFoto ?? "",
                        IsStaff = item.IsStaff,
                        IsLibero = item.IsLibero,
                        IsCapitano = item.IsCapitano
                    });
                }
            }

            _rosaCompleta2 = new List<GiocatoreItem>();
            if (rosa2 != null)
            {
                foreach (var item in rosa2)
                {
                    _rosaCompleta2.Add(new GiocatoreItem
                    {
                        Numero = item.Numero ?? "",
                        Cognome = item.Cognome ?? "",
                        Nome = item.Nome ?? "",
                        TestoVisualizzato = item.NomeFormattatoTabellone,
                        Visualizza = item.Visualizza,
                        ForegroundColor = item.ForegroundColor,
                        PercorsoFoto = item.PercorsoFoto ?? "",
                        IsStaff = item.IsStaff,
                        IsLibero = item.IsLibero,
                        IsCapitano = item.IsCapitano
                    });
                }
            }
            ImpostaRosaConStaff(_rosaCompleta1, ListRosa1, ItemsStaff1);
            ImpostaRosaConStaff(_rosaCompleta2, ListRosa2, ItemsStaff2);

            ImpostaLogo(
                ImgLogoSquadra1,
                _configurazioneService.RisolviPercorsoFileConfigurazione(logo1));
            ImpostaLogo(
                ImgLogoSquadra2,
                _configurazioneService.RisolviPercorsoFileConfigurazione(logo2));

            _consoleWindow?.AggiornaTestiPulsanti(nome1, nome2);
            SalvaSuFileEsterno(nome1, nome2, _rosaCompleta1, _rosaCompleta2, logo1, logo2);

            CaricaColoriDaFile();
        }
        private void SalvaSuFileEsterno(string nome1, string nome2, List<GiocatoreItem> r1, List<GiocatoreItem> r2, string logo1, string logo2)
        {
            try
            {
                string cartellaDestinazione = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastconfig");
                if (!Directory.Exists(cartellaDestinazione))
                {
                    Directory.CreateDirectory(cartellaDestinazione);
                }

                string logo1Risolto = _configurazioneService
                    .RisolviPercorsoFileConfigurazione(logo1);
                string logo2Risolto = _configurazioneService
                    .RisolviPercorsoFileConfigurazione(logo2);

                // I loghi sono già salvati da GestioneSquadreWindow in
                // lastconfig\InfoPartita. Manteniamo il percorso relativo
                // invece di creare copie nella radice di lastconfig.
                string percorsoLogo1Salvato =
                    File.Exists(logo1Risolto) ? logo1 : "";
                string percorsoLogo2Salvato =
                    File.Exists(logo2Risolto) ? logo2 : "";

                var model = new ConfigClassXML
                {
                    NomeSquadra1 = nome1,
                    LogoSquadra1 = percorsoLogo1Salvato,
                    NomeSquadra2 = nome2,
                    LogoSquadra2 = percorsoLogo2Salvato
                };

                ProcessaRosa(r1, model.Rosa1, "s1");
                ProcessaRosa(r2, model.Rosa2, "s2");

                _configurazioneService.Salva(
                    _configurazioneService.PercorsoConfigurazioneSquadre,
                    model);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore nel salvataggio dei file o dell'XML: " + ex.Message);
            }
        }

        private void ProcessaRosa(List<GiocatoreItem> rosaSorgente, List<ItemXml> rosaDestinazione, string prefissoFoto)
        {
            foreach (var g in rosaSorgente)
            {
                bool eStaff = g.IsStaff || (g.Numero != null && (g.Numero.StartsWith("1°") || g.Numero.StartsWith("2°") || g.Numero.StartsWith("DRG")));

                string nomeFileFoto = "";

                if (!string.IsNullOrEmpty(g.PercorsoFoto))
                {
                    string percorsoFotoRisolto = _configurazioneService
                        .RisolviPercorsoFileConfigurazione(g.PercorsoFoto);

                    if (File.Exists(percorsoFotoRisolto))
                    {
                        string percorsoBase = AppDomain.CurrentDomain.BaseDirectory;
                        nomeFileFoto = Path.GetRelativePath(percorsoBase, percorsoFotoRisolto)
                            .Replace(Path.DirectorySeparatorChar, '/');
                    }
                }

                rosaDestinazione.Add(new ItemXml
                {
                    Numero = g.Numero,
                    Cognome = g.Cognome, // Salviamo anche il cognome separato
                    Nome = g.Nome,
                    Visualizza = g.Visualizza,
                    IsStaff = eStaff,
                    PercorsoFoto = nomeFileFoto,
                    IsLibero = g.IsLibero,
                    IsCapitano = g.IsCapitano
                });
            }
        }

        private void CaricaUltimaConfigurazioneDaFile()
        {
            string percorso = _configurazioneService.PercorsoConfigurazioneSquadre;

            if (File.Exists(percorso))
            {
                try
                {
                    ConfigClassXML model = _configurazioneService.Carica<ConfigClassXML>(percorso);
                    if (model == null) return;

                    TxtSquadra1.Text = model.NomeSquadra1 ?? "";
                    TxtSquadra2.Text = model.NomeSquadra2 ?? "";

                    _rosaCompleta1 = new List<GiocatoreItem>();
                    if (model.Rosa1 != null)
                    {
                        foreach (var item in model.Rosa1)
                        {
                            string percorsoFotoEffettivo = "";
                            if (!string.IsNullOrEmpty(item.PercorsoFoto))
                            {
                                percorsoFotoEffettivo = _configurazioneService.RisolviPercorsoFileConfigurazione(item.PercorsoFoto);
                            }

                            _rosaCompleta1.Add(new GiocatoreItem
                            {
                                Numero = item.Numero ?? "",
                                Cognome = item.Cognome ?? "",
                                Nome = item.Nome ?? "",
                                Visualizza = item.Visualizza,
                                ForegroundColor = Brushes.White,
                                PercorsoFoto = percorsoFotoEffettivo,
                                IsStaff = item.IsStaff,
                                IsLibero = item.IsLibero,
                                IsCapitano = item.IsCapitano,
                            });
                        }
                    }

                    _rosaCompleta2 = new List<GiocatoreItem>();
                    if (model.Rosa2 != null)
                    {
                        foreach (var item in model.Rosa2)
                        {
                            string percorsoFotoEffettivo = "";
                            if (!string.IsNullOrEmpty(item.PercorsoFoto))
                            {
                                percorsoFotoEffettivo = _configurazioneService.RisolviPercorsoFileConfigurazione(item.PercorsoFoto);
                            }
                            _rosaCompleta2.Add(new GiocatoreItem
                            {
                                Numero = item.Numero ?? "",
                                Cognome = item.Cognome ?? "",
                                Nome = item.Nome ?? "",
                                Visualizza = item.Visualizza,
                                ForegroundColor = Brushes.White,
                                PercorsoFoto = percorsoFotoEffettivo,
                                IsStaff = item.IsStaff,
                                IsLibero = item.IsLibero,
                                IsCapitano = item.IsCapitano,
                            });
                        }
                    }

                    ImpostaRosaConStaff(_rosaCompleta1, ListRosa1, ItemsStaff1);
                    ImpostaRosaConStaff(_rosaCompleta2, ListRosa2, ItemsStaff2);
                    AggiornaLoghiTabellone(model);
                    CaricaColoriDaFile();
                    return;
                }
                catch
                {
                    // Fallback
                }
            }

            TxtSquadra1.Text = "SQUADRA 1";
            TxtSquadra2.Text = "SQUADRA 2";
            _rosaCompleta1.Clear();
            _rosaCompleta2.Clear();
            ImpostaRosaConStaff(_rosaCompleta1, ListRosa1, ItemsStaff1);
            ImpostaRosaConStaff(_rosaCompleta2, ListRosa2, ItemsStaff2);
            ImpostaLogo(ImgLogoSquadra1, string.Empty);
            ImpostaLogo(ImgLogoSquadra2, string.Empty);
        }

        private void CaricaColoriDaFile()
        {
            string percorso = _configurazioneService.PercorsoConfigurazioneColori;

            if (File.Exists(percorso))
            {
                try
                {
                    ConfigurazioneColoriModel model = _configurazioneService.Carica<ConfigurazioneColoriModel>(percorso);
                    if (model == null) return;

                    ImpostaColoriPersonalizzati(
                        (Color)ColorConverter.ConvertFromString(model.SfondoGenerale),
                        (Color)ColorConverter.ConvertFromString(model.SfondoSquadre),
                        (Color)ColorConverter.ConvertFromString(model.SfondoNomeSq1),
                        (Color)ColorConverter.ConvertFromString(model.SfondoNomeSq2),
                        (Color)ColorConverter.ConvertFromString(model.ColoreCaratterePunti),
                        (Color)ColorConverter.ConvertFromString(model.ColoriCarattereSet),
                        (Color)ColorConverter.ConvertFromString(model.ColoreCarattereStorico),
                        (Color)ColorConverter.ConvertFromString(model.ColoreCarattereSquadra1),
                        (Color)ColorConverter.ConvertFromString(model.ColoreCarattereSquadra2),
                        (Color)ColorConverter.ConvertFromString(model.ColoreGiocatori),
                        (Color)ColorConverter.ConvertFromString(string.IsNullOrEmpty(model.ColoreLiberi) ? (model.ColoreGiocatori ?? "#FFFFFF") : model.ColoreLiberi),
                        (Color)ColorConverter.ConvertFromString(string.IsNullOrEmpty(model.ColoreCapitano) ? (model.ColoreGiocatori ?? "#FFFFFF") : model.ColoreCapitano),
                        (Color)ColorConverter.ConvertFromString(string.IsNullOrEmpty(model.ColoreStaff) ? (model.ColoreGiocatori ?? "#FFFFFF") : model.ColoreStaff),
                        model.BordoPunti,
                        model.BordoSet,
                        model.BordoStorico,
                        model.BordoSq1,
                        model.BordoSq2,
                        model.BordoGiocatori
                    );
                    return;
                }
                catch
                {
                    // Fallback
                }
            }

            ImpostaColoriPersonalizzati(
                (Color)ColorConverter.ConvertFromString("#120A1A"),
                (Color)ColorConverter.ConvertFromString("#1D1226"),
                (Color)ColorConverter.ConvertFromString("#1D1226"),
                (Color)ColorConverter.ConvertFromString("#1D1226"),
                Colors.White,
                Colors.Orange,
                Colors.White,
                Colors.White,
                Colors.White,
                Colors.White,
                Colors.White,
                Colors.White,
                Colors.White,
                false, false, false, false, false, false
            );
        }

        private void CaricaDimensioniDaFile()
        {
            string percorso = _configurazioneService.PercorsoConfigurazioneColori;

            if (File.Exists(percorso))
            {
                try
                {
                    ConfigurazioneColoriModel model = _configurazioneService.Carica<ConfigurazioneColoriModel>(percorso);
                    if (model == null) return;

                    ImpostaDimensioniEPosizioni(
                        model.DimensioneGiocatori,
                        model.DimensioneStaff,
                        model.DimensioneNomiSquadre,
                        model.DimensionePunti,
                        model.DimensioneSet,
                        model.DimensioneStorico,
                        model.DimensioneSetCorrente,
                        model.PosizioneVerticalePunti,
                        model.PosizioneVerticaleSet,
                        model.PosizioneVerticaleStorico,
                        model.PosizioneVerticaleSetCorrente
                    );
                }
                catch
                {
                    // Fallback
                }
            }
        }

        public string OttieniPercorsoLogoEffettivo(string percorsoSalvatoInConfig)
        {
            if (!string.IsNullOrWhiteSpace(percorsoSalvatoInConfig))
            {
                string percorsoRisolto = _configurazioneService
                    .RisolviPercorsoFileConfigurazione(percorsoSalvatoInConfig);

                if (File.Exists(percorsoRisolto))
                    return percorsoRisolto;

                if (File.Exists(percorsoSalvatoInConfig))
                    return percorsoSalvatoInConfig;
            }

            if (!string.IsNullOrEmpty(FinestraConfig.PercorsoLogoDefault) && File.Exists(FinestraConfig.PercorsoLogoDefault))
            {
                return FinestraConfig.PercorsoLogoDefault;
            }

            string cartellaDef = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "DEF");
            if (Directory.Exists(cartellaDef))
            {
                foreach (string file in Directory.GetFiles(cartellaDef))
                {
                    if (Path.GetFileName(file).ToLower().StartsWith("logodefault"))
                    {
                        return file;
                    }
                }
            }

            return string.Empty;
        }

        private void ImpostaLogo(Image controlloImmagine, string percorsoSalvato)
        {
            string percorsoEffettivo = OttieniPercorsoLogoEffettivo(percorsoSalvato);

            if (!string.IsNullOrEmpty(percorsoEffettivo))
            {
                try
                {
                    string percorsoAssoluto = Path.GetFullPath(percorsoEffettivo);

                    if (File.Exists(percorsoAssoluto))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(percorsoAssoluto, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        controlloImmagine.Source = bitmap;
                    }
                    else
                    {
                        controlloImmagine.Source = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nel caricamento del logo: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                controlloImmagine.Source = null;
            }
        }

        public void AggiornaLoghiTabellone(ConfigClassXML modello)
        {
            string percorsoLogo1 = OttieniPercorsoLogoEffettivo(modello.LogoSquadra1);
            string percorsoLogo2 = OttieniPercorsoLogoEffettivo(modello.LogoSquadra2);

            ImpostaLogo(ImgLogoSquadra1, percorsoLogo1);
            ImpostaLogo(ImgLogoSquadra2, percorsoLogo2);
        }

        private void AggiornaInterfaccia()
        {
            TxtPunti1.Text = _punteggioService.PuntiSquadra1.ToString();
            TxtPunti2.Text = _punteggioService.PuntiSquadra2.ToString();
            TxtSetS1.Text = _punteggioService.SetSquadra1.ToString();
            TxtSetS2.Text = _punteggioService.SetSquadra2.ToString();
            TxtSetCorrente.Text = $"SET {_punteggioService.SetCorrente}";
            TxtStoricoSet.Text = _punteggioService.StoricoFormattato;
        }

        public void EseguiPunto1() { _punteggioService.PuntoSquadra1(); AggiornaInterfaccia(); }
        public void EseguiPunto2() { _punteggioService.PuntoSquadra2(); AggiornaInterfaccia(); }
        public void EseguiAnnulla1() { _punteggioService.AnnullaPuntoSquadra1(); AggiornaInterfaccia(); }
        public void EseguiAnnulla2() { _punteggioService.AnnullaPuntoSquadra2(); AggiornaInterfaccia(); }
        public void EseguiResetPunti() { _punteggioService.ResetPunti(); AggiornaInterfaccia(); }

        public void EseguiResetSet()
        {
            _punteggioService.ResetSet();
            AggiornaInterfaccia();
        }

        public void EseguiAnnullaSet()
        {
            _punteggioService.AnnullaSet();
            AggiornaInterfaccia();
        }

        public async Task AvviaAnimazioneSponsor(List<string> listaPercorsiSponsor)
        {
            if (listaPercorsiSponsor == null || listaPercorsiSponsor.Count == 0) return;

            GridSponsor.Visibility = Visibility.Visible;

            await _animazioniService.EseguiSponsorAsync(async token =>
            {
                foreach (string percorso in listaPercorsiSponsor)
                {
                    if (token.IsCancellationRequested) break;

                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(percorso, UriKind.RelativeOrAbsolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ImgSponsor.Source = bitmap;

                        await AnimaOpacitaAsync(ImgSponsor, 0, 1, TimeSpan.FromMilliseconds(500));
                        try { await Task.Delay(3000, token); } catch (TaskCanceledException) { break; }

                        if (token.IsCancellationRequested) break;
                        await AnimaOpacitaAsync(ImgSponsor, 1, 0, TimeSpan.FromMilliseconds(500));
                    }
                    catch
                    {
                        // Ignora errori di caricamento immagine sponsor
                    }
                }
            });

            GridSponsor.Visibility = Visibility.Collapsed;
        }

        public void InterrompiAnimazioneSponsor()
        {
            _animazioniService.InterrompiSponsor();
            GridSponsor.Visibility = Visibility.Collapsed;
        }

        public void FermaAnimazioneSponsor() => InterrompiAnimazioneSponsor();

        private Task AnimaOpacitaAsync(UIElement elemento, double da, double a, TimeSpan durata)
        {
            var tcs = new TaskCompletionSource<bool>();
            var animazione = new DoubleAnimation(da, a, durata);

            animazione.Completed += (s, e) => tcs.SetResult(true);
            elemento.BeginAnimation(UIElement.OpacityProperty, animazione);

            return tcs.Task;
        }



    }
}