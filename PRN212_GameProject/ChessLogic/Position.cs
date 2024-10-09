namespace ChessLogic
{
    public class Position
    {
        // Đại diện cho một vị trí trên bàn cờ.
        public int Row { get; }
        public int Column { get; }

        // Constructor để khởi tạo hàng và cột của vị trí.
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        // Xác định màu của ô vuông tại vị trí này.
        // Giả định bàn cờ tiêu chuẩn, nơi (0,0) là ô đen.
        public Player SquareColor()
        {
            // Nếu tổng của chỉ số hàng và cột là số chẵn, ô là màu trắng.
            // Ngược lại, ô là màu đen.
            if ((Row + Column) % 2 == 0)
            {
                return Player.White;
            }
            return Player.Black;
        }

        // Ghi đè phương thức Equals để so sánh các vị trí theo giá trị hàng và cột.
        public override bool Equals(object obj)
        {
            // Kiểm tra nếu đối tượng là Position và so sánh hàng và cột.
            return obj is Position position &&
                   Row == position.Row &&
                   Column == position.Column;
        }

        // Ghi đè phương thức GetHashCode để đảm bảo tính nhất quán với Equals.
        public override int GetHashCode()
        {
            // Kết hợp giá trị hàng và cột để tạo ra một mã hash duy nhất.
            return HashCode.Combine(Row, Column);
        }

        // Định nghĩa toán tử so sánh bằng để so sánh hai đối tượng Position.
        public static bool operator ==(Position left, Position right)
        {
            // Sử dụng bộ so sánh mặc định cho Position.
            return EqualityComparer<Position>.Default.Equals(left, right);
        }

        // Định nghĩa toán tử so sánh khác để so sánh hai đối tượng Position.
        public static bool operator !=(Position left, Position right)
        {
            // Phủ định kết quả của toán tử so sánh bằng.
            return !(left == right);
        }

        // Định nghĩa toán tử cộng để di chuyển vị trí theo một hướng.
        public static Position operator +(Position pos, Direction dir)
        {
            // Tạo một vị trí mới với hàng và cột được cập nhật dựa trên các delta hướng.
            return new Position(pos.Row + dir.RowDelta, pos.Column + dir.ColumnDelta);
        }
    }
}
