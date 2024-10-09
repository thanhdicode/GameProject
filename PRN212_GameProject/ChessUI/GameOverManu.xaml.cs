using ChessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for GameOverManu.xaml
    /// </summary>
    public partial class GameOverManu : UserControl
    {
        public event Action<Option> OptionSelected;



        public GameOverManu(GameState gameState)
        {
            InitializeComponent();
            Result result = gameState.Result;
            WinnerText.Text = GetWinnerText(result.Winner);
            ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer);
        }

        private static string GetWinnerText(Player winner)
        {
            return winner switch
            {
                Player.White => "WHITE WINS",
                Player.Black => "BLACK WINs",
                _ => "None",
            };

        }

        private static string PlayerString(Player player)
        {
            return player switch
            {
                Player.White => "WHITE",
                Player.Black => "BLACK",
                _ => ""
            };
        }

        private static string GetReasonText(EndReason reason, Player currentPlayer)
        {
            return reason switch
            {
                EndReason.Stalemate => $"STATEMATE - {PlayerString(currentPlayer)} CAN'T MOVE",// tạm thời hòa nhé, quân tử trả thù 10 năm chưa muộn
                EndReason.Checkmate => $"CHECKMATE - {PlayerString(currentPlayer)} CAN'T MOVE",//m khỏi chạy chiếu tương rồi con trai
                EndReason.FiftyMoveRule => "FIFTY-MOVE RULE",
                EndReason.InSufficientMaterial => $"InSufficient Material",
                EndReason.ThreefoldRepetition => "ThreefoldRepetition",
                _ => ""
            };
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart); // Lai mày, nãy do chủ quan thôi quất tiếp hiệp nữa
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit); // dẹp nghỉ đi nay không có tâm trạng 
        }
    }
}
