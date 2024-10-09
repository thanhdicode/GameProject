using Memory.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Memory.View
{
    /// <summary>
    /// Interaction logic for StartMenuView.xaml
    /// </summary>
    public partial class StartMenuView : UserControl
    {
        public StartMenuView()
        {
            InitializeComponent();
        }
        private void Play_Clicked(object sender, RoutedEventArgs e)
        {
            if (categoryBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a category before starting the game.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var startMenu = DataContext as StartMenuViewModel;
            Debug.WriteLine(this.DataContext);
            startMenu.StartNewGame(categoryBox.SelectedIndex);
        }
    }
}
