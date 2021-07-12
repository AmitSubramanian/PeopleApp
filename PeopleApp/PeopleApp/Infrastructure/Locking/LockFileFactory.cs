using System.IO;

namespace PeopleApp.Infrastructure.Locking
{
    public class LockFileFactory : ILockFileFactory
    {
        private readonly string _filePathAndName;

        public LockFileFactory(string filePathAndName)
        {
            _filePathAndName = filePathAndName;
        }

        public LockFile GetLock()
        {
            // Open file with a Read Exclusive lock.
            FileStream fs = new FileStream(_filePathAndName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            return new LockFile(fs);
        }
    }
}
