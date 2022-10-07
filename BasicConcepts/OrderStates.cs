namespace BasicConcepts
{
    [Flags]
    public enum OrderStates : long
    {
        /// <summary>
        /// Not sent yet
        /// </summary>
        None = 0,
        /// <summary>
        /// Order was rejected for some reason
        /// </summary>
        Rejected = 1,
        /// <summary>
        /// Filled or cancelled by user
        /// </summary>
        Done = 2,
        /// <summary>
        /// Awaiting to get registered
        /// </summary>
        Registering = 4,
        /// <summary>
        /// Currently suspended by the exchange but expected to be repositioned back afterwards
        /// </summary>
        OnHold = 8,
        /// <summary>
        /// Change requested
        /// </summary>
        Changing = 16,
        /// <summary>
        /// Cancel requested
        /// </summary>
        Cancelling = 32,
        /// <summary>
        /// Successfully registered and still alive
        /// </summary>
        Active = 64
    }
}
