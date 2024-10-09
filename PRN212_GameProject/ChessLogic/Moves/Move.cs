namespace ChessLogic.Moves
{
    public abstract class Move
    {
        public abstract MoveType Type { get; }
        public abstract Position FromPos { get; }
        public abstract Position ToPos { get; }
        public abstract bool Execute(Board board);

        public virtual bool IsLegal(Board board)
        {
            Player player = board[FromPos].Color;

            // Tạo bản sao của bàn cờ hiện tại để kiểm tra nước đi mà không thay đổi trạng thái của bàn cờ thực tế
            Board boardCopy = board.Copy();
            Execute(boardCopy);
            return !boardCopy.IsInCheck(player);
        }




    }
}
/*
  ✍️(◔◡◔))

 Trong C#, từ khóa virtual được sử dụng để cho phép một phương thức, thuộc tính, chỉ mục, hoặc sự kiện có thể bị ghi đè (override) trong các lớp con. Điều này là một phần của cơ chế kế thừa và đa hình (polymorphism) trong lập trình hướng đối tượng. 

Giải thích về virtual:
Khái niệm:

virtual là từ khóa được đặt trước khai báo của phương thức, thuộc tính, chỉ mục, hoặc sự kiện trong lớp cơ sở. Nó cho phép lớp con ghi đè hoặc thay đổi cách hoạt động của thành viên đó.
Tại sao cần virtual:

Tính linh hoạt: virtual cung cấp khả năng linh hoạt cho các lớp kế thừa để thay đổi hoặc mở rộng hành vi của các phương thức trong lớp cơ sở mà không làm thay đổi lớp cơ sở.
Đa hình: Đây là một trong những đặc tính cơ bản của lập trình hướng đối tượng, cho phép các đối tượng của các lớp khác nhau thực thi các phương thức có cùng tên nhưng với hành vi khác nhau.
Cách hoạt động:

Lớp cơ sở: Khi bạn khai báo một phương thức hoặc thuộc tính là virtual trong lớp cơ sở, bạn đang cho phép các lớp con kế thừa từ lớp cơ sở ghi đè phương thức đó.
Lớp con: Trong lớp con, bạn có thể sử dụng từ khóa override để thay thế hoặc mở rộng hành vi của phương thức virtual từ lớp cơ sở.
 */