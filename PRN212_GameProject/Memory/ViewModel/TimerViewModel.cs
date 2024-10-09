using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Memory.ViewModel
{
    public class TimerViewModel : ObservableObject
    {
        private DispatcherTimer _playedTimer;
        private TimeSpan _timePlayed;
        public TimeSpan Time
        {
            get => _timePlayed;
            set
            {
                _timePlayed = value;
                OnPropertyChanged("Time");
            }
        }
        private void PlayedTimer_Tick(object sender, EventArgs e)
        {
            Time = _timePlayed.Add(new TimeSpan(0, 0, 1));
        }
        public TimerViewModel(TimeSpan time)
        {
            _playedTimer = new DispatcherTimer();
            _playedTimer.Interval = time;
            _playedTimer.Tick += PlayedTimer_Tick;
            _timePlayed = new TimeSpan();
        }
        public void Start()
        {
            _playedTimer.Start();
        }
        public void Stop()
        {
            _playedTimer.Stop();
        }

    }
}
