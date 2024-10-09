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
    public class GameState
    {
        //quản lý trạng thái hiện tại của trò chơi 

        // Bàn cờ (Board) hiện tại
        public Board Board { get; }

        // Người chơi hiện tại
        public Player CurrentPlayer { get; private set; }


        // Kết quả của trận đấu (nếu có)
        public Result Result { get; private set; } = null;


        // Số lần đi mà không có quân tốt di chuyển hoặc ăn quân

        private int noCaptureOrPawnMoves = 0;


        // Chuỗi trạng thái hiện tại của trò chơi

        private string stateString;



        // Lưu lịch sử của các trạng thái để kiểm tra điều kiện lặp lại ba lần
        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();


        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;



            // Tạo chuỗi trạng thái ban đầu và thêm vào lịch sử trạng thái
            stateString = new StateString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }



        // Lấy danh sách các nước đi hợp lệ cho một quân cờ tại vị trí pos


        //IEnumerable<T> hỗ trợ "lười tính toán", tức là các phần tử chỉ được tính toán khi bạn thực sự duyệt qua chúng. Điều này có thể tiết kiệm bộ nhớ và tài nguyên vì bạn chỉ thực sự lấy dữ liệu khi cần thiết.
        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Piece piece = Board[pos];
            IEnumerable<Move> moveCandidates = piece.GetMoves(pos, Board);
            return moveCandidates.Where(move => move.IsLegal(Board));

            // Phương thức này trả về một IEnumerable<Move>, nghĩa là một tập hợp các nước đi hợp lệ cho quân cờ tại vị trí pos.Bạn có thể duyệt qua tập hợp này để xem hoặc thao tác với các nước đi hợp lệ.
        }

        public void MakeMove(Move move)
        {

            Board.SetPawnSkipPosition(CurrentPlayer, null);
            bool captureOrPawn = move.Execute(Board);
            if (captureOrPawn)
            {
                noCaptureOrPawnMoves = 0;
                stateHistory.Clear();
            }
            else
            {
                noCaptureOrPawnMoves++;
            }

            CurrentPlayer = CurrentPlayer.Opponent();
            UpdateStateString();
            CheckForGameOver();
        }

        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Piece piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });

            return moveCandidates.Where(move => move.IsLegal(Board));


            // Tương tự, phương thức này trả về một IEnumerable<Move> chứa tất cả các nước đi hợp lệ của người chơi được chỉ định. Điều này cho phép bạn dễ dàng xử lý từng nước đi trong tập hợp này mà không cần biết chính xác loại dữ liệu chứa các nước đi này là gì.
        }


        private void CheckForGameOver()
        {
            if (!AllLegalMovesFor(CurrentPlayer).Any())  // Nếu không còn nước đi hợp lệ cho người chơi hiện tại
            {
                if (Board.IsInCheck(CurrentPlayer)) //// Nếu người chơi hiện tại đang bị chiếu tướng, đối thủ thắng
                {
                    Result = Result.Win(CurrentPlayer.Opponent());
                }

                else  // Nếu không, trận đấu hòa do hết nước đi (stalemate)
                {
                    Result = Result.Draw(EndReason.Stalemate);
                }
            }

            else if (Board.InSufficientMaterial()) //https://support.chess.com/en/articles/8705277-what-does-insufficient-mating-material-mean
            {
                Result = Result.Draw(EndReason.InSufficientMaterial);

                /*
                  
                   There are other combinations that will cause a draw that are not as obvious:
                   
                   If both sides have any one of the following, and there are no pawns on the board: 
                   
                   A lone king 
                   
                   a king and bishop
                   
                   a king and knight
                   
                   In the above scenarios the game will end in a draw, because it is not possible to force mate against a lone king with that material. You have a king and bishop your opponent has a king and bishop? It’s a draw! A king and bishop vs a king and a knight? Draw! And so on. 
                 */
            }

            else if (FiftyMoveRule())  // Cơ bản: Nếu trong 50 nước đi liên tiếp (của cả hai bên) không có ai bắt quân hoặc di chuyển quân tốt, thì một trong hai người chơi có thể yêu cầu hòa.
                                       // Mục đích: Quy tắc này tồn tại để ngăn chặn các ván cờ kéo dài vô tận khi không có tiến triển gì về mặt chiến thuật, đặc biệt khi cả hai người chơi chỉ di chuyển quân qua lại mà không tạo ra kết quả rõ ràng.
            {
                Result = Result.Draw(EndReason.FiftyMoveRule);
            }
            else if (ThreefoldRepetition())  // "Threefold Repetition" (Lặp lại ba lần) là một quy tắc trong cờ vua cho phép một ván cờ kết thúc với kết quả hòa nếu một trạng thái cụ thể của bàn cờ lặp lại ba lần trong quá trình chơi. Điều này xảy ra khi cùng một cấu hình bàn cờ xuất hiện ít nhất ba lần, không nhất thiết phải liên tiếp.
            {
                Result = Result.Draw(EndReason.ThreefoldRepetition);
            }

        }

        public bool IsGameOver()
        {
            return Result != null;
        }


        private bool FiftyMoveRule()
        {
            int fullMoves = noCaptureOrPawnMoves / 2;
            return fullMoves == 50;
        }

        private void UpdateStateString()
        {
            stateString = new StateString(CurrentPlayer, Board).ToString();
            if (!stateHistory.ContainsKey(stateString))
            {
                stateHistory[stateString] = 1;
            }
            else
            {
                stateHistory[stateString]++;
            }
        }

        private bool ThreefoldRepetition()
        {
            return stateHistory[stateString] == 3;
        }
    }
}
