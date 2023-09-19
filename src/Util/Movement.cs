
using Microsoft.Xna.Framework;

namespace nx.util;
public class MovementUtility
{
    public static float lerp(float fgoal, float fcurrent, float dt)
    {
        // Calculate the interpolation factor based on time elapsed
        float t = MathHelper.Clamp(dt, 0f, 1f);

        // Use the interpolation factor to calculate the interpolated value
        return MathHelper.Lerp(fcurrent, fgoal, t);
    }

}