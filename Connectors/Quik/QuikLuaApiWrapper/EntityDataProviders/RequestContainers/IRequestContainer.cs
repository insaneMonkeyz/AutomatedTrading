namespace Quik.EntityDataProviders.RequestContainers
{
    internal interface IRequestContainer<T> where T : class
    {
        /// <summary>
        /// Checks if provided entity matches this request
        /// </summary>
        bool IsMatching(T? entity);
        bool HasData { get; }
    }
}
