using System;

namespace Assets.Scripts.Map
{
    [Serializable]
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position ()
		{			
		}

		public Position(int x, int y)
		{
			X = x;
			Y = y;
		}

        public override bool Equals(object obj)
        {
            var position = obj as Position;
            if (position == null)
                return false;
            return X == position.X && Y == position.Y;
        }

        public override int GetHashCode() => base.GetHashCode();        

        public static Position operator +(Position p1, Position p2)
        {
            var x = p1.X + p2.X;
            var y = p1.Y + p2.Y;
            return new Position(x, y);
        }

        public static Position operator -(Position p1, Position p2)
        {
            var x = p1.X - p2.X;
            var y = p1.Y - p2.Y;
            return new Position(x, y);
        }

        public override string ToString() => $"Position X: {X}, Y: {Y}";
    }
}
