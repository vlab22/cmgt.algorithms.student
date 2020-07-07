using System.Drawing;
using GXPEngine.Core;

namespace GXPEngine
{
    public static class PointTools
    {
        public static float PointDistance(Point pEnd, Point pStart)
        {
            float x = (pEnd.X - pStart.X);
            float y = pEnd.Y - pStart.Y;

            return Mathf.Sqrt(x * x + y * y);
        }
    }
}