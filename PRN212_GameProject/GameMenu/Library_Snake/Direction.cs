using System;
using System.Collections.Generic;

namespace GameMenu.Library_Snake
{
    public class Direction
    {
        // Các hằng số chỉ hướng di chuyển của rắn
        public readonly static Direction Left = new Direction(0, -1); // Di chuyển sang trái
        public readonly static Direction Right = new Direction(0, 1); // Di chuyển sang phải
        public readonly static Direction Up = new Direction(-1, 0); // Di chuyển lên trên
        public readonly static Direction Down = new Direction(1, 0); // Di chuyển xuống dưới

        // Thuộc tính để lưu thông tin về sự thay đổi của hàng và cột khi di chuyển
        public int RowOffset { get; }
        public int ColOffset { get; }

        // Constructor của lớp Direction, thiết lập giá trị cho RowOffset và ColOffset
        private Direction(int rowOffset, int colOffset)
        {
            RowOffset = rowOffset;
            ColOffset = colOffset;
        }

        // Phương thức để lấy hướng đối diện với hướng hiện tại
        public Direction Opposite()
        {
            return new Direction(-RowOffset, -ColOffset);
        }

        // Ghi đè phương thức Equals để so sánh hai đối tượng Direction
        public override bool Equals(object obj)
        {
            return obj is Direction direction &&
                   RowOffset == direction.RowOffset &&
                   ColOffset == direction.ColOffset;
        }

        // Ghi đè phương thức GetHashCode để trả về mã băm của đối tượng Direction
        public override int GetHashCode()
        {
            return HashCode.Combine(RowOffset, ColOffset);
        }

        // Toán tử so sánh == để kiểm tra hai đối tượng Direction có bằng nhau hay không
        public static bool operator ==(Direction left, Direction right)
        {
            return EqualityComparer<Direction>.Default.Equals(left, right);
        }

        // Toán tử so sánh != để kiểm tra hai đối tượng Direction có khác nhau hay không
        public static bool operator !=(Direction left, Direction right)
        {
            return !(left == right);
        }
    }
}
