using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TabelloneWPF
{
    public partial class ConsoleControllo : Window
    {
        private MainWindow _tabellonePrincipale;
        public bool PulsantiAnimazioniVisibili { get; private set; } = false;

        public ConsoleControllo(MainWindow mainWindow)
        {
            InitializeComponent();
            _tabellonePrincipale = mainWindow;
        }
       
        // Metodo richiamato dalla finestra Opzioni per attivare/disattivare i pulsanti
        public void ImpostaPulsantiAnimazioni(bool visibili)
        {
            PulsantiAnimazioniVisibili = visibili;
            PanelAnimazioni.Visibility = visibili ? Visibility.Visible : Visibility.Collapsed;

            // Aggiorna l'altezza della finestra in tempo reale
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.SizeToContent = SizeToContent.Height; // Riporta a solo altezza dinamica
        }

        // ... (altri metodi esistenti)

    public void AggiornaTestiPulsanti(string nome1, string nome2)
        {          
           if (TxtNomeBtn1 != null)
               TxtNomeBtn1.Text = nome1;

           if (TxtNomeBtn2 != null)
               TxtNomeBtn2.Text = nome2;         
        }

        private void BtnPunto1_Click(object sender, RoutedEventArgs e) => _tabellonePrincipale?.EseguiPunto1();
        private void BtnPunto2_Click(object sender, RoutedEventArgs e) => _tabellonePrincipale?.EseguiPunto2();
        private void BtnAnnulla1_Click(object sender, RoutedEventArgs e) => _tabellonePrincipale?.EseguiAnnulla1();
        private void BtnAnnulla2_Click(object sender, RoutedEventArgs e) => _tabellonePrincipale?.EseguiAnnulla2();

        private void BtnAnnullaSet_Click(object sender, RoutedEventArgs e) => _tabellonePrincipale?.EseguiAnnullaSet();
        private void BtnResetPunti_Click(object sender, RoutedEventArgs e) => _tabellonePrincipale?.EseguiResetPunti();
        private void BtnResetSet_Click(object sender, RoutedEventArgs e) => _tabellonePrincipale?.EseguiResetSet();

        private void BtnGestioneSquadre_Click(object sender, RoutedEventArgs e)
        {
            var rosa1 = _tabellonePrincipale.GetRosa1();
            var rosa2 = _tabellonePrincipale.GetRosa2();
            string nome1 = _tabellonePrincipale.GetNomeSquadra1();
            string nome2 = _tabellonePrincipale.GetNomeSquadra2();

            GestioneSquadreWindow finestraGestione = new GestioneSquadreWindow(_tabellonePrincipale, nome1, nome2, rosa1, rosa2);
            finestraGestione.ShowDialog();
        }

        private void ExpanderAvanzate_Expanded(object sender, RoutedEventArgs e)
        {
            // Quando apri le funzioni avanzate, adatta l'altezza della finestra per includerle
            this.SizeToContent = SizeToContent.Height;
        }

        private void ExpanderAvanzate_Collapsed(object sender, RoutedEventArgs e)
        {
            // Quando chiudi le funzioni avanzate, riduci l'altezza della finestra
            this.SizeToContent = SizeToContent.Height;
        }

        private void BtnOpzioni_Click(object sender, RoutedEventArgs e)
        {
            FinestraOpzioni finestraOpzioni = new FinestraOpzioni();
            finestraOpzioni.Owner = this; // Imposta la console come finestra padre
            finestraOpzioni.ShowDialog();
        }

        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            FinestraConfig finestraConfig = new FinestraConfig();
            finestraConfig.Owner = this; // Imposta la console come finestra padre
            finestraConfig.ShowDialog();
        }

        private void BtnInfoPartita_Click(object sender, RoutedEventArgs e)
        {
            var finestraInfo = new InfoPartitaWindow(_tabellonePrincipale)
            {
                Owner = this
            };
            finestraInfo.ShowDialog();
        }

        private async void BtnSponsor_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow tabellone)
            {
                // Se l'animazione sta già girando, la interrompiamo subito
                if (tabellone.IsSponsorRunning())
                {
                    tabellone.InterrompiAnimazioneSponsor();
                    BtnSponsor.Background = (Brush)new BrushConverter().ConvertFrom("#332244"); // Colore originale
                    return;
                }

                // Prende la lista degli sponsor
                List<string> elencoSponsor = FinestraConfig.PercorsiSponsor;

                if (elencoSponsor == null || elencoSponsor.Count == 0)
                {
                    return;
                }

                // Cambia colore al pulsante per indicare che è attivo (es. Viola acceso)
                BtnSponsor.Background = (Brush)new BrushConverter().ConvertFrom("#A855F7");

                // Avvia l'animazione sul thread UI in modo asincrono
                await tabellone.AvviaAnimazioneSponsor(elencoSponsor);

                // Quando l'animazione termina (per fine lista o perché interrotta), riporta il pulsante al colore normale
                BtnSponsor.Background = (Brush)new BrushConverter().ConvertFrom("#332244");
            }
        }

        private async void BtnPresentazionePartita_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is not MainWindow tabellone)
                return;

            if (tabellone.IsPresentazioneRunning())
            {
                tabellone.InterrompiPresentazione();
                BtnPresentazionePartita.Background = (Brush)new BrushConverter().ConvertFrom("#332244");
                return;
            }

            BtnPresentazionePartita.Background = (Brush)new BrushConverter().ConvertFrom("#A855F7");
            await tabellone.AvviaPresentazionePartitaAsync();
            BtnPresentazionePartita.Background = (Brush)new BrushConverter().ConvertFrom("#332244");
        }

        private async void BtnRose_Click(object sender, RoutedEventArgs e)
{
    if (_tabellonePrincipale != null)
    {
        if (_tabellonePrincipale.IsPresentazioneRunning())
        {
            _tabellonePrincipale.InterrompiPresentazione();
            BtnRose.Background = (Brush)new BrushConverter().ConvertFrom("#332244"); // Colore originale
        }
        else
        {
            // Cambia colore al pulsante per indicare che la presentazione è attiva
            BtnRose.Background = (Brush)new BrushConverter().ConvertFrom("#A855F7");

            await _tabellonePrincipale.AvviaPresentazioneRoseAsync();

            // Riporta il pulsante al colore normale a fine presentazione
            BtnRose.Background = (Brush)new BrushConverter().ConvertFrom("#332244");
        }
    }
}
    }
}