using ChessLogic.Moves;

namespace ChessLogic.Pieces
{
    // Lớp abstract Piece đại diện cho một quân cờ trong trò chơi cờ vua
    public abstract class Piece
    {
        public abstract PieceType Type { get; }
        public abstract Player Color { get; }
        public bool HasMoved { get; set; } = false;
        public abstract Piece Copy();

        // Phương thức abstract GetMoves để lấy danh sách các nước đi hợp lệ của quân cờ từ vị trí hiện tại
        public abstract IEnumerable<Move> GetMoves(Position from, Board board);

        // Phương thức MovePositionsInDir trả về các vị trí mà quân cờ có thể di chuyển theo một hướng nhất định

        /*
         Trong C#, từ khóa yield được sử dụng để trả về từng phần tử một trong một chuỗi giá trị mà một phương thức IEnumerable hoặc IEnumerator có thể lặp qua.
        Khi phương thức chứa yield return được gọi, nó trả về một phần tử tại một thời điểm, sau đó tạm dừng thực thi và giữ trạng thái của phương thức cho lần gọi
        tiếp theo. Điều này giúp tiết kiệm bộ nhớ và tối ưu hóa hiệu suất, đặc biệt khi làm việc với các tập dữ liệu lớn hoặc các chuỗi không xác định trước độ
        dài.
         */
        protected IEnumerable<Position> MovePositionsInDir(Position from, Board board, Direction dir)
        {
            for (Position pos = from + dir; Board.IsInside(pos); pos += dir)
            {
                // Nếu vị trí đang xét trống, trả về vị trí này
                if (board.IsEmpty(pos))
                {
                    yield return pos;
                    continue;
                }

                // Nếu vị trí có quân cờ, kiểm tra xem quân cờ đó có màu khác với quân cờ hiện tại
                Piece piece = board[pos];

                if (piece.Color != Color)
                {
                    // Nếu có, trả về vị trí này vì quân cờ có thể ăn quân địch
                    yield return pos;
                }
                // Nếu không, kết thúc vòng lặp vì quân cờ không thể di chuyển xa hơn
                yield break;
            }
        }

        protected IEnumerable<Position> MovePositionInDirs(Position from, Board board, Direction[] dirs)
        {
            return dirs.SelectMany(dir => MovePositionsInDir(from, board, dir));
        }

        public virtual bool CanCaptureOpponentKing(Position from, Board board)
        {
            return GetMoves(from, board).Any(move =>
            {
                Piece piece = board[move.ToPos];
                return piece != null && piece.Type == PieceType.King;

            });

        }
    }
}
