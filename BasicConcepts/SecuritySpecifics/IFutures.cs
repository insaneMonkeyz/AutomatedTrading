namespace BasicConcepts.SecuritySpecifics
{
    public interface IFutures : ISecurity, IExpiring
    {
        ISecurity Underlying { get; }
    }
}
