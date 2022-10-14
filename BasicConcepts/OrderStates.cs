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
        Done = 1 << 1,
        /// <summary>
        /// Awaiting to get registered
        /// </summary>
        Registering = 1 << 2,
        /// <summary>
        /// Currently suspended by the exchange but expected to be repositioned back afterwards
        /// </summary>
        OnHold = 1 << 3,
        /// <summary>
        /// Change requested
        /// </summary>
        Changing = 1 << 4,
        /// <summary>
        /// Cancel requested
        /// </summary>
        Cancelling = 1 << 5,
        /// <summary>
        /// Successfully registered and still alive
        /// </summary>
        Active = 1 << 6
    }
}
