namespace ChessLogic.Moves
{
    public class DoublePawn : Move
    {
        //https://vi.wikipedia.org/wiki/Khai_cu%E1%BB%99c_m%E1%BB%9F_(c%E1%BB%9D_vua)
        public override MoveType Type => MoveType.DoublePawn;
        public override Position FromPos { get; }

        public override Position ToPos { get; }


        private readonly Position skippedPos;
        public DoublePawn(Position fromPos, Position toPos)
        {
            FromPos = fromPos;
            ToPos = toPos;
            skippedPos = new Position((fromPos.Row + toPos.Row) / 2, fromPos.Column);
        }

        public override bool Execute(Board board)
        {
            Player player = board[FromPos].Color;
            board.SetPawnSkipPosition(player, skippedPos);

            new NormalMove(FromPos, ToPos).Execute(board);

            return true;
        }
    }
}
