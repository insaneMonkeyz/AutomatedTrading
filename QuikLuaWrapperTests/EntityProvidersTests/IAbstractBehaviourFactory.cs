namespace QuikLuaWrapperTests.EntityProvidersTests
{
    internal interface IAbstractBehaviourFactory<T>
    {
        T CreateSuccessfulOrderSubmissionBehaviour();
    }
}