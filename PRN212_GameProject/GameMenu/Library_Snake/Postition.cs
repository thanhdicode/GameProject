using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMenu.Library_Snake
{
    public class Postition
    {
        public int Row { get; }
        public int Col { get; }

        public Postition(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public Postition Translate(Direction dir)
        {
            return new Postition(Row + dir.RowOffset, Col + dir.ColOffset);
        }

        public override bool Equals(object obj)
        {
            return obj is Postition postition &&
                   Row == postition.Row &&
                   Col == postition.Col;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }

        public static bool operator ==(Postition left, Postition right)
        {
            return EqualityComparer<Postition>.Default.Equals(left, right);
        }

        public static bool operator !=(Postition left, Postition right)
        {
            return !(left == right);
        }
    }
}
