namespace Assets.Scripts.Map
{
    public class CubeCoord
	{
		public int X { get; private set; }
		public int Y { get; private set; }
		public int Z { get; private set; }

		public static CubeCoord Create(int x, int y, int z)
		{
			if(x + y + z == 0)
				return new CubeCoord(x, y, z);
			return null;
		}

		private CubeCoord (int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}

