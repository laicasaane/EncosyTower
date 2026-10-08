namespace EncosyTower.Tasks
{
    /// <summary>
    /// Selects the clock that a <see cref="UnityTask"/> delay measures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The delay is checked on each tick of the Encosy player-loop scheduler at the requested
    /// <see cref="UnityTaskTiming"/>. In the Editor outside Play Mode, delays use real time.
    /// </para>
    /// <para>
    /// <b>Counterparts:</b> UniTask: <c>Cysharp.Threading.Tasks.DelayType</c>; Unity: none.
    /// </para>
    /// </remarks>
    public enum UnityTaskDelayType
    {
        /// <summary>
        /// Subtracts <c>Time.deltaTime</c> from the remaining delay on each tick, so the delay follows
        /// <c>Time.timeScale</c>.
        /// </summary>
        DeltaTime,

        /// <summary>
        /// Subtracts <c>Time.unscaledDeltaTime</c> from the remaining delay on each tick, so the delay ignores
        /// <c>Time.timeScale</c>.
        /// </summary>
        UnscaledDeltaTime,

        /// <summary>
        /// Compares <c>Time.realtimeSinceStartupAsDouble</c> with the time at which the delay started.
        /// </summary>
        Realtime,
    }
}
