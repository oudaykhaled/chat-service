// See https://aka.ms/new-console-template for more information
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var configuration = LoadConfiguration();
string projectId = configuration["Firestore:ProjectId"];
string keyFilePath = configuration["Firestore:KeyFilePath"];
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);

FirestoreDb firestoreDb = FirestoreDb.Create(projectId);
Console.WriteLine($"Connected to Firestore for project: {projectId}");

// Set up real-time listener
string collectionName = "message";
Console.WriteLine($"Listening to changes in collection: {collectionName}");

CollectionReference collection = firestoreDb.Collection(collectionName);
collection.Listen(snapshot =>
{
    Console.WriteLine("Change detected!");

    foreach (DocumentChange change in snapshot.Changes)
    {
        switch (change.ChangeType)
        {
            case DocumentChange.Type.Added:
                Console.WriteLine($"New document added: {change.Document.Id}");
                Console.WriteLine($"Data: {JsonConvert.SerializeObject(change.Document.ToDictionary(), Formatting.Indented)}");
                break;

            case DocumentChange.Type.Modified:
                Console.WriteLine($"Document modified: {change.Document.Id}");
                Console.WriteLine($"Data: {JsonConvert.SerializeObject(change.Document.ToDictionary(), Formatting.Indented)}");
                break;

            case DocumentChange.Type.Removed:
                Console.WriteLine($"Document removed: {change.Document.Id}");
                break;
        }
    }
});

// Prevent the application from exiting
Console.WriteLine("Press Enter to exit the application...");
Console.ReadLine();


static IConfiguration LoadConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    return builder.Build();
}
