using System.IO;

namespace PeopleApp.Infrastructure.Locking
{
    // This class uses a FileStream that acts as a locking mechanism and can be used mark 
    // "critical sections" in the Add/Update/Delete methods of the repository.
    public class LockFile
    {
        private readonly FileStream _fileStream;

        public LockFile(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        public void Unlock()
        {
            _fileStream.Close();
        }
    }
}
