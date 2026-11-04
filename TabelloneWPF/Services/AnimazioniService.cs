using System;
using System.Threading;
using System.Threading.Tasks;

namespace TabelloneWPF
{
    public sealed class AnimazioniService
    {
        private CancellationTokenSource? _presentazioneCancellationTokenSource;
        private CancellationTokenSource? _sponsorCancellationTokenSource;

        public bool IsPresentazioneRunning { get; private set; }
        public bool IsSponsorRunning { get; private set; }

        public async Task EseguiPresentazioneAsync(
            Func<CancellationToken, Task> presentazione)
        {
            if (IsPresentazioneRunning)
                return;

            IsPresentazioneRunning = true;
            _presentazioneCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await presentazione(_presentazioneCancellationTokenSource.Token);
            }
            finally
            {
                IsPresentazioneRunning = false;
                _presentazioneCancellationTokenSource.Dispose();
                _presentazioneCancellationTokenSource = null;
            }
        }

        public void InterrompiPresentazione()
        {
            _presentazioneCancellationTokenSource?.Cancel();
        }

        public async Task EseguiSponsorAsync(
            Func<CancellationToken, Task> animazione)
        {
            if (IsSponsorRunning)
                return;

            IsSponsorRunning = true;
            _sponsorCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await animazione(_sponsorCancellationTokenSource.Token);
            }
            finally
            {
                IsSponsorRunning = false;
                _sponsorCancellationTokenSource.Dispose();
                _sponsorCancellationTokenSource = null;
            }
        }

        public void InterrompiSponsor()
        {
            _sponsorCancellationTokenSource?.Cancel();
        }
    }
}
