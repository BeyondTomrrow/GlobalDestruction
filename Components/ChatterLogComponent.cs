using System.Collections.Generic;
using WorldNMilSim.Core;

namespace WorldNMilSim.Components;

public class ChatterMessage
{
    public required string Text;
    public double RemainingSeconds;
}

public class ChatterLogComponent
{
    public List<ChatterMessage> Messages = new();
    public int TotalPosted; // monotonic counter, used to detect "a new message landed" even as old ones expire
}

public static class ChatterLog
{
    private const double MessageLifetimeSeconds = 12;
    private const int MaxMessages = 8;

    public static void Post(World world, Entity chatterEntity, string text)
    {
        var log = world.Get<ChatterLogComponent>(chatterEntity);
        if (log == null) return;

        log.Messages.Add(new ChatterMessage { Text = text, RemainingSeconds = MessageLifetimeSeconds });
        log.TotalPosted++;
        while (log.Messages.Count > MaxMessages)
            log.Messages.RemoveAt(0);
    }
}