namespace EncosyTower.PageFlows
{
    /// <summary>What a flow does with a page it removes.</summary>
    public enum PageReturnOperation
    {
        /// <summary>Return the page to the pool of its flow, so it can be rented again.</summary>
        Pooling,

        /// <summary>Destroy the page.</summary>
        Destroy,
    }
}
