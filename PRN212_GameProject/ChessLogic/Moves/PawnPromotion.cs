using ChessLogic.Pieces;

namespace ChessLogic.Moves
{
    public class PawnPromotion : Move
    {
        public override MoveType Type => MoveType.PawnPromotion;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        private readonly PieceType newType;
        public PawnPromotion(Position fromPos, Position toPos, PieceType newType)
        {
            FromPos = fromPos;
            ToPos = toPos;
            this.newType = newType;
        }


        // tùy vào bên nào thăng cấp thì sẽ chọn màu cho bên đó 
        private Piece CreatePromotionPiece(Player color)
        {
            return newType switch
            {
                PieceType.Knight => new Knight(color),
                PieceType.Bishop => new Bishop(color),
                PieceType.Rook => new Rook(color),
                _ => new Queen(color)

            };
        }

        public override bool Execute(Board board)
        {
            Piece pawn = board[FromPos];
            board[FromPos] = null;
            Piece promotionPiece = CreatePromotionPiece(pawn.Color);
            promotionPiece.HasMoved = true;
            board[ToPos] = promotionPiece;

            return true;
        }

        /*  Xóa quân tốt tại vị trí xuất phát: board[FromPos] = null;
          Tạo quân cờ mới: Piece promotionPiece = CreatePromotionPiece(pawn.Color);
          Đánh dấu quân cờ mới đã di chuyển: promotionPiece.HasMoved = true;
          Đặt quân cờ mới tại vị trí đích: board[ToPos] = promotionPiece; */
    }
}
