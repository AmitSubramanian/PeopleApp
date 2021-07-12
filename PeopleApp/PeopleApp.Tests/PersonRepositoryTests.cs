using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Json;
using Xunit;
using PeopleApp.Infrastructure.FileContext;
using PeopleApp.Infrastructure.Locking;
using PeopleApp.Infrastructure.Repository;
using PeopleApp.Models;


namespace PeopleApp.Tests
{
    public class PersonRepositoryTests
    {
        private static IConfiguration config;
        private static string testAppDir;
        private static string peopleFileName;
        private static string jsonDataStorePath;
        private static string lockFileName;
        private static string peopleFilePathAndName;
        private static string lockFilePathAndName;
        private static PeopleFileContext context;
        private static LockFileFactory lockFileFactory;
        private static PersonRepository repository;

        static PersonRepositoryTests()
        {
            config = InitConfiguration();

            testAppDir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            
            peopleFileName = config["PeopleFileName"];
            jsonDataStorePath = config["JsonDataStorePath"];
            lockFileName = config["LockFileName"];

            peopleFilePathAndName = Path.Combine(testAppDir, jsonDataStorePath, peopleFileName);
            lockFilePathAndName = Path.Combine(testAppDir, jsonDataStorePath, lockFileName);

            context = new PeopleFileContext(peopleFilePathAndName);
            lockFileFactory = new LockFileFactory(lockFilePathAndName);
            repository = new PersonRepository(context, lockFileFactory);
        }

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            return config;
        }

        Person GetTestPerson()
        {
            return new Person
            {
                Name = "John Doe",
                Email = "john.doe@hotmail.com",
                IsActive = true
            };
        }

