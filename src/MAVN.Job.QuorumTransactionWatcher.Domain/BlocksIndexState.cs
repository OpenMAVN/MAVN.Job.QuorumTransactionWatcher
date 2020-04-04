namespace MAVN.Job.QuorumTransactionWatcher.Domain
{
    /// <summary>
    /// The enum describes the state of local blocks index
    /// </summary>
    public enum BlocksIndexState
    {
        /// <summary>
        /// The index is out of date
        /// </summary>
        OutOfDate,
        
        /// <summary>
        /// The index is up to date
        /// </summary>
        UpToDate,
        
        /// <summary>
        /// The index is ahead of current state of blockchain
        /// </summary>
        Ahead
    }
}
