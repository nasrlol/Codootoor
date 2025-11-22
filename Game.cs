using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

namespace Odootoor;

public partial class Program
{
    public static bool DEBUGDrawBoundingBox = true;

    public static bool StickmanOver(Vector2 pos, Rectangle bounds)
    {
        bool result = false;

        var handHitbox = new Rectangle(pos.X - 36 * stickmanFacing, pos.Y - 8, 20, 20);
        result = CheckCollisionRecs(handHitbox, bounds);

        return result;
    }

}

