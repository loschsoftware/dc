using System.Collections.Generic;
using System.Linq;

namespace Dassie.Tests.Internal;

internal static class TestHelpers
{
    public static Status GetStatus(IEnumerable<Status> childStates)
    {
        if (childStates.All(c => c == Status.Todo))
            return Status.Todo;

        if (childStates.Any(c => c == Status.InProgress))
            return Status.InProgress;

        if (childStates.All(c => c == Status.Passed))
            return Status.Passed;

        if (childStates.Any(c => c == Status.Failed))
            return Status.Failed;

        return Status.Todo;
    }
}