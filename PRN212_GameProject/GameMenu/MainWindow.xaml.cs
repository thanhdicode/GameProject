using System.Collections.Generic;
using System.Windows;

namespace GameMenu
{
    public partial class MainWindow : Window
    {
        private GameListWindow singlePlayerWindow;
        private GameListWindow multiPlayerWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void one_player_btn_Click(object sender, RoutedEventArgs e)
        {
            // Check if the single-player window is already open
            if (singlePlayerWindow == null || !singlePlayerWindow.IsVisible)
            {
                // Define single-player games with their images
                var singlePlayerGames = new List<Game>
                {
                    new Game { Name = "Space Shooter", ImagePath = "Logo_Game/space_shooter.jpg" },
                    new Game { Name = "FlappyBird", ImagePath = "Logo_Game/flappybird.jpg" },
                    new Game { Name = "Snake", ImagePath = "Logo_Game/snake.png" },
                };

                // Create and show the single-player window
                singlePlayerWindow = new GameListWindow(singlePlayerGames);
                singlePlayerWindow.Closed += (s, args) => singlePlayerWindow = null; // Reset the reference when closed
                singlePlayerWindow.Show();
            }
            else
            {
                // Bring the existing window to the front
                singlePlayerWindow.Activate();
            }
        }

        private void two_player_btn_Click(object sender, RoutedEventArgs e)
        {
            // Check if the multi-player window is already open
            if (multiPlayerWindow == null || !multiPlayerWindow.IsVisible)
            {
                // Define multi-player games with their images
                var multiPlayerGames = new List<Game>
                {
                    new Game { Name = "Chess", ImagePath = "Logo_Game/chess.jpg" },
                    new Game { Name = "Tetris", ImagePath = "Logo_Game/tetris.jpg" },
                    new Game { Name = "Memory", ImagePath = "Logo_Game/memory.png" },
                };

                // Create and show the multi-player window
                multiPlayerWindow = new GameListWindow(multiPlayerGames);
                multiPlayerWindow.Closed += (s, args) => multiPlayerWindow = null; // Reset the reference when closed
                multiPlayerWindow.Show();
            }
            else
            {
                // Bring the existing window to the front
                multiPlayerWindow.Activate();
            }
        }

        private void exit_btn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to exit?", "Confirm exit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
