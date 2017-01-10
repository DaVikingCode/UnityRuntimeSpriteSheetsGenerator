namespace DaVikingCode.RectanglePacking {

	public class IntegerRectangle {

		public int x;
		public int y;
		public int width;
		public int height;
		public int right;
		public int bottom;
		public int id;

		public IntegerRectangle(int x = 0, int y = 0, int width = 0, int height = 0) {

			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.right = x + width;
			this.bottom = y + height;
		}
	}
}
