using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PeopleApp.Infrastructure.FileContext;
using PeopleApp.Infrastructure.Locking;
using PeopleApp.Models;

namespace PeopleApp.Infrastructure.Repository
{
    public class PersonRepository : IPersonRepository
    {
        private readonly PeopleFileContext _peopleFileContext;
        private readonly ILockFileFactory _lockFileFactory;

        public PersonRepository(PeopleFileContext peopleFileContext, ILockFileFactory lockFileFactory)
        {
            _peopleFileContext = peopleFileContext;
            _lockFileFactory = lockFileFactory;
        }

        public async Task<List<Person>> GetAll()
        {
            return await _peopleFileContext.ReadAll();
        }

        public async Task<Person> Get(Guid personId)
        {
            var people = await _peopleFileContext.ReadAll();
            var person = people.Where(x => x.PersonId == personId).FirstOrDefault();
            return person;
        }

        public async Task<Result> Add(Person person)
        {
            Result result = null;

            LockFile lockFile = null;

            try
            {
                // Get an exclusive lock to prevent concurrent Add/Edit/Delete
                lockFile = _lockFileFactory.GetLock();

                // Get entire people list
                var people = await _peopleFileContext.ReadAll();

                // Append person to list
                person.PersonId = Guid.NewGuid();
                person.LastUpdatedDttm = DateTime.Now;
                people.Add(person);

                // Save entire people list
                _peopleFileContext.WriteAll(people);

                result = new Result { Id = person.PersonId, Success = true };
            }
            catch (Exception e)
            {
                result = new Result { Success = false, Error = e.Message };
            }
            finally
            {
                lockFile.Unlock();
            }

            return result;
        }

        public async Task<Result> Update(Person person)
        {
            Result result = null;

            LockFile lockFile = null;

            try
            {
                // Get an exclusive lock to prevent concurrent Add/Edit/Delete
                lockFile = _lockFileFactory.GetLock();

                // Get entire people list
                var people = await _peopleFileContext.ReadAll();

                // Find person in list
                int i = people.FindIndex(x => x.PersonId == person.PersonId);

                if (i == -1)
                {
                    result = new Result { Success = false, Error = $"Error: Person {person.Name} does not exist." };
                }
                else if ((people[i].LastUpdatedDttm - person.LastUpdatedDttm).TotalSeconds > 1)
                {
                    result = new Result { Success = false, Error = $"Error: Person {person.Name} cannot be saved because their record has been updated by someone else. Please retry the operation." };
                }
                else
                {
                    // Update person details
                    person.LastUpdatedDttm = DateTime.Now;
                    people[i] = person;
                     
                    // Save entire people list
                    _peopleFileContext.WriteAll(people);

                    result = new Result { Success = true };
                }
            }
            catch (Exception e)
            {
                result = new Result { Success = false, Error = e.Message };
            }
            finally
            {
                lockFile.Unlock();
            }

            return result;
        }

        public async Task<Result> Delete(Guid personId)
        {
            Result result = null;
            
            LockFile lockFile = null;

            try
            {
                // Get an exclusive lock to prevent concurrent Add/Edit/Delete
                lockFile = _lockFileFactory.GetLock();

                // Get entire people list
                List<Person> people = await _peopleFileContext.ReadAll();

                // Find person in list
                int i = people.FindIndex(x => x.PersonId == personId);

                if (i == -1)
                {
                    result = new Result { Success = false, Error = $"Error: Person does not exist." };
                }
                else
                {
                    // Delete person from list
                    people.RemoveAt(i);

                    // Save entire people list
                    _peopleFileContext.WriteAll(people);

                    result = new Result { Success = true };
                }
            }
            catch (Exception e)
            {
                result = new Result { Success = false, Error = e.Message };
            }
            finally
            {
                lockFile.Unlock();
            }

            return result;
        }
    }
}
