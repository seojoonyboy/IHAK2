
namespace DataModules {
    public class Coord {
        public int row;
        public int col;

        public Coord(int row, int col) {
            this.row = row;
            this.col = col;
        }

        public int GetRow() {
            return row;
        }

        public int GetColumn() {
            return col;
        }
    }
}