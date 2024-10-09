using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChessUI
{
    public static class ChessCursors
    {

        // Các con trỏ cho quân trắng và quân đen được tải từ các tệp .cur
        public static readonly Cursor WhiteCursor = LoadCursor("Assets/CursorW.cur");
        public static readonly Cursor BlackCursor = LoadCursor("Assets/CursorB.cur");

        /// <summary>
        /// Tải một con trỏ từ đường dẫn tệp đã chỉ định.
        /// </summary>
        /// <param name="filePath">Đường dẫn tương đối hoặc tuyệt đối đến tệp con trỏ.</param>
        /// <returns>Đối tượng Cursor mới đại diện cho con trỏ đã tải.</returns>
        /// <remarks>
        /// Hàm sử dụng <see cref="Application.GetResourceStream"/> để mở một luồng tới tệp đã chỉ định.
        /// Luồng sau đó được truyền vào constructor của <see cref="Cursor"/>, tạo một đối tượng Cursor mới.
        /// Tham số thứ hai của constructor Cursor được đặt là true, cho biết rằng con trỏ nên được giải phóng khi không còn sử dụng.
        /// </remarks>
        private static Cursor LoadCursor(string filePath)
        {

            // Mở luồng đến tệp con trỏ từ đường dẫn đã chỉ định
            Stream stream = Application.GetResourceStream(new Uri(filePath, UriKind.Relative)).Stream;
            // Tạo một đối tượng Cursor từ luồng và trả về
            return new Cursor(stream, true);
        }
    }
}
