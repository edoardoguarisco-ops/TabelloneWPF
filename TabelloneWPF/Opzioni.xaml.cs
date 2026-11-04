using System.IO;
using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TabelloneWPF
{
    public partial class FinestraOpzioni : Window
    {
        private MainWindow _mainWindow;
        private readonly ConfigurazioneService _configurazioneService = new();

        private Color _correnteSfondoGen = (Color)ColorConverter.ConvertFromString("#120A1A");
        private Color _correnteSfondoSq = (Color)ColorConverter.ConvertFromString("#1D1226");
        private Color _correnteSfondoNomeSq1 = (Color)ColorConverter.ConvertFromString("#1D1226");
        private Color _correnteSfondoNomeSq2 = (Color)ColorConverter.ConvertFromString("#1D1226");
        private Color _correnteCaratterePunti = Colors.White;
        private Color _correnteCarattereSet = Colors.Orange;
        private Color _correnteCarattereStorico = Colors.White;
        private Color _correnteCarattereSq1 = Colors.White;
        private Color _correnteCarattereSq2 = Colors.White;
        private Color _correnteGiocatori = Colors.White;
        private Color _correnteLiberi = Colors.White;
        private Color _correnteCapitano = Colors.White;
        private Color _correnteStaff = Colors.White;

        public FinestraOpzioni()
        {
            InitializeComponent();

            if (TxtPercorsoRisorse != null)
            {
                TxtPercorsoRisorse.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lastconfig");
            }

            if (TxtPercorsoDef != null)
            {
                TxtPercorsoDef.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "DEF");
            }

            if (TxtPercorsoSponsor != null)
            {
                TxtPercorsoSponsor.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "sponsor");
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow main)
                {
                    _mainWindow = main;
                    break;
                }
            }

            if (_mainWindow == null)
            {
                MessageBox.Show("Impossibile agganciare la finestra principale del tabellone.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            if (this.Owner is ConsoleControllo console)
            {
                ChkAnimazioni.IsChecked = console.PulsantiAnimazioniVisibili;
            }

            CaricaColoriCorrentiNelleOpzioni();
            AggiornaMiniatureGrafiche();
        }

        private void CaricaColoriCorrentiNelleOpzioni()
        {
            string percorso = _configurazioneService.PercorsoConfigurazioneColori;

            if (File.Exists(percorso))
            {
                try
                {
                    ConfigurazioneColoriModel model = _configurazioneService.Carica<ConfigurazioneColoriModel>(percorso);
                    if (model == null) return;

                    if (!string.IsNullOrEmpty(model.SfondoGenerale)) _correnteSfondoGen = (Color)ColorConverter.ConvertFromString(model.SfondoGenerale);
                    if (!string.IsNullOrEmpty(model.SfondoSquadre)) _correnteSfondoSq = (Color)ColorConverter.ConvertFromString(model.SfondoSquadre);
                    if (!string.IsNullOrEmpty(model.SfondoNomeSq1)) _correnteSfondoNomeSq1 = (Color)ColorConverter.ConvertFromString(model.SfondoNomeSq1);
                    if (!string.IsNullOrEmpty(model.SfondoNomeSq2)) _correnteSfondoNomeSq2 = (Color)ColorConverter.ConvertFromString(model.SfondoNomeSq2);
                    if (!string.IsNullOrEmpty(model.ColoreCaratterePunti)) _correnteCaratterePunti = (Color)ColorConverter.ConvertFromString(model.ColoreCaratterePunti);
                    if (!string.IsNullOrEmpty(model.ColoriCarattereSet)) _correnteCarattereSet = (Color)ColorConverter.ConvertFromString(model.ColoriCarattereSet);
                    if (!string.IsNullOrEmpty(model.ColoreCarattereStorico)) _correnteCarattereStorico = (Color)ColorConverter.ConvertFromString(model.ColoreCarattereStorico);
                    if (!string.IsNullOrEmpty(model.ColoreCarattereSquadra1)) _correnteCarattereSq1 = (Color)ColorConverter.ConvertFromString(model.ColoreCarattereSquadra1);
                    if (!string.IsNullOrEmpty(model.ColoreCarattereSquadra2)) _correnteCarattereSq2 = (Color)ColorConverter.ConvertFromString(model.ColoreCarattereSquadra2);
                    if (!string.IsNullOrEmpty(model.ColoreGiocatori)) _correnteGiocatori = (Color)ColorConverter.ConvertFromString(model.ColoreGiocatori);
                    if (!string.IsNullOrEmpty(model.ColoreLiberi)) _correnteLiberi = (Color)ColorConverter.ConvertFromString(model.ColoreLiberi);
                    if (!string.IsNullOrEmpty(model.ColoreCapitano)) _correnteCapitano = (Color)ColorConverter.ConvertFromString(model.ColoreCapitano);
                    if (!string.IsNullOrEmpty(model.ColoreStaff)) _correnteStaff = (Color)ColorConverter.ConvertFromString(model.ColoreStaff);


                    if (ChkBordoPunti != null) ChkBordoPunti.IsChecked = model.BordoPunti;
                    if (ChkBordoSet != null) ChkBordoSet.IsChecked = model.BordoSet;
                    if (ChkBordoStorico != null) ChkBordoStorico.IsChecked = model.BordoStorico;
                    if (ChkBordoSq1 != null) ChkBordoSq1.IsChecked = model.BordoSq1;
                    if (ChkBordoSq2 != null) ChkBordoSq2.IsChecked = model.BordoSq2;
                    if (ChkBordoGiocatori != null) ChkBordoGiocatori.IsChecked = model.BordoGiocatori;

                    // Caricamento Dimensioni e Posizioni nelle TextBox
                    if (TxtDimGiocatori != null) TxtDimGiocatori.Text = model.DimensioneGiocatori.ToString();
                    if (TxtDimStaff != null) TxtDimStaff.Text = model.DimensioneStaff.ToString();
                    if (TxtDimNomiSquadre != null) TxtDimNomiSquadre.Text = model.DimensioneNomiSquadre.ToString();
                    if (TxtDimPunti != null) TxtDimPunti.Text = model.DimensionePunti.ToString();
                    if (TxtDimSet != null) TxtDimSet.Text = model.DimensioneSet.ToString();
                    if (TxtDimStorico != null) TxtDimStorico.Text = model.DimensioneStorico.ToString();
                    if (TxtDimSetCorrente != null) TxtDimSetCorrente.Text = model.DimensioneSetCorrente.ToString();
                    if (TxtPosYPunti != null) TxtPosYPunti.Text = model.PosizioneVerticalePunti.ToString();
                    if (TxtPosYSet != null) TxtPosYSet.Text = model.PosizioneVerticaleSet.ToString();
                    if (TxtPosYSetCorrente != null) TxtPosYSetCorrente.Text = model.PosizioneVerticaleSetCorrente.ToString();
                    if (TxtPosYStorico != null) TxtPosYStorico.Text = model.PosizioneVerticaleStorico.ToString();
                }
                catch
                {
                    // Fallback silenzioso in caso di errore di lettura
                }
            }
        }
        private void BtnCambiaColore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string tipoColore)
            {
                using (var colorDialog = new System.Windows.Forms.ColorDialog())
                {
                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var sysColor = colorDialog.Color;
                        Color wpfColor = Color.FromArgb(sysColor.A, sysColor.R, sysColor.G, sysColor.B);

                        switch (tipoColore)
                        {
                            case "SfondoGenerale": _correnteSfondoGen = wpfColor; break;
                            case "SfondoSquadre": _correnteSfondoSq = wpfColor; break;
                            case "SfondoNomeSquadra1": _correnteSfondoNomeSq1 = wpfColor; break;
                            case "SfondoNomeSquadra2": _correnteSfondoNomeSq2 = wpfColor; break;
                            case "ColoreCaratterePunti": _correnteCaratterePunti = wpfColor; break;
                            case "ColoriCarattereSet": _correnteCarattereSet = wpfColor; break;
                            case "ColoreCarattereStorico": _correnteCarattereStorico = wpfColor; break;
                            case "ColoreCarattereSquadra1": _correnteCarattereSq1 = wpfColor; break;
                            case "ColoreCarattereSquadra2": _correnteCarattereSq2 = wpfColor; break;
                            case "ColoreGiocatori": _correnteGiocatori = wpfColor; break;
                            case "ColoreLiberi": _correnteLiberi = wpfColor; break;
                            case "ColoreCapitano": _correnteCapitano = wpfColor; break;
                            case "ColoreStaff": _correnteStaff = wpfColor; break;
                        }

                        ApplicaColoriCorrenti();
                    }
                }
            }
        }

        private void BtnCaricaDefault_Click(object sender, RoutedEventArgs e)
        {
            _correnteSfondoGen = (Color)ColorConverter.ConvertFromString("#120A1A");
            _correnteSfondoSq = (Color)ColorConverter.ConvertFromString("#1D1226");
            _correnteSfondoNomeSq1 = (Color)ColorConverter.ConvertFromString("#1D1226");
            _correnteSfondoNomeSq2 = (Color)ColorConverter.ConvertFromString("#1D1226");
            _correnteCaratterePunti = Colors.White;
            _correnteCarattereSet = Colors.Orange;
            _correnteCarattereStorico = Colors.White;
            _correnteCarattereSq1 = Colors.White;
            _correnteCarattereSq2 = Colors.White;
            _correnteGiocatori = Colors.White;
            _correnteStaff = Colors.White;

            if (ChkBordoPunti != null) ChkBordoPunti.IsChecked = false;
            if (ChkBordoSet != null) ChkBordoSet.IsChecked = false;
            if (ChkBordoStorico != null) ChkBordoStorico.IsChecked = false;
            if (ChkBordoSq1 != null) ChkBordoSq1.IsChecked = false;
            if (ChkBordoSq2 != null) ChkBordoSq2.IsChecked = false;
            if (ChkBordoGiocatori != null) ChkBordoGiocatori.IsChecked = false;

            if (TxtDimGiocatori != null) TxtDimGiocatori.Text = "16";
            if (TxtDimStaff != null) TxtDimStaff.Text = "15";
            if (TxtDimNomiSquadre != null) TxtDimNomiSquadre.Text = "22";
            if (TxtDimPunti != null) TxtDimPunti.Text = "80";
            if (TxtDimSet != null) TxtDimSet.Text = "28";
            if (TxtDimStorico != null) TxtDimStorico.Text = "13";
            if (TxtDimSetCorrente != null) TxtDimSetCorrente.Text = "18";
            if (TxtPosYPunti != null) TxtPosYPunti.Text = "0";
            if (TxtPosYSet != null) TxtPosYSet.Text = "0";
            if (TxtPosYSetCorrente != null) TxtPosYSetCorrente.Text = "0";
            if (TxtPosYStorico != null) TxtPosYStorico.Text = "0";

            ApplicaColoriCorrenti();
            ApplicaDimensioniCorrenti();
        }

        private void BtnCaricaDefaultDimensioni_Click(object sender, RoutedEventArgs e)
        {
            if (TxtDimGiocatori != null) TxtDimGiocatori.Text = "16";
            if (TxtDimStaff != null) TxtDimStaff.Text = "15";
            if (TxtDimNomiSquadre != null) TxtDimNomiSquadre.Text = "22";
            if (TxtDimPunti != null) TxtDimPunti.Text = "80";
            if (TxtDimSet != null) TxtDimSet.Text = "28";
            if (TxtDimStorico != null) TxtDimStorico.Text = "13";
            if (TxtDimSetCorrente != null) TxtDimSetCorrente.Text = "18";
            if (TxtPosYPunti != null) TxtPosYPunti.Text = "0";
            if (TxtPosYSet != null) TxtPosYSet.Text = "0";
            if (TxtPosYSetCorrente != null) TxtPosYSetCorrente.Text = "0";
            if (TxtPosYStorico != null) TxtPosYStorico.Text = "0";

            ApplicaDimensioniCorrenti();
        }

        private void ChkBordo_Changed(object sender, RoutedEventArgs e)
        {
            ApplicaColoriCorrenti();
        }

        private void Dimensione_Changed(object sender, RoutedEventArgs e)
        {
            ApplicaDimensioniCorrenti();
        }

        private void ApplicaColoriCorrenti()
        {
            AggiornaMiniatureGrafiche();

            bool bordoPunti = ChkBordoPunti?.IsChecked ?? false;
            bool bordoSet = ChkBordoSet?.IsChecked ?? false;
            bool bordoStorico = ChkBordoStorico?.IsChecked ?? false;
            bool bordoSq1 = ChkBordoSq1?.IsChecked ?? false;
            bool bordoSq2 = ChkBordoSq2?.IsChecked ?? false;
            bool bordoGiocatori = ChkBordoGiocatori?.IsChecked ?? false;

            _mainWindow?.ImpostaColoriPersonalizzati(
                _correnteSfondoGen,
                _correnteSfondoSq,
                _correnteSfondoNomeSq1,
                _correnteSfondoNomeSq2,
                _correnteCaratterePunti,
                _correnteCarattereSet,
                _correnteCarattereStorico,
                _correnteCarattereSq1,
                _correnteCarattereSq2,
                _correnteGiocatori,
                _correnteLiberi,
                _correnteCapitano,
                _correnteStaff,
                bordoPunti,
                bordoSet,
                bordoStorico,
                bordoSq1,
                bordoSq2,
                bordoGiocatori
            );
        }

        private void ApplicaDimensioniCorrenti()
        {
            if (_mainWindow == null) return;

            double.TryParse(TxtDimGiocatori?.Text, out double dimG);
            double.TryParse(TxtDimStaff?.Text, out double dimSt);
            double.TryParse(TxtDimNomiSquadre?.Text, out double dimNSq);
            double.TryParse(TxtDimPunti?.Text, out double dimP);
            double.TryParse(TxtDimSet?.Text, out double dimS);
            double.TryParse(TxtDimStorico?.Text, out double dimStor);
            double.TryParse(TxtDimSetCorrente?.Text, out double dimSetCorr);
            double.TryParse(TxtPosYPunti?.Text, out double posYP);
            double.TryParse(TxtPosYSet?.Text, out double posYS);
            double.TryParse(TxtPosYStorico?.Text, out double posYStor);
            double.TryParse(TxtPosYSetCorrente?.Text, out double posYSetCorr);

            _mainWindow.ImpostaDimensioniEPosizioni(
                dimG == 0 ? 16 : dimG,
                dimSt == 0 ? 15 : dimSt,
                dimNSq == 0 ? 22 : dimNSq,
                dimP == 0 ? 80 : dimP,
                dimS == 0 ? 28 : dimS,
                dimStor == 0 ? 13 : dimStor,
                dimSetCorr == 0 ? 18 : dimSetCorr,
                posYP,
                posYS,
                posYStor,
                posYSetCorr
            );
        }

        private void AggiornaMiniatureGrafiche()
        {
            if (RectSfondoGenerale != null) RectSfondoGenerale.Fill = new SolidColorBrush(_correnteSfondoGen);
            if (RectSfondoSquadre != null) RectSfondoSquadre.Fill = new SolidColorBrush(_correnteSfondoSq);
            if (RectSfondoNomeSquadra1 != null) RectSfondoNomeSquadra1.Fill = new SolidColorBrush(_correnteSfondoNomeSq1);
            if (RectSfondoNomeSquadra2 != null) RectSfondoNomeSquadra2.Fill = new SolidColorBrush(_correnteSfondoNomeSq2);
            if (RectColoreCaratterePunti != null) RectColoreCaratterePunti.Fill = new SolidColorBrush(_correnteCaratterePunti);
            if (RectColoriCarattereSet != null) RectColoriCarattereSet.Fill = new SolidColorBrush(_correnteCarattereSet);
            if (RectColoreCarattereStorico != null) RectColoreCarattereStorico.Fill = new SolidColorBrush(_correnteCarattereStorico);
            if (RectColoreCarattereSquadra1 != null) RectColoreCarattereSquadra1.Fill = new SolidColorBrush(_correnteCarattereSq1);
            if (RectColoreCarattereSquadra2 != null) RectColoreCarattereSquadra2.Fill = new SolidColorBrush(_correnteCarattereSq2);
            if (RectColoreGiocatori != null) RectColoreGiocatori.Fill = new SolidColorBrush(_correnteGiocatori);
            if (RectColoreLiberi != null) RectColoreLiberi.Fill = new SolidColorBrush(_correnteLiberi);
            if (RectColoreCapitano != null) RectColoreCapitano.Fill = new SolidColorBrush(_correnteCapitano);
            if (RectColoreStaff != null) RectColoreStaff.Fill = new SolidColorBrush(_correnteStaff);
        }

        private void BtnSalvaColori_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parsing sicuro dei valori correnti per il salvataggio
                double.TryParse(TxtDimGiocatori?.Text, out double dimG);
                double.TryParse(TxtDimStaff?.Text, out double dimSt);
                double.TryParse(TxtDimNomiSquadre?.Text, out double dimNSq);
                double.TryParse(TxtDimPunti?.Text, out double dimP);
                double.TryParse(TxtDimSet?.Text, out double dimS);
                double.TryParse(TxtDimStorico?.Text, out double dimStor);
                double.TryParse(TxtDimSetCorrente?.Text, out double dimSetCorr);
                double.TryParse(TxtPosYPunti?.Text, out double posYP);
                double.TryParse(TxtPosYSet?.Text, out double posYS);
                double.TryParse(TxtPosYStorico?.Text, out double posYStor);
                double.TryParse(TxtPosYSetCorrente?.Text, out double posYSetCorr);

                var model = new ConfigurazioneColoriModel
                {
                    SfondoGenerale = _correnteSfondoGen.ToString(),
                    SfondoSquadre = _correnteSfondoSq.ToString(),
                    SfondoNomeSq1 = _correnteSfondoNomeSq1.ToString(),
                    SfondoNomeSq2 = _correnteSfondoNomeSq2.ToString(),
                    ColoreCaratterePunti = _correnteCaratterePunti.ToString(),
                    ColoriCarattereSet = _correnteCarattereSet.ToString(),
                    ColoreCarattereStorico = _correnteCarattereStorico.ToString(),
                    ColoreCarattereSquadra1 = _correnteCarattereSq1.ToString(),
                    ColoreCarattereSquadra2 = _correnteCarattereSq2.ToString(),
                    ColoreGiocatori = _correnteGiocatori.ToString(),
                    ColoreLiberi = _correnteLiberi.ToString(),
                    ColoreCapitano = _correnteCapitano.ToString(),
                    ColoreStaff = _correnteStaff.ToString(),

                    BordoPunti = ChkBordoPunti?.IsChecked ?? false,
                    BordoSet = ChkBordoSet?.IsChecked ?? false,
                    BordoStorico = ChkBordoStorico?.IsChecked ?? false,
                    BordoSq1 = ChkBordoSq1?.IsChecked ?? false,
                    BordoSq2 = ChkBordoSq2?.IsChecked ?? false,
                    BordoGiocatori = ChkBordoGiocatori?.IsChecked ?? false,

                    DimensioneGiocatori = dimG == 0 ? 16 : dimG,
                    DimensioneStaff = dimSt == 0 ? 15 : dimSt,
                    DimensioneNomiSquadre = dimNSq == 0 ? 22 : dimNSq,
                    DimensionePunti = dimP == 0 ? 80 : dimP,
                    DimensioneSet = dimS == 0 ? 28 : dimS,
                    DimensioneStorico = dimStor == 0 ? 13 : dimStor,
                    DimensioneSetCorrente = dimSetCorr == 0 ? 18 : dimSetCorr,
                    PosizioneVerticalePunti = posYP,
                    PosizioneVerticaleSet = posYS,
                    PosizioneVerticaleSetCorrente = posYSetCorr,
                    PosizioneVerticaleStorico = posYStor
                };

                _configurazioneService.Salva(
                    _configurazioneService.PercorsoConfigurazioneColori,
                    model);

                MessageBox.Show("Configurazione salvata con successo!", "Salvataggio", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante il salvataggio della configurazione: " + ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChkAnimazioni_Checked(object sender, RoutedEventArgs e)
        {
            if (this.Owner is ConsoleControllo console) console.ImpostaPulsantiAnimazioni(true);
        }

        private void ChkAnimazioni_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.Owner is ConsoleControllo console) console.ImpostaPulsantiAnimazioni(false);
        }

        private void BtnChiudi_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