        [Fact]
        public async void GetAll_ReturnsAllPersons()
        {
            // Create a new file
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            // Add Persons to repository
            var res = await repository.Add(new Person { Name = "Ima Spender", Email = "ima.spender@gmail.com", IsActive = true });
            res = await repository.Add(new Person { Name = "Ima Saver", Email = "ima.saver@gmail.com", IsActive = true });
            res = await repository.Add(new Person { Name = "Isaac Newton", Email = "isaac.newton@gmail.com", IsActive = true });

            // Get Persons from repository
            var repository2 = new PersonRepository(context, lockFileFactory);
            var result = await repository2.GetAll();

            Assert.True(result != null);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async void Add_Get_ReturnsSamePerson()
        {
            // Create a new file
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            var person = GetTestPerson();

            // Add person to respository
            var res = await repository.Add(person);

            // Get person from repository
            var repository2 = new PersonRepository(context, lockFileFactory);
            var result = await repository2.Get(res.Id);

            Assert.Equal(res.Id, result.PersonId);
            Assert.Equal(person.Name, result.Name);
            Assert.Equal(person.Email, result.Email);
            Assert.Equal(person.IsActive, result.IsActive);
        }

        [Fact]
        public async void Get_ReturnsPersonDetails()
        {
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            // Add Persons to repository
            var person = new Person { Name = "Ima Spender", Email = "ima.spender@gmail.com", IsActive = true };
            var res = await repository.Add(person);
            var personId2 = await repository.Add(new Person { Name = "Isaac Newton", Email = "isaac.newton@gmail.com", IsActive = true });

            // Get Person from repository
            var repository2 = new PersonRepository(context, lockFileFactory);
            var result = await repository2.Get(res.Id);

            Assert.True(result != null);
            Assert.Equal(res.Id, result.PersonId);
            Assert.Equal(person.Name, result.Name);
            Assert.Equal(person.Email, result.Email);
            Assert.Equal(person.IsActive, result.IsActive);
        }

        [Fact]
        public async void Get_WhenPersonDoesNotExist_ReturnsNull()
        {
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            // Add Persons to repository
            var person = new Person { Name = "Ima Spender", Email = "ima.spender@gmail.com", IsActive = true };
            var res = await repository.Add(person);
            res = await repository.Add(new Person { Name = "Isaac Newton", Email = "isaac.newton@gmail.com", IsActive = true });

            // Get Person from repository using default Guid
            var repository2 = new PersonRepository(context, lockFileFactory);
            var result = await repository2.Get(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async void Update_ThenGet_ReturnsUpdatedPersonDetails()
        {
            // Create a new file
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            var person = GetTestPerson();
            
            // Add person to respository
            var res = await repository.Add(person);
            // Get back details to get LastUpdatedDttm
            var repository2 = new PersonRepository(context, lockFileFactory);
            var person2 = await repository2.Get(res.Id);

            // Update details using retrieved person2.LastUpdatedDttm
            var repository3 = new PersonRepository(context, lockFileFactory);
            var person3 = new Person { PersonId = res.Id, Name = "Ima Spender", Email = "ima.spender@gmail.com", IsActive = false, LastUpdatedDttm = person2.LastUpdatedDttm };
            var res3 = await repository2.Update(person3);

            // Get updated details
            var repository4 = new PersonRepository(context, lockFileFactory);
            var result = await repository4.Get(res.Id);

            Assert.Equal(res.Id, result.PersonId);
            Assert.Equal(person3.Name, result.Name);
            Assert.Equal(person3.Email, result.Email);
            Assert.Equal(person3.IsActive, result.IsActive);
        }

        [Fact]
        public async void Update_WhenPersonDoesNotExist_ReturnsError()
        {
            // Create a new file
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            var person = GetTestPerson();
            person.LastUpdatedDttm = DateTime.Now;

            // Add person to respository
            var res = await repository.Add(person);

            // Update details for PersonId that does not exist
            var repository2 = new PersonRepository(context, lockFileFactory);
            var person2 = new Person {PersonId = Guid.NewGuid() , Name = "Ima Spender", Email = "ima.spender@gmail.com", IsActive = false };
            person2.LastUpdatedDttm = person.LastUpdatedDttm;
            var res2 = await repository2.Update(person2);

            Assert.False(res2.Success);
        }

        [Fact]
        public async void Update_WhenLastUpdatedDttmHasChanged_ReturnsError()
        {
            // Create a new file
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            var person = GetTestPerson();
            person.LastUpdatedDttm = DateTime.Now;

            // Add person to respository
            var res = await repository.Add(person);
            // Get back details to get LastUpdatedDttm
            var repository2 = new PersonRepository(context, lockFileFactory);
            var person2 = await repository2.Get(res.Id);

            // Update details using retrieved person2.LastUpdatedDttm
            var repository3 = new PersonRepository(context, lockFileFactory);
            var person3 = new Person { PersonId = res.Id, Name = "Ima Spender", Email = "ima.spender@gmail.com", IsActive = false,
                                       LastUpdatedDttm = person2.LastUpdatedDttm };
            var res3 = await repository2.Update(person2);

            // Update details again using a LastUpdatedDttm = person2.LastUpdatedDttm - 2 seconds
            var repository4 = new PersonRepository(context, lockFileFactory);
            var person4 = new Person { PersonId = res.Id, Name = "My Name", Email = "my.name@gmail.com", IsActive = false,
                                       LastUpdatedDttm = person2.LastUpdatedDttm.AddSeconds(-2) };
            var res4 = await repository4.Update(person4);

            Assert.False(res4.Success);
        }

        [Fact]
        public async void Delete_ThenGet_ReturnsError()
        {
            // Create a new file
            await File.WriteAllTextAsync(peopleFilePathAndName, JsonSerializer.Serialize(new Person[0]));

            var person = GetTestPerson();

            // Add person to respository
            Result res = await repository.Add(person);

            // Delete person
            var repository2 = new PersonRepository(context, lockFileFactory);
            var res2 = await repository2.Delete(res.Id);

            // Get person from repository
            var repository3 = new PersonRepository(context, lockFileFactory);
            var result = await repository3.Get(res.Id);

            Assert.Null(result);
        }
    }
}
