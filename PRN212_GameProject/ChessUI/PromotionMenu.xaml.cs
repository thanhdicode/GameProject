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
    /// Interaction logic for PromotionMenu.xaml
    /// </summary>
    public partial class PromotionMenu : UserControl
    {

        // anh rất hài lòng với em, quyết định thăng chức ===> Chọn đi em muốn là gì của anh
        public event Action<PieceType> PieceSelected;

        public PromotionMenu(Player player)
        {
            InitializeComponent();

            // đen hoặc trắng, tùy vào thằng nào tới bên kia

            QueenImg.Source = Images.GetImage(player, PieceType.Queen);
            BishopImg.Source = Images.GetImage(player, PieceType.Bishop);
            RookImg.Source = Images.GetImage(player, PieceType.Rook);
            KnightImg.Source = Images.GetImage(player, PieceType.Knight);
        }

        private void QueenImg_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Queen); // làm hậu của anh nha （づ￣3￣）づ╭❤️～
        }

        private void BishopImg_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Bishop); // thôi làm tượng đi dù sao vẫn đỡ hơn
        }

        private void RookImg_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Rook); // quân xe cho em muốn đi đâu thì đi nha
        }

        private void KnightImg_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Knight); // em là ngựa đi cho anh cưỡi =)))) 👈(ﾟヮﾟ👈))
        }
    }
}
