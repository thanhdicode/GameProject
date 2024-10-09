using ChessLogic.Moves;
using ChessLogic.Pieces;
using ChessLogic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //// Mảng 2 chiều lưu trữ các hình ảnh quân cờ trên bàn cờ
        private readonly Image[,] pieceImages = new Image[8, 8];



        // Mảng 2 chiều lưu trữ các hình chữ nhật để làm nổi bật các nước đi hợp lệ
        private readonly Rectangle[,] hightlights = new Rectangle[8, 8];


        // Bộ nhớ tạm để lưu trữ các nước đi hợp lệ cho quân cờ được chọn
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();



        // Vị trí hiện tại của quân cờ đang được chọn
        private Position selectedPos = null;


        // Trạng thái hiện tại của trò chơi
        private GameState gameState;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();// Thiết lập các ô và hình ảnh trên bàn cờ
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);// Thiết lập con trỏ 

        }

        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;  // Lưu trữ hình ảnh trong mảng
                    PieceGrid.Children.Add(image);  // Thêm hình ảnh vào lưới giao diện

                    Rectangle hightlight = new Rectangle(); // tao cac huong dan nuoc di 
                    hightlights[r, c] = hightlight;
                    HighlightGrid.Children.Add(hightlight);

                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);

                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen()) // Nếu có menu nào đang hiển thị, bỏ qua 
            {
                return;
            }

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }

        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);  // Lấy các nước đi hợp lệ cho quân cờ tại vị trí đã chọn
            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }


        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();

            if (moveCache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
        }




        // truong hợp đặc biệt, nếu như quân tốt di chuyển tới tận biên giới bên kia ==> cho phép được thăng cấp thành siêu quân tốt =>> hiển thiện menu 

        // Em muốn trở thành gì nào ???? 
        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null; // Xóa hình ảnh quân cờ ở vị trí cũ
            PromotionMenu promotionMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promotionMenu;

            promotionMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };
        }

        private void HandleMove(Move move)
        {
            gameState.MakeMove(move);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            if (gameState.IsGameOver())
            {
                ShowGameOver();
            }

        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();

            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;

            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 125, 255, 125); // set màu cho highlight

            foreach (Position to in moveCache.Keys)
            {
                hightlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighlights() // sẽ ẩn highlight khi không được phép đi kh hợp lệ
        {
            foreach (Position to in moveCache.Keys)
            {
                hightlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player) // nước đi luân phiên giữa 2 player, thằng này xong thì tới thằng kia
        {
            if (player == Player.White)
            {
                Cursor = ChessCursors.WhiteCursor;
            }
            else
            {
                Cursor = ChessCursors.BlackCursor;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }


        private void ShowGameOver()
        {
            GameOverManu gameOverManu = new GameOverManu(gameState);
            MenuContainer.Content = gameOverManu;

            gameOverManu.OptionSelected += Option =>
            {
                if (Option == Option.Restart) // Neu chua phục muốn chơi lại thì quất luôn em !!!
                {
                    MenuContainer.Content = null; // Ẩn menu
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };

        }

        private void RestartGame()
        {
            selectedPos = null;
            HideHighlights();
            moveCache.Clear();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMenuOnScreen() && e.Key == Key.Escape) // neu nhan ESC thi show menu tạm dừng 
            {
                ShowPauseMenu();
            }
        }

        private void ShowPauseMenu()
        {
            PauseMenu pasuMenu = new PauseMenu();
            MenuContainer.Content = pasuMenu;
            pasuMenu.OptionSelected += Option =>
            {
                MenuContainer.Content = null;
                if (Option == Option.Restart)
                {
                    RestartGame();
                }
            };
        }
    }
}