using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SilentMessageBus : IMessageBus
{
    public readonly List<IMessageSinkMessage> Messages = new();

    public bool QueueMessage(IMessageSinkMessage message)
    {
        Messages.Add(message);
        return true; // Allow test to proceed
    }

    public void Dispose() { }
}