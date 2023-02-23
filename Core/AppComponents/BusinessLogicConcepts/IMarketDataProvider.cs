using System;
using TradingConcepts;

namespace Core.AppComponents.BusinessLogicConcepts
{
    /// <summary>
    /// Used to retrieve all market specific data, such as securities, trade statistics, quotes etc
    /// </summary>
    public interface IMarketDataProvider
    {
        /// <summary>
        /// Subscribes to updates of certain kind for a security
        /// </summary>
        /// <typeparam name="T">Data kind to get updates of</typeparam>
        /// <param name="subject">A security that the updates are requested for</param>
        /// <param name="newDataHandler">A method that is invoked when there's an update for requested data kind and security</param>
        /// <param name="filter">A function that incapsulates logic to filter out incoming updates</param>
        void Subscribe<T>(ISecurity subject, Action<T> newDataHandler, Predicate<T> filter = null);
        /// <summary>
        /// Invoked to stop getting updates of certain kind for a security
        /// </summary>
        /// <typeparam name="T">Kind of data to stop getting updates for</typeparam>
        /// <param name="subject">A security whos updates are no longer needed</param>
        void Unsubscribe<T>(ISecurity subject);
    }
}