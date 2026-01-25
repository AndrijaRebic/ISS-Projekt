using UnityEngine;

public static class InputBlocker
{
    private static int blockFireUntilFrame = -1;
    public static void BlockFireForOneFrame()
    {
        blockFireUntilFrame = Time.frameCount;
    }

    public static bool IsFireBlocked()
    {
        return Time.frameCount == blockFireUntilFrame;
    }
}