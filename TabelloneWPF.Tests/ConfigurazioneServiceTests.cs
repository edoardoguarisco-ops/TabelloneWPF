using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TabelloneWPF.Tests
{
    [TestClass]
    public class ConfigurazioneServiceTests
    {
        private string _cartellaTemporanea = "";

        [TestInitialize]
        public void Inizializza()
        {
            _cartellaTemporanea = Path.Combine(
                Path.GetTempPath(),
                "TabelloneWPF.Tests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_cartellaTemporanea);
        }

        [TestCleanup]
        public void Pulisci()
        {
            if (Directory.Exists(_cartellaTemporanea))
                Directory.Delete(_cartellaTemporanea, true);
        }

        [TestMethod]
        public void SalvaECariacaMantengonoLaConfigurazione()
        {
            var servizio = new ConfigurazioneService();
            string percorso = Path.Combine(_cartellaTemporanea, "config", "squadre.xml");
            var configurazione = new ConfigClassXML
            {
                NomeSquadra1 = "Squadra A",
                NomeSquadra2 = "Squadra B",
                Rosa1 =
                {
                    new ItemXml
                    {
                        Numero = "7",
                        Nome = "Mario",
                        Cognome = "Rossi",
                        IsCapitano = true
                    }
                }
            };

            servizio.Salva(percorso, configurazione);
            ConfigClassXML? caricata = servizio.Carica<ConfigClassXML>(percorso);

            Assert.IsNotNull(caricata);
            Assert.AreEqual("Squadra A", caricata!.NomeSquadra1);
            Assert.AreEqual("Squadra B", caricata.NomeSquadra2);
            Assert.AreEqual(1, caricata.Rosa1.Count);
            Assert.AreEqual("Rossi", caricata.Rosa1[0].Cognome);
            Assert.IsTrue(caricata.Rosa1[0].IsCapitano);
        }

        [TestMethod]
        public void SalvaCreaLeCartelleMancanti()
        {
            var servizio = new ConfigurazioneService();
            string percorso = Path.Combine(_cartellaTemporanea, "cartella", "config.xml");

            servizio.Salva(percorso, new ConfigClassXML());

            Assert.IsTrue(File.Exists(percorso));
        }

        [TestMethod]
        public void CaricaRestituisceNullPerFileInesistente()
        {
            var servizio = new ConfigurazioneService();
            string percorso = Path.Combine(_cartellaTemporanea, "inesistente.xml");

            ConfigClassXML? configurazione = servizio.Carica<ConfigClassXML>(percorso);

            Assert.IsNull(configurazione);
        }

        [TestMethod]
        public void RisolviPercorsoRestituisceIlPercorsoAssolutoEsistente()
        {
            var servizio = new ConfigurazioneService();
            string percorso = Path.Combine(_cartellaTemporanea, "foto.jpg");
            File.WriteAllText(percorso, "foto di test");

            string risultato = servizio.RisolviPercorsoFileConfigurazione(percorso);

            Assert.AreEqual(percorso, risultato);
        }

        [TestMethod]
        public void RisolviPercorsoRestituisceIlValoreOriginaleSeNonTrovaIlFile()
        {
            var servizio = new ConfigurazioneService();
            const string percorso = "foto-non-presente.jpg";

            string risultato = servizio.RisolviPercorsoFileConfigurazione(percorso);

            Assert.AreEqual(percorso, risultato);
        }

        [TestMethod]
        public void RisolviPercorsoGestisceValoreVuoto()
        {
            var servizio = new ConfigurazioneService();

            Assert.AreEqual("", servizio.RisolviPercorsoFileConfigurazione(null));
            Assert.AreEqual("", servizio.RisolviPercorsoFileConfigurazione("   "));
        }
    }
}
