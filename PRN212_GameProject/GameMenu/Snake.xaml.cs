using GameMenu.Library_Snake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameMenu
{
    /// <summary>
    /// Interaction logic for Snake.xaml
    /// </summary>
    public partial class Snake : Window
    {
        // Từ điển để ánh xạ giá trị của lưới với hình ảnh tương ứng
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food }
        };

        // Từ điển để ánh xạ hướng di chuyển với góc xoay tương ứng
        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        private readonly int rows = 15, cols = 15; // Số hàng và số cột của lưới
        private readonly Image[,] gridImages; // Mảng hình ảnh đại diện cho lưới
        private GameState gameState; // Trạng thái của trò chơi
        private bool gameRunning; //  để kiểm tra xem trò chơi đang chạy hay không
        public Snake()
        {
            InitializeComponent();
            gridImages = SetupGrid(); // Thiết lập lưới
            gameState = new GameState(rows, cols); // Khởi tạo trạng thái trò chơi
        }
        // Hàm chạy trò chơi
        private async Task RunGame()
        {
            Draw(); // Vẽ lưới ban đầu
            await ShowCountDown(); // Hiển thị đếm ngược

            Overlay.Visibility = Visibility.Hidden; // Ẩn lớp phủ
            await GameLoop(); // Chạy vòng lặp trò chơi
            await ShowGameOver(); // Hiển thị thông báo game over
            gameState = new GameState(rows, cols); // Khởi tạo lại trạng thái trò chơi
        }

        // Hàm xử lý sự kiện khi nhấn phím
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame(); // Bắt đầu trò chơi
                gameRunning = false;
            }
        }

        // Hàm xử lý sự kiện khi nhấn phím để thay đổi hướng di chuyển
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }

        // Vòng lặp trò chơi
        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100); // Đợi 100ms giữa mỗi bước di chuyển
                gameState.Move(); // Di chuyển rắn
                Draw(); // Vẽ lại lưới
            }
        }

        // Thiết lập lưới
        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;

            GameGrid.Width = GameGrid.Height * (cols / (double)rows);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };
                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }
            return images;
        }

        // Vẽ lại lưới
        private void Draw()
        {
            DrawGrid(); // Vẽ lưới
            DrawSnakeHead(); // Vẽ đầu rắn
            ScoreText.Text = $"SCORE {gameState.Score}"; // Cập nhật điểm số
        }

        // Vẽ các ô trong lưới
        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        // Vẽ đầu rắn
        private void DrawSnakeHead()
        {
            Postition headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        // Vẽ rắn đã chết
        private async Task DrawDeadSnake()
        {
            List<Postition> positions = new List<Postition>(gameState.SnakePositions());
            for (int i = 0; i < positions.Count; i++)
            {
                Postition pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }

        // Hiển thị đếm ngược
        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        // Hiển thị thông báo game over
        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "GAME OVER !!! PRESS ANY KEY TO START";
        }
    }
}
