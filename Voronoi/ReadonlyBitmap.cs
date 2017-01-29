using System.Drawing;

namespace Voronoi
{
    public static class ReadonlyBitmap
    {
        private static readonly object Locker = new object();
        private static Bitmap _snapshot;

        public static Bitmap GetSnapshot(int width, int height)
        {
            lock (Locker)
            {
                return _snapshot == null ? null : new Bitmap(_snapshot, new Size(width, height));
            }
        }

        public static void SetSnapshot(Bitmap source, int width, int height)
        {
            var copy = new Bitmap(source, new Size(width, height));
            lock (Locker)
            {
                _snapshot?.Dispose();
                _snapshot = copy;
            }
        }
    }
}
