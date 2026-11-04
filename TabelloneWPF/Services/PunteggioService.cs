using System;
using System.Collections.Generic;

namespace TabelloneWPF
{
    public sealed class PunteggioService
    {
        private const int NumeroMassimoSet = 5;
        private readonly int[] _storicoPunti1 = new int[NumeroMassimoSet];
        private readonly int[] _storicoPunti2 = new int[NumeroMassimoSet];

        public int PuntiSquadra1 { get; private set; }
        public int PuntiSquadra2 { get; private set; }
        public int SetSquadra1 { get; private set; }
        public int SetSquadra2 { get; private set; }
        public int SetCorrente { get; private set; } = 1;

        public string StoricoFormattato
        {
            get
            {
                var storico = new List<string>();
                int setDaMostrare = SetCorrente == NumeroMassimoSet
                    ? SetCorrente
                    : SetCorrente - 1;

                for (int i = 0; i < setDaMostrare; i++)
                {
                    storico.Add($"Set {i + 1}: {_storicoPunti1[i]} - {_storicoPunti2[i]}");
                }

                return storico.Count > 0
                    ? string.Join(Environment.NewLine, storico)
                    : "Set 1: 0 - 0";
            }
        }

        public void PuntoSquadra1()
        {
            PuntiSquadra1++;
            ControllaFineSet();
        }

        public void PuntoSquadra2()
        {
            PuntiSquadra2++;
            ControllaFineSet();
        }

        public void AnnullaPuntoSquadra1()
        {
            if (PuntiSquadra1 > 0)
                PuntiSquadra1--;
        }

        public void AnnullaPuntoSquadra2()
        {
            if (PuntiSquadra2 > 0)
                PuntiSquadra2--;
        }

        public void ResetPunti()
        {
            PuntiSquadra1 = 0;
            PuntiSquadra2 = 0;
        }

        public void ResetSet()
        {
            SetSquadra1 = 0;
            SetSquadra2 = 0;
            SetCorrente = 1;
            PuntiSquadra1 = 0;
            PuntiSquadra2 = 0;
            Array.Clear(_storicoPunti1, 0, _storicoPunti1.Length);
            Array.Clear(_storicoPunti2, 0, _storicoPunti2.Length);
        }

        public void AnnullaSet()
        {
            if (SetCorrente <= 1)
                return;

            int setPrecedente = SetCorrente - 1;
            int indice = setPrecedente - 1;

            if (_storicoPunti1[indice] > _storicoPunti2[indice])
            {
                if (SetSquadra1 > 0)
                    SetSquadra1--;
            }
            else if (SetSquadra2 > 0)
            {
                SetSquadra2--;
            }

            PuntiSquadra1 = _storicoPunti1[indice];
            PuntiSquadra2 = _storicoPunti2[indice];
            SetCorrente = setPrecedente;
        }

        private void ControllaFineSet()
        {
            int puntiLimite = SetCorrente == NumeroMassimoSet ? 15 : 25;

            if ((PuntiSquadra1 < puntiLimite && PuntiSquadra2 < puntiLimite) ||
                Math.Abs(PuntiSquadra1 - PuntiSquadra2) < 2)
            {
                return;
            }

            int indice = SetCorrente - 1;
            _storicoPunti1[indice] = PuntiSquadra1;
            _storicoPunti2[indice] = PuntiSquadra2;

            if (PuntiSquadra1 > PuntiSquadra2)
                SetSquadra1++;
            else
                SetSquadra2++;

            if (SetCorrente < NumeroMassimoSet)
                SetCorrente++;

            PuntiSquadra1 = 0;
            PuntiSquadra2 = 0;
        }
    }
}
