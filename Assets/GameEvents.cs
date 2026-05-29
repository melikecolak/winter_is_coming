using System;

/// <summary>
/// Central event hub for game-wide signals.
/// Add new events here as static members; keep raisers internal so only
/// the owning system can fire them.
/// </summary>
public static class GameEvents
{
    // Fired the instant the local player's health reaches zero.
    //
    // TODO (multiplayer): change the signature to Action<GameObject> (or Action<int> playerId)
    // so subscribers receive which player died. Enemy.OnPlayerDied() already has a
    // FindNextTarget() hook — wire it up when multi-player targeting is needed.
    public static event Action OnPlayerDied;

    internal static void RaisePlayerDied() => OnPlayerDied?.Invoke();

    // Fired when the last living enemy dies — triggers Victory screen.
    public static event Action OnAllEnemiesDied;

    internal static void RaiseAllEnemiesDied() => OnAllEnemiesDied?.Invoke();
}
