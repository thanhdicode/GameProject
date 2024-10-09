using ChessLogic.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic.Pieces
{
    public class Rook : Piece
    {
        public override PieceType Type => PieceType.Rook;
        public override Player Color { get; }


        private static readonly Direction[] dirs = new Direction[]
       {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West,
       };

        public Rook(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Rook copy = new Rook(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            // Gọi MovePositionInDirs để lấy các vị trí có thể di chuyển theo các hướng đường thang
            // Sau đó, chuyển đổi mỗi vị trí thành một nước đi NormalMove
            return MovePositionInDirs(from, board, dirs).Select(to => new NormalMove(from, to));
        }
    }
}
