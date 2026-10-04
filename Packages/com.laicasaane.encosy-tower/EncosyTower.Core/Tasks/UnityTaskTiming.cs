namespace EncosyTower.Tasks
{
    /// <summary>
    /// Selects the player-loop phase at which a scheduled <see cref="UnityTask"/> operation resumes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Yield</c>, <c>Delay</c>, <c>NextFrameAsync</c>, <c>WaitUntil</c> and <c>WaitWhile</c> share one Encosy
    /// player-loop scheduler.
    /// </para>
    /// <para>
    /// A request resumes at the next run of its phase. A request made while that phase is running resumes in the
    /// next frame. In the Editor outside Play Mode, every phase is run from <c>EditorApplication.update</c>.
    /// </para>
    /// </remarks>
    public enum UnityTaskTiming
    {
        /// <summary>
        /// Runs at the start of the <c>Initialization</c> phase of the player loop.
        /// </summary>
        Initialization,

        /// <summary>
        /// Runs at the end of the <c>Initialization</c> phase of the player loop.
        /// </summary>
        LastInitialization,

        /// <summary>
        /// Runs at the start of the <c>EarlyUpdate</c> phase of the player loop.
        /// </summary>
        EarlyUpdate,

        /// <summary>
        /// Runs at the end of the <c>EarlyUpdate</c> phase of the player loop.
        /// </summary>
        LastEarlyUpdate,

        /// <summary>
        /// Runs at the start of the <c>FixedUpdate</c> phase of the player loop.
        /// </summary>
        FixedUpdate,

        /// <summary>
        /// Runs at the end of the <c>FixedUpdate</c> phase of the player loop.
        /// </summary>
        LastFixedUpdate,

        /// <summary>
        /// Runs at the start of the <c>PreUpdate</c> phase of the player loop.
        /// </summary>
        PreUpdate,

        /// <summary>
        /// Runs at the end of the <c>PreUpdate</c> phase of the player loop.
        /// </summary>
        LastPreUpdate,

        /// <summary>
        /// Runs at the start of the <c>Update</c> phase of the player loop.
        /// </summary>
        Update,

        /// <summary>
        /// Runs at the end of the <c>Update</c> phase of the player loop.
        /// </summary>
        LastUpdate,

        /// <summary>
        /// Runs at the start of the <c>PreLateUpdate</c> phase of the player loop.
        /// </summary>
        PreLateUpdate,

        /// <summary>
        /// Runs at the end of the <c>PreLateUpdate</c> phase of the player loop.
        /// </summary>
        LastPreLateUpdate,

        /// <summary>
        /// Runs at the start of the <c>PostLateUpdate</c> phase of the player loop.
        /// </summary>
        PostLateUpdate,

        /// <summary>
        /// Runs at the end of the <c>PostLateUpdate</c> phase of the player loop.
        /// </summary>
        LastPostLateUpdate,

        /// <summary>
        /// Runs at the start of the <c>TimeUpdate</c> phase of the player loop.
        /// </summary>
        TimeUpdate,

        /// <summary>
        /// Runs at the end of the <c>TimeUpdate</c> phase of the player loop.
        /// </summary>
        LastTimeUpdate,
    }
}
