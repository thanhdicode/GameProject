namespace ChessLogic.Moves
{
    public class EnPassant : Move
    {
        // trừu tượng chúa đó là Bắt Tốt qua đường !!!
        // cụ thể xin mời quý vị đọc https://vi.wikipedia.org/wiki/B%E1%BA%AFt_T%E1%BB%91t_qua_%C4%91%C6%B0%E1%BB%9Dng
        public override MoveType Type => MoveType.EnPassant;

        public override Position FromPos { get; }

        public override Position ToPos { get; }


        private readonly Position capturePos;
        public EnPassant(Position fromPos, Position toPos)
        {
            FromPos = fromPos;
            ToPos = toPos;
            capturePos = new Position(fromPos.Row, toPos.Column);
        }


        public override bool Execute(Board board)
        {
            new NormalMove(FromPos, ToPos).Execute(board);
            board[capturePos] = null;

            return true;
        }


    }
}
