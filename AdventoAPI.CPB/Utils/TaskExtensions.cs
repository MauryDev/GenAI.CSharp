using System;
using System.Collections.Generic;
using System.Text;

namespace AdventoAPI.CPB.Utils;

internal static class TaskExtensions
{
    extension (Task _task)
    {
        public ValueTask AsValueTask() => new(_task);
    }
    extension<T>(Task<T> _task)
    {
        public ValueTask<T> AsValueTask() => new(_task);
    }
}
