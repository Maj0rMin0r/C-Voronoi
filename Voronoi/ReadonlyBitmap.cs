using System.Drawing;

namespace Voronoi
{
    public static class ReadonlyBitmap
    {
        private static readonly object Locker = new object();
        private static Bitmap _snapshot;

        /// <summary>
        /// Gets the Bitmap from memory.
        /// </summary>
        /// <returns>read-only bitmap</returns>
        public static Bitmap Get()
        {
            lock (Locker)
            {
                return _snapshot == null ? null : new Bitmap(_snapshot);
            }
        }

        /// <summary>
        /// Locks a Bitmap into memory for read-only access. 
        /// </summary>
        /// <param name="source">Bitmap to set into memory</param>
        public static void Set(Bitmap source)
        {
            var copy = source == null ? null : new Bitmap(source);
            lock (Locker)
            {
                _snapshot?.Dispose();
                _snapshot = copy;
            }
        }
    }
}
