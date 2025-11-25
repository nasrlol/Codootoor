using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

namespace Odootoor;

public partial class Program
{
    public static bool DEBUGDrawBoundingBox = false;

    public static bool StickmanOver(Vector2 pos, Rectangle bounds)
    {
        bool result = false;

        float dX;
        if (stickmanFacing < 0)
        {
            dX = +36;
        }
        else
        {
            dX = -36 * 2;
        }

        var handHitbox = new Rectangle(pos.X + dX, pos.Y - 8, 80, 20);
        if (DEBUGDrawBoundingBox)
        {
            DrawRectangleRec(handHitbox, Color.Pink);
        }
        result = CheckCollisionRecs(handHitbox, bounds);

        return result;
    }

}

