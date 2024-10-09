using ChessLogic.Moves;
using ChessLogic.Pieces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class Board
    {
        // Mảng 2 chiều để lưu trữ các quân cờ trên bàn cờ (8x8).
        private readonly Piece[,] pieces = new Piece[8, 8];

        // Dictionary để lưu trữ các vị trí mà quân tốt có thể bỏ qua (en passant) cho từng người chơi.
        private readonly Dictionary<Player, Position> pawnSkipPositions = new Dictionary<Player, Position>
        {
            { Player.White, null },
            { Player.Black, null }
        };

        // Truy cập quân cờ bằng cách sử dụng chỉ số hàng và cột.
        public Piece this[int row, int col]
        {
            get { return pieces[row, col]; }
            set { pieces[row, col] = value; }
        }

        // Truy cập quân cờ bằng cách sử dụng đối tượng Position.
        public Piece this[Position pos]
        {
            get { return this[pos.Row, pos.Column]; }
            set { this[pos.Row, pos.Column] = value; }
        }

        // Lấy vị trí mà quân tốt có thể bỏ qua (en passant) cho người chơi.
        public Position GetPawnSkipPositons(Player player)
        {
            return pawnSkipPositions[player];
        }

        // Đặt vị trí mà quân tốt có thể bỏ qua (en passant) cho người chơi.
        public void SetPawnSkipPosition(Player player, Position pos)
        {
            pawnSkipPositions[player] = pos;
        }

        // Tạo bàn cờ khởi đầu với các quân cờ đặt ở vị trí mặc định.
        public static Board Initial()
        {
            Board board = new Board();
            board.AddStartPieces();
            return board;
        }

        // Thêm các quân cờ khởi đầu vào bàn cờ.
        private void AddStartPieces()
        {
            // Đặt quân cờ cho hai hàng đầu tiên (hàng 0 và hàng 7) cho cả hai người chơi.
            this[0, 0] = new Rook(Player.Black);
            this[0, 1] = new Knight(Player.Black);
            this[0, 2] = new Bishop(Player.Black);
            this[0, 3] = new Queen(Player.Black);
            this[0, 4] = new King(Player.Black);
            this[0, 5] = new Bishop(Player.Black);
            this[0, 6] = new Knight(Player.Black);
            this[0, 7] = new Rook(Player.Black);

            this[7, 0] = new Rook(Player.White);
            this[7, 1] = new Knight(Player.White);
            this[7, 2] = new Bishop(Player.White);
            this[7, 3] = new Queen(Player.White);
            this[7, 4] = new King(Player.White);
            this[7, 5] = new Bishop(Player.White);
            this[7, 6] = new Knight(Player.White);
            this[7, 7] = new Rook(Player.White);

            // Đặt quân tốt cho hàng 1 và hàng 6 cho cả hai người chơi.
            for (int c = 0; c < 8; c++)
            {
                this[1, c] = new Pawn(Player.Black);
                this[6, c] = new Pawn(Player.White);
            }
        }

        // Kiểm tra xem một vị trí có nằm trong bàn cờ không (0 <= Row < 8 và 0 <= Column < 8).
        public static bool IsInside(Position pos)
        {
            return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
        }

        // Kiểm tra xem một vị trí có trống không.
        public bool IsEmpty(Position pos)
        {
            return this[pos] == null;
        }

        // Trả về danh sách các vị trí của quân cờ trên bàn cờ.
        public IEnumerable<Position> PiecePositions()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);
                    if (!IsEmpty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        // Trả về danh sách các vị trí của quân cờ thuộc về người chơi cụ thể.
        public IEnumerable<Position> PiecePositionsFor(Player player)
        {
            return PiecePositions().Where(pos => this[pos].Color == player);
        }

        // Kiểm tra xem người chơi có bị chiếu không.
        public bool IsInCheck(Player player)
        {
            return PiecePositionsFor(player.Opponent()).Any(pos =>
            {
                Piece piece = this[pos];
                return piece.CanCaptureOpponentKing(pos, this);
            });
        }

        // Tạo bản sao của bàn cờ.
        public Board Copy()
        {
            Board copy = new Board();
            foreach (Position pos in PiecePositions())
            {
                copy[pos] = this[pos].Copy();
            }

            return copy;
        }

        // Đếm số lượng quân cờ của mỗi loại và màu sắc trên bàn cờ.
        public Counting CountPieces()
        {
            Counting counting = new Counting();
            foreach (Position pos in PiecePositions())
            {
                Piece piece = this[pos];
                counting.Increment(piece.Color, piece.Type);
            }

            return counting;
        }

        // Kiểm tra xem bàn cờ có đủ quân cờ để tiếp tục chơi không (kiểm tra tình huống hòa vì thiếu quân cờ).
        public bool InSufficientMaterial()
        {
            Counting counting = CountPieces();

            return IsKingVKing(counting) || IsKingBishopVKing(counting) || IsKingKnightVKing(counting) ||
                   IsKingBishopVKingBishop(counting);
        }

        // Kiểm tra tình huống "vua đấu vua" (chỉ còn hai quân vua).
        private static bool IsKingVKing(Counting counting)
        {
            return counting.TotalCount == 2;
        }

        // Kiểm tra tình huống "vua và sĩ đấu vua" (chỉ còn một quân vua và một quân sĩ).
        private static bool IsKingBishopVKing(Counting counting)
        {
            return counting.TotalCount == 3 &&
                   (counting.White(PieceType.Bishop) == 1 || counting.Black(PieceType.Bishop) == 1);
        }

        // Kiểm tra tình huống "vua và mã đấu vua" (chỉ còn một quân vua và một quân mã).
        private static bool IsKingKnightVKing(Counting counting)
        {
            return counting.TotalCount == 3 &&
                   (counting.White(PieceType.Knight) == 1 || counting.Black(PieceType.Knight) == 1);
        }

        // Kiểm tra tình huống "vua và sĩ đấu vua và sĩ" (chỉ còn một quân vua và một quân sĩ của mỗi bên).
        private bool IsKingBishopVKingBishop(Counting counting)
        {
            if (counting.TotalCount != 4)
            {
                return false;
            }

            if (counting.White(PieceType.Bishop) != 1 || counting.Black(PieceType.Bishop) != 1)
            {
                return false;
            }

            Position wBishopPos = FindPiece(Player.White, PieceType.Bishop);
            Position bBishopPos = FindPiece(Player.Black, PieceType.Bishop);

            return wBishopPos.SquareColor() == bBishopPos.SquareColor();
        }

        // Tìm quân cờ cụ thể của người chơi tại một vị trí.
        private Position FindPiece(Player color, PieceType type)
        {
            return PiecePositionsFor(color).First(pos => this[pos].Type == type);
        }

        // Kiểm tra xem vua và xe có di chuyển chưa và có nằm ở vị trí phù hợp cho rochade không.
        private bool IsUnmovedKingAndRook(Position kingPos, Position rookPos)
        {
            if (IsEmpty(kingPos) || IsEmpty(rookPos))
            {
                return false;
            }

            Piece King = this[kingPos];
            Piece rook = this[rookPos];

            return King.Type == PieceType.King && rook.Type == PieceType.Rook && !King.HasMoved && !rook.HasMoved;
        }

        // Kiểm tra quyền rochade ngắn (Kingside) cho người chơi cụ thể.
        public bool CastleRightKS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 7)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 7)),
                _ => false
            };
        }

        // Kiểm tra quyền rochade dài (Queenside) cho người chơi cụ thể.
        public bool CastleRightQS(Player player)
        {
            return player switch
            {
                Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 0)),
                Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 0)),
                _ => false
            };
        }

        // Kiểm tra xem có quân tốt nào có thể thực hiện nước đi en passant không.
        private bool HasPawnInPosition(Player player, Position[] pawnPositions, Position skipPos)
        {
            foreach (Position pos in pawnPositions.Where(IsInside))
            {
                Piece piece = this[pos];
                if (piece == null || piece.Color != player || piece.Type != PieceType.Pawn)
                {
                    continue;
                }

                EnPassant move = new EnPassant(pos, skipPos);
                if (move.IsLegal(this))
                {
                    return true;
                }
            }

            return false;
        }

        // Kiểm tra xem người chơi có thể thực hiện nước đi en passant không.
        public bool CanCaptureEnPassant(Player player)
        {
            Position skipPos = GetPawnSkipPositons(player.Opponent());
            if (skipPos == null)
            {
                return false;
            }

            Position[] pawnPositions = player switch
            {
                Player.White => new Position[] { skipPos + Direction.SouthEast, skipPos + Direction.SouthWest },
                Player.Black => new Position[] { skipPos + Direction.NorthEast, skipPos + Direction.NorthWest },
                _ => Array.Empty<Position>()
            };

            return HasPawnInPosition(player, pawnPositions, skipPos);
        }
    }
}
