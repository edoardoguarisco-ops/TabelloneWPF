using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace TabelloneWPF
{
    public partial class FinestraConfig : Window
    {
        public static string PercorsoLogoDefault { get; private set; } = "";
        public static List<string> PercorsiSponsor { get; set; } = new List<string>();

        private static string CartellaSponsorPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "sponsor");

        public FinestraConfig()
        {
            InitializeComponent();
            CaricaConfigurazioneEsistente();
        }


        private void CaricaConfigurazioneEsistente()
        {
            try
            {
                // 1. Caricamento Immagini di Default (Cartella data/DEF)
                string cartellaDef = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "DEF");
                if (Directory.Exists(cartellaDef))
                {
                    string atleta = Path.Combine(cartellaDef, "fotoatletadefault.jpg");
                    if (File.Exists(atleta) && TxtFotoAtletaDefault != null) { TxtFotoAtletaDefault.Text = atleta; }

                    string staff = Path.Combine(cartellaDef, "fotostaffdefault.jpg");
                    if (File.Exists(staff) && TxtFotoStaffDefault != null) { TxtFotoStaffDefault.Text = staff; }

                    string arbitro = Path.Combine(cartellaDef, "fotoarbitrodefault.jpg");
                    if (File.Exists(arbitro) && TxtFotoArbitroDefault != null) { TxtFotoArbitroDefault.Text = arbitro; }

                    string logo = Path.Combine(cartellaDef, "logodefault.png");
                    if (File.Exists(logo) && TxtLogoDefault != null)
                    {
                        TxtLogoDefault.Text = logo;
                        PercorsoLogoDefault = logo;
                    }
                }

                // 2. Caricamento Sponsor (Cartella data/sponsor)
                if (ListSponsor != null)
                {
                    ListSponsor.Items.Clear();
                }
                PercorsiSponsor.Clear();

                InizializzaSponsorAutomatici();

                if (ListSponsor != null)
                {
                    foreach (var percorso in PercorsiSponsor)
                    {
                        if (!ListSponsor.Items.Contains(percorso))
                        {
                            ListSponsor.Items.Add(percorso);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento delle configurazioni: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void InizializzaSponsorAutomatici()
        {
            try
            {
                if (!Directory.Exists(CartellaSponsorPath))
                {
                    Directory.CreateDirectory(CartellaSponsorPath);
                }

                string[] fileSponsor = Directory.GetFiles(CartellaSponsorPath, "*.*");
                foreach (var file in fileSponsor)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        if (!PercorsiSponsor.Contains(file))
                        {
                            PercorsiSponsor.Add(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli sponsor: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAggiungiSponsor_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Immagini Sponsor (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (!Directory.Exists(CartellaSponsorPath))
                    {
                        Directory.CreateDirectory(CartellaSponsorPath);
                    }

                    foreach (var filename in openFileDialog.FileNames)
                    {
                        string nomeFile = Path.GetFileName(filename);
                        string destinazione = Path.Combine(CartellaSponsorPath, nomeFile);

                        // Evita conflitti se il file esiste già copiando con un suffisso o sovrascrivendo
                        File.Copy(filename, destinazione, true);

                        if (!PercorsiSponsor.Contains(destinazione))
                        {
                            PercorsiSponsor.Add(destinazione);
                        }

                        if (!ListSponsor.Items.Contains(destinazione))
                        {
                            ListSponsor.Items.Add(destinazione);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nel salvataggio dello sponsor: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRimuoviSponsor_Click(object sender, RoutedEventArgs e)
        {
            if (ListSponsor.SelectedItem != null)
            {
                var selezionato = ListSponsor.SelectedItem.ToString();
                ListSponsor.Items.Remove(ListSponsor.SelectedItem);
                PercorsiSponsor.Remove(selezionato);

                try
                {
                    if (File.Exists(selezionato))
                    {
                        File.Delete(selezionato);
                    }
                }
                catch { }
            }
        }

        private void BtnCaricaFotoAtletaDefault_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Immagini (*.png;*.jpg)|*.png;*.jpg" };
            if (dlg.ShowDialog() == true) TxtFotoAtletaDefault.Text = dlg.FileName;
        }

        private void BtnCaricaFotoStaffDefault_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Immagini (*.png;*.jpg)|*.png;*.jpg" };
            if (dlg.ShowDialog() == true) TxtFotoStaffDefault.Text = dlg.FileName;
        }

        private void BtnCaricaFotoArbitroDefault_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Immagini (*.png;*.jpg)|*.png;*.jpg" };
            if (dlg.ShowDialog() == true) TxtFotoArbitroDefault.Text = dlg.FileName;
        }

        private void BtnCaricaLogoDefault_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Immagini (*.png;*.jpg)|*.png;*.jpg" };
            if (dlg.ShowDialog() == true)
            {
                TxtLogoDefault.Text = dlg.FileName;
                PercorsoLogoDefault = dlg.FileName;
            }
        }

        private void BtnChiudi_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}