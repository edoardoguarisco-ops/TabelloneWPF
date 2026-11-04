using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TabelloneWPF.Tests
{
    [TestClass]
    public class PunteggioServiceTests
    {
        [TestMethod]
        public void NuovaPartitaParteDaZeroNelPrimoSet()
        {
            var punteggio = new PunteggioService();

            Assert.AreEqual(0, punteggio.PuntiSquadra1);
            Assert.AreEqual(0, punteggio.PuntiSquadra2);
            Assert.AreEqual(0, punteggio.SetSquadra1);
            Assert.AreEqual(0, punteggio.SetSquadra2);
            Assert.AreEqual(1, punteggio.SetCorrente);
        }

        [TestMethod]
        public void IlSetNonFinisceSenzaDuePuntiDiVantaggio()
        {
            var punteggio = new PunteggioService();

            AggiungiPunti(punteggio, 25, 24);

            Assert.AreEqual(0, punteggio.SetSquadra1);
            Assert.AreEqual(1, punteggio.SetCorrente);
            Assert.AreEqual(25, punteggio.PuntiSquadra1);
            Assert.AreEqual(24, punteggio.PuntiSquadra2);

            punteggio.PuntoSquadra2();
            punteggio.PuntoSquadra1();

            Assert.AreEqual(0, punteggio.SetSquadra1);
            Assert.AreEqual(26, punteggio.PuntiSquadra1);
            Assert.AreEqual(25, punteggio.PuntiSquadra2);
        }

        [TestMethod]
        public void IlSetVintoPassaAlSetSuccessivo()
        {
            var punteggio = new PunteggioService();

            AggiungiPunti(punteggio, 26, 24);

            Assert.AreEqual(1, punteggio.SetSquadra1);
            Assert.AreEqual(0, punteggio.SetSquadra2);
            Assert.AreEqual(2, punteggio.SetCorrente);
            Assert.AreEqual(0, punteggio.PuntiSquadra1);
            Assert.AreEqual(0, punteggio.PuntiSquadra2);
            Assert.AreEqual("Set 1: 26 - 24", punteggio.StoricoFormattato);
        }

        [TestMethod]
        public void IlQuintoSetSiGiocaAQuindiciPunti()
        {
            var punteggio = new PunteggioService();

            for (int i = 0; i < 4; i++)
                AggiungiPunti(punteggio, 25, 23);

            Assert.AreEqual(4, punteggio.SetSquadra1);
            Assert.AreEqual(5, punteggio.SetCorrente);

            AggiungiPunti(punteggio, 15, 13);

            Assert.AreEqual(5, punteggio.SetSquadra1);
            Assert.AreEqual(5, punteggio.SetCorrente);
            Assert.AreEqual(0, punteggio.PuntiSquadra1);
            Assert.AreEqual(0, punteggio.PuntiSquadra2);
        }

        [TestMethod]
        public void AnnullaPuntoNonScendeSottoZero()
        {
            var punteggio = new PunteggioService();

            punteggio.AnnullaPuntoSquadra1();
            punteggio.AnnullaPuntoSquadra2();

            Assert.AreEqual(0, punteggio.PuntiSquadra1);
            Assert.AreEqual(0, punteggio.PuntiSquadra2);

            punteggio.PuntoSquadra1();
            punteggio.AnnullaPuntoSquadra1();
            punteggio.AnnullaPuntoSquadra1();

            Assert.AreEqual(0, punteggio.PuntiSquadra1);
        }

        [TestMethod]
        public void AnnullaSetRipristinaIlSetPrecedente()
        {
            var punteggio = new PunteggioService();
            AggiungiPunti(punteggio, 25, 23);

            punteggio.AnnullaSet();

            Assert.AreEqual(0, punteggio.SetSquadra1);
            Assert.AreEqual(0, punteggio.SetSquadra2);
            Assert.AreEqual(1, punteggio.SetCorrente);
            Assert.AreEqual(25, punteggio.PuntiSquadra1);
            Assert.AreEqual(23, punteggio.PuntiSquadra2);
            Assert.AreEqual("Set 1: 0 - 0", punteggio.StoricoFormattato);
        }

        [TestMethod]
        public void ResetSetAzzeraPunteggioSetEStorico()
        {
            var punteggio = new PunteggioService();
            AggiungiPunti(punteggio, 25, 23);
            punteggio.PuntoSquadra1();

            punteggio.ResetSet();

            Assert.AreEqual(0, punteggio.PuntiSquadra1);
            Assert.AreEqual(0, punteggio.PuntiSquadra2);
            Assert.AreEqual(0, punteggio.SetSquadra1);
            Assert.AreEqual(0, punteggio.SetSquadra2);
            Assert.AreEqual(1, punteggio.SetCorrente);
            Assert.AreEqual("Set 1: 0 - 0", punteggio.StoricoFormattato);
        }

        private static void AggiungiPunti(
            PunteggioService punteggio,
            int puntiSquadra1,
            int puntiSquadra2)
        {
            int turni = puntiSquadra1 > puntiSquadra2
                ? puntiSquadra1
                : puntiSquadra2;

            for (int i = 0; i < turni; i++)
            {
                if (i < puntiSquadra1)
                    punteggio.PuntoSquadra1();

                if (i < puntiSquadra2)
                    punteggio.PuntoSquadra2();
            }
        }
    }
}
