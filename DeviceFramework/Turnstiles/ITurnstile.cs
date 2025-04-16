using System;
using TIM.Devices.Framework.Common;

namespace TIM.Devices.Framework.Turnstiles
{
    public interface ITurnstile : IDevice
    {
        event EventHandler<TurnstileEventArgs> Passed;

        bool SupportsPassed { get; }
    }
}