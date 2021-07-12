using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PeopleApp.Models;

namespace PeopleApp.Infrastructure.FileContext
{
    public class PeopleFileContext
    {
        private readonly string _filePathAndName;

        public PeopleFileContext(string filePathAndName)
        {
            _filePathAndName = filePathAndName;
        }

        public async Task<List<Person>> ReadAll()
        {
            Person[] people = null;

            using (FileStream fs = File.OpenRead(_filePathAndName))
            {
                people = await JsonSerializer.DeserializeAsync<Person[]>(fs);
            }

            return (people != null) ? people.ToList() : new List<Person>();
        }

        public async void WriteAll(List<Person> people)
        {
            await File.WriteAllTextAsync(_filePathAndName, JsonSerializer.Serialize(people.ToArray()));
        }
    }
}
