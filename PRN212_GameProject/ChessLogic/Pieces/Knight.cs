using ChessLogic.Moves;

namespace ChessLogic.Pieces
{
    // Lớp Knight đại diện cho quân Mã trong trò chơi cờ vua
    public class Knight : Piece
    {
        public override PieceType Type => PieceType.Knight;
        public override Player Color { get; }

        public Knight(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Knight copy = new Knight(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        // Phương thức tĩnh PotentialToPosition trả về các vị trí mà quân Mã có thể di chuyển tới từ vị trí hiện tại
        private static IEnumerable<Position> PotentialToPosition(Position from)
        {
            foreach (Direction vDir in new Direction[] { Direction.North, Direction.South })
            {
                foreach (Direction hDir in new Direction[] { Direction.West, Direction.East })
                {
                    yield return from + 2 * vDir + hDir;// Di chuyển theo đường thang: 2 ô theo một hướng và 1 ô theo hướng khác
                    yield return from + 2 * hDir + vDir;
                }
            }
        }

        private IEnumerable<Position> MovePositions(Position from, Board board)
        {
            return PotentialToPosition(from).Where(pos => Board.IsInside(pos)
            && (board.IsEmpty(pos) || board[pos].Color != Color));
        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            // Gọi MovePositions để lấy các vị trí mà quân Mã có thể di chuyển
            // Sau đó, chuyển đổi mỗi vị trí thành một nước đi NormalMove
            return MovePositions(from, board).Select(to => new NormalMove(from, to));
        }
    }
}
