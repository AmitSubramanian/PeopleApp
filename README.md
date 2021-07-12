# Introduction 
A simple CRUD application to maintain a Person list stored as a JSON file.

# Getting Started
1.	Installation process
- Pull the repository down to your local machine, open the .sln file in Visual Studio 2019, and hit Ctrl-F5 to run.
2.	Software dependencies
- .NET Core 3.1

# Design Notes
- The application contains the following layers: View, Repository, and FileContext (which reads the JSON file).
- The JSON file is stored at: wwwroot\People.json
- Views:
  - The opening page displays the list of people sorted by Name, which links to the remaining views.
  - 'Add' view: adds a person.
  - 'Edit': edits person details.
  - 'Delete': asks for delete confirmation, and deletes a person. 
- Repository:
  - To keep the solution simple, the Repository methods write directly to the JSON file, and not through an intermediate in-memory representation.
  - Add, Update, and Delete rewrite all records to the JSON file.
- The solution is not scalable to a large number of records, as the application will take longer to respond.
- Locking:
  - The file wwwroot\_LockFile.txt is used to avoid concurrent writes.  If the user initiates an Add/Edit/Delete operation through a browser window, all sessions in other browser windows will not be able to Add/Edit/Delete to the file until the first browser window has completed its operation.

#Areas for Improvement
- Performance Note:  Stored one Person JSON record per line (instead of a JSON array of Person records).  This will allow an Add operation to simply append a new record to the file, instead of rewriting all records.  However, in that case, the file records would not be stored in a proper JSON format.
- If the solution is extended to have another record type (e.g. a Role record, where a User may have multiple Roles), and transactions are required, a UnitOfWork class can be written.
- For more complex functionality, a DbContext Provider for JSON data store can be written (and hosted as.a NuGet package). This will ensure that the JSON data store can be used in a way similar to database providers.
- The solution contains Unit Test Cases for the Repository.  Unit Test Cases can also be added for the Controller.
