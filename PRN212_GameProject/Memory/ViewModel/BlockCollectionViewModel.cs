using Memory.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Memory.ViewModel
{
    public class BlockCollectionViewModel : ObservableObject
    {
        public ObservableCollection<BlockViewModel> Blocks { get; set; }
        private BlockViewModel? SelectedBlock1;
        private BlockViewModel? SelectedBlock2;

        private readonly DispatcherTimer _peekTimer;
        private readonly DispatcherTimer _openingTimer;
        private readonly DispatcherTimer _visibleTimer;

        private const int _peekSeconds = 3;
        private const int _openSeconds = 5;

        //constructor
        public BlockCollectionViewModel()
        {
            _peekTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, _peekSeconds) };
            _peekTimer.Tick += PeekTimer_Tick!;

            _openingTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, _openSeconds) };
            _openingTimer.Tick += OpeningTimer_Tick!;

            _visibleTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, _peekSeconds) };
            _visibleTimer.Tick += VisibleTimer_Tick!;
        }

        public bool AreBlocksActive
        {
            get
            {
                if (SelectedBlock1 == null || SelectedBlock2 == null)
                    return true;
                return false;
            }
        }
        public bool AllBlockMatched
        {
            get
            {
                foreach (var block in Blocks)
                {
                    if (!block.IsMatched) return false;
                }
                return true;
            }
        }
        public bool CanSelect { get; private set; }
        private List<Block> GetModelsFrom(string relativePath) //Hàm khởi tạo block model
        {
            var models = new List<Block>();
            var images = Directory.GetFiles(@relativePath, "*.jpg", SearchOption.AllDirectories);
            var id = 0;
            foreach (var image in images)
            {
                models.Add(new Block() { Id = id, ImageSource = "/Memory;component/" + image });
                id++;
            }
            return models;
        }
        private void ShuffleBlocks()
        {
            var rng = new Random();
            for (int i = 0; i < 64; i++)
            {
                Blocks.Reverse();
                Blocks.Move(rng.Next(0, Blocks.Count), rng.Next(0, Blocks.Count));
            }
        }
        public void CreateBlocks(string imagePath)
        {
            Blocks = new ObservableCollection<BlockViewModel>();
            var models = GetModelsFrom(@imagePath);

            for (int i = 0; i < 6; i++)
            {
                var newBlock = new BlockViewModel(models[i]);
                var newBlockMatched = new BlockViewModel(models[i]);
                Blocks.Add(newBlock);
                Blocks.Add(newBlockMatched);
                newBlock.PeekAtImage();
                newBlockMatched.PeekAtImage();
            }
            ShuffleBlocks();
            OnPropertyChanged(nameof(Blocks));
        }
        public void SelectBlock(BlockViewModel block)
        {
            block.PeekAtImage();
            if (SelectedBlock1 == null) SelectedBlock1 = block;
            else SelectedBlock2 ??= block;

            //Đóng những block nào đang mở nhưng không match và ẩn những block nào đã match khi click vào block thứ 3
            foreach (var unSelectedBlock in Blocks)
                if (unSelectedBlock != SelectedBlock1 && unSelectedBlock != SelectedBlock2)
                {
                    if (unSelectedBlock.IsViewed) unSelectedBlock.ClosePeek(); //Đang mở nhưng không match
                    if (unSelectedBlock.IsMatched) unSelectedBlock.HideMatched();//đã match thì ẩn đi
                }
        }
        public void ClearSelected()
        {
            SelectedBlock1 = null;
            SelectedBlock2 = null;
            //CanSelect = false;
        }
        public void MatchFailed()
        {
            SelectedBlock1?.MarkFailed();
            SelectedBlock2?.MarkFailed();
            ClearSelected();
            if (_peekTimer.IsEnabled) _peekTimer.Stop(); //Nếu thời gian đã chạy thì restart
            _peekTimer.Start();//hide unmatched
        }
        public void MatchSuccess()
        {
            SelectedBlock1?.MarkMatched();
            SelectedBlock2?.MarkMatched();
            ClearSelected();
            if (_visibleTimer.IsEnabled) _visibleTimer.Stop();
            _visibleTimer.Start();
        }
        public bool CheckIfMatched()
        {
            if (SelectedBlock1?.Id == SelectedBlock2?.Id)
            {
                MatchSuccess();
                return true;
            }
            MatchFailed();
            return false;
        }
        public void RevealUnMatchedAndMatched()
        {
            foreach (var block in Blocks!)
            {
                if (!block.IsMatched)
                {
                    _peekTimer.Stop();
                    block.MarkFailed(); //Tô đỏ những ô còn lại chưa được match
                    block.PeekAtImage();
                }
                else
                {
                    _visibleTimer.Stop();
                    block.ShowMatched();
                }
            }
        }
        public void Memorize()
        {
            _openingTimer.Start();
        }
        private void OpeningTimer_Tick(object sender, EventArgs e)
        {
            foreach (var block in Blocks) block.ClosePeek();
            OnPropertyChanged(nameof(AreBlocksActive));
            _openingTimer.Stop();
        }
        private void PeekTimer_Tick(object sender, EventArgs e)
        {
            foreach (var block in Blocks)
                if (!block.IsMatched && block != SelectedBlock1)
                    block.ClosePeek();
            OnPropertyChanged(nameof(AreBlocksActive));
            _peekTimer.Stop();
        }
        private void VisibleTimer_Tick(object sender, EventArgs e)
        {
            foreach (var block in Blocks)
                if (block.IsMatched)
                    block.HideMatched();
            _visibleTimer.Stop();
        }
    }
}
