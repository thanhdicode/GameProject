using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.ViewModel
{
    public enum BlockCategories
    {
        Animals,
        Cars,
        Foods
    }
    public class GameViewModel : ObservableObject
    {
        public BlockCollectionViewModel Blocks { get; private set; }
        public TimerViewModel Timer { get; private set; }
        public GameInfoViewModel GameInfo { get; private set; }
        public BlockCategories Category { get; private set; }
        public GameViewModel(BlockCategories category)
        {
            Category = category;
            SetupGame(category);
        }
        public void SetupGame(BlockCategories category)
        {
            Blocks = new BlockCollectionViewModel();
            Timer = new TimerViewModel(new TimeSpan(0, 0, 1));
            GameInfo = new GameInfoViewModel();

            GameInfo.ClearInfo();

            Blocks.CreateBlocks("Assets_Memory/" + category.ToString());
            Blocks.Memorize();

            Timer.Start();

            OnPropertyChanged(nameof(Blocks));
            OnPropertyChanged(nameof(Timer));
            OnPropertyChanged(nameof(GameInfo));
        }
        public void ClickedBlock(object block)
        {
            //var selected = block as BlockViewModel;
            Blocks.SelectBlock((BlockViewModel)block);
            if (!Blocks.AreBlocksActive)
            {
                if (Blocks.CheckIfMatched()) GameInfo.Award();
                else GameInfo.Penalize();
            }
            GameStatus();
        }
        public void GameStatus()
        {
            if (GameInfo.MatchAttempts <= 0)
            {
                GameInfo.GameStatus(false);
                Blocks.RevealUnMatchedAndMatched();
                Timer.Stop();
            }
            if (Blocks.AllBlockMatched)
            {
                GameInfo.GameStatus(true);
                Blocks.RevealUnMatchedAndMatched();
                Timer.Stop();
            }
        }
        public void Restart()
        {
            SetupGame(Category);
        }
    }
}
