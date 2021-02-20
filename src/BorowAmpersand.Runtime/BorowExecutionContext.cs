using System.Threading;

namespace BorowAmpersand.Runtime;

public static class BorowExecutionContext
{
    private static readonly AsyncLocal<BorowContext?> CurrentContext = new();

    public static BorowContext? Current => CurrentContext.Value;

    public static IDisposable Use(BorowContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var previous = CurrentContext.Value;
        CurrentContext.Value = context;
        return new PopScope(previous);
    }

    private sealed class PopScope : IDisposable
    {
        private readonly BorowContext? _previous;
        private bool _disposed;

        public PopScope(BorowContext? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            CurrentContext.Value = _previous;
            _disposed = true;
        }
    }
}
