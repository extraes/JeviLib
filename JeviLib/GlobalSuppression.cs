using System.Diagnostics.CodeAnalysis;

#if DEBUG
[assembly: SuppressMessage("Resolve nullable warnings", "CS8600:Converting null literal or possible null value to non-nullable type.", Justification = "Null checks in dbg builds should suffice")]
#endif
