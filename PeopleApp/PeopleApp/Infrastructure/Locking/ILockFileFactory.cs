namespace PeopleApp.Infrastructure.Locking
{
    public interface ILockFileFactory
    {
        LockFile GetLock();
    }
}
