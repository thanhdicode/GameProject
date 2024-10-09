using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.ViewModel
{
    public class StartMenuViewModel
    {
        private readonly MainWindow _mainWindow;
        public StartMenuViewModel(MainWindow main) => _mainWindow = main;

        public void StartNewGame(int categoryIndex)
        {
            var category = (BlockCategories)categoryIndex;
            GameViewModel newGame = new(category);
            _mainWindow.DataContext = newGame;
        }
        public void NavigateToStartMenu()
        {
            _mainWindow.DataContext = new StartMenuViewModel(_mainWindow);
        }
    }
}
