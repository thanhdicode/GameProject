using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMenu.Library_Snake
{
    public class GameState
    {
        // Các thuộc tính để lưu trữ thông tin về trạng thái của trò chơi
        public int Rows { get; } // Số hàng của bảng trò chơi
        public int Cols { get; } // Số cột của bảng trò chơi
        public GridValue[,] Grid { get; } // Bảng trò chơi lưu trữ trạng thái các ô
        public Direction Dir { get; private set; } // Hướng di chuyển của rắn
        public int Score { get; private set; } // Điểm số của người chơi
        public bool GameOver { get; private set; } // Trạng thái kết thúc trò chơi

        // Danh sách liên kết để lưu trữ các thay đổi hướng và vị trí của rắn
        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Postition> snakePostitions = new LinkedList<Postition>();

        private readonly Random random = new Random();

        /// <summary>
        /// Khởi tạo trạng thái của trò chơi với số hàng và cột cho trước.
        /// </summary>
        /// <param name="rows">Số hàng của bảng.</param>
        /// <param name="cols">Số cột của bảng.</param>
        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right; // Khởi tạo hướng di chuyển của rắn mặc định là Right

            AddSnake(); // Thêm rắn vào bảng
            AddFood();  // Thêm thực phẩm vào bảng
        }

        // Thêm rắn vào bảng trò chơi, bắt đầu từ giữa bảng
        private void AddSnake()
        {
            int r = Rows / 2; // Xác định hàng giữa bảng
            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake; // Đặt giá trị của ô là Snake
                snakePostitions.AddFirst(new Postition(r, c)); // Thêm vị trí của rắn vào danh sách liên kết
            }
        }

        // Trả về các vị trí trống trong bảng trò chơi
        private IEnumerable<Postition> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty) // Kiểm tra ô trống
                    {
                        yield return new Postition(r, c); // Trả về vị trí trống
                    }
                }
            }
        }

        // Thêm thực phẩm vào bảng trò chơi tại một vị trí trống ngẫu nhiên
        private void AddFood()
        {
            List<Postition> empty = new List<Postition>(EmptyPositions());
            if (empty.Count == 0) // Nếu không còn vị trí trống
            {
                return;
            }

            Postition pos = empty[random.Next(empty.Count)]; // Chọn một vị trí trống ngẫu nhiên
            Grid[pos.Row, pos.Col] = GridValue.Food; // Đặt giá trị ô là Food
        }

        // Trả về vị trí đầu của rắn (vị trí đầu rắn)
        public Postition HeadPosition()
        {
            return snakePostitions.First.Value;
        }

        // Trả về vị trí đuôi của rắn (vị trí cuối rắn)
        public Postition TailPosition()
        {
            return snakePostitions.Last.Value;
        }

        // Trả về các vị trí của rắn
        public IEnumerable<Postition> SnakePositions()
        {
            return snakePostitions;
        }

        // Thêm đầu rắn vào vị trí mới
        private void AddHead(Postition pos)
        {
            snakePostitions.AddFirst(pos); // Thêm vị trí đầu vào danh sách liên kết
            Grid[pos.Row, pos.Col] = GridValue.Snake; // Đặt giá trị của ô là Snake
        }

        // Loại bỏ đuôi rắn (vị trí cuối rắn)
        private void RemoveTail()
        {
            Postition tail = snakePostitions.Last.Value; // Lấy vị trí đuôi
            Grid[tail.Row, tail.Col] = GridValue.Empty; // Đặt giá trị ô là Empty
            snakePostitions.RemoveLast(); // Loại bỏ đuôi khỏi danh sách liên kết
        }

        // Lấy hướng di chuyển cuối cùng từ danh sách thay đổi hướng
        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir; // Nếu không có thay đổi hướng, trả về hướng hiện tại
            }
            return dirChanges.Last.Value; // Trả về hướng di chuyển cuối cùng
        }

        // Kiểm tra xem hướng mới có thể thay đổi hay không
        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false; // Không cho phép thay đổi nếu đã có 2 hướng
            }

            Direction lastDir = GetLastDirection(); // Lấy hướng di chuyển cuối cùng
            return newDir != lastDir && newDir != lastDir.Opposite(); // Kiểm tra xem hướng mới có khác với hướng hiện tại và hướng đối diện không
        }

        // Thay đổi hướng di chuyển của rắn
        public void ChangeDirection(Direction dir)
        {
            // Nếu có thể thay đổi hướng
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir); // Thêm hướng mới vào danh sách thay đổi hướng
            }
        }

        // Kiểm tra xem vị trí có nằm ngoài bảng trò chơi không
        private bool OutsideGrid(Postition pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }

        // Xác định giá trị ô sẽ bị rắn va phải
        private GridValue WillHit(Postition newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside; // Nếu ngoài bảng trò chơi
            }

            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty; // Nếu va phải đuôi của rắn, ô trống
            }

            return Grid[newHeadPos.Row, newHeadPos.Col]; // Giá trị ô trên bảng
        }

        // Di chuyển rắn theo hướng hiện tại
        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value; // Cập nhật hướng di chuyển
                dirChanges.RemoveFirst(); // Xóa hướng đã sử dụng khỏi danh sách thay đổi hướng
            }

            Postition newHeadPos = HeadPosition().Translate(Dir); // Tính toán vị trí đầu rắn mới
            GridValue hit = WillHit(newHeadPos); // Xác định giá trị ô rắn sẽ va phải

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true; // Kết thúc trò chơi nếu va phải ngoài bảng hoặc va phải chính nó
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail(); // Loại bỏ đuôi rắn nếu va phải ô trống
                AddHead(newHeadPos); // Thêm đầu rắn vào vị trí mới
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos); // Thêm đầu rắn vào vị trí mới nếu va phải thực phẩm
                Score++; // Tăng điểm
                AddFood(); // Thêm thực phẩm mới vào bảng
            }
        }
    }
}
