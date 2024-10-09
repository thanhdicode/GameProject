using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Memory.ViewModel
{
    public class GameInfoViewModel : ObservableObject
    {
        private const int _maxAttempts = 10;
        private const int _pointAward = 75;
        private const int _pointDeduction = 15;

        private int _matchAttempts;
        private int _score;

        private bool _gameLost;
        private bool _gameWon;
        public int MatchAttempts
        {
            get => _matchAttempts;
            private set
            {
                _matchAttempts = value;
                OnPropertyChanged(nameof(MatchAttempts));
            }
        }
        public int Score
        {
            get => _score;
            private set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }
        public Visibility LostMessage
        {
            get
            {
                if (_gameLost) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }
        public Visibility WonMessage
        {
            get
            {
                if (_gameWon) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }
        public void GameStatus(bool win)
        {
            if (!win)
            {
                _gameLost = true;
                OnPropertyChanged(nameof(LostMessage));
            }
            else
            {
                _gameWon = true;
                OnPropertyChanged(nameof(WonMessage));
            }
        }
        public void ClearInfo()
        {
            Score = 0;
            MatchAttempts = _maxAttempts;
            _gameLost = false;
            _gameWon = false;
            OnPropertyChanged(nameof(LostMessage));
            OnPropertyChanged(nameof(WonMessage));
        }
        public void Award()
        {
            Score += _pointAward;
            MatchAttempts--;
        }
        public void Penalize()
        {
            Score -= _pointDeduction;
            MatchAttempts--;
        }
    }
}
