using Assets.Contracts.Map;

namespace Assets.Scripts.Map
{
    public class Sector
    {
        public Sector(int top, int right, int bottom, int left)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;

            Width = Right - Left;
            Height = Top - Bottom;


            Center = new Position
            {
                X = Width / 2 + left,
                Y = Height / 2 + bottom
            };
        }

        public Position Center { get; private set; }
        public int Top { get; private set; }
        public int Right { get; private set; }
        public int Bottom { get; private set; }
        public int Left { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    }
}
