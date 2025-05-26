using Microsoft.Extensions.Configuration;
using Teedy.ApiClient;
using Teedy.ApiClient.Models.Document;
using Teedy.ApiClient.Models.Tags;
using Microsoft.Extensions.Logging;
using Teedy.ApiClient.Models.Tag;


// Setup Logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

ILogger logger = loggerFactory.CreateLogger("Teedy.Test");

// Inject Json File
var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsetting.json")
            .Build();

 //Instantiate TeedyApiMethods with configuration
var apiMethods = new TeedyApiMethods(configuration);

// Get Token
var authToken = await TeedyApiMethods.Login("admin", "admin");
Console.WriteLine("Authentication Token : " + authToken);
logger.LogInformation("Authentication Token: {AuthToken}", authToken);

// File
var fileID = await TeedyApiMethods.PutFile("C:\\Users\\user\\Desktop\\Caching.txt", authToken);
Console.WriteLine("the File Added With ID : " + fileID);
logger.LogInformation("Added File with ID: {FileID}", fileID);

// Document
var document = new Document
{
    Title = "Today",   // Required field
    Language = "eng",            // Required field
    
};
var documentId = await TeedyApiMethods.AddDocument(document, authToken);
Console.WriteLine($"Document ID : {documentId}");
logger.LogInformation("Document ID: {DocumentId}", documentId);

// Attach File To Document
var status = await TeedyApiMethods.AttachFileToDoc(fileID, documentId, authToken);
Console.WriteLine("Status : " + status);
logger.LogInformation("Attach File to Document Status: {Status}", status);

List<string> files = new List<string>()
{
    "C:\\Users\\user\\Desktop\\Response.txt",
    "C:\\Users\\user\\Desktop\\Comments.txt",
   "C:\\Users\\user\\Desktop\\Book.txt"
};

var attaches = await TeedyApiMethods.AddFilesToDocument(document, files, authToken);
Console.WriteLine(attaches);

#region Create Tag
// Add Tag


var createtag = new CreateTag()
{
    Name = "Ali",
    Color = "#008000"
};
var addtag = await TeedyApiMethods.CreateTag(createtag, authToken);
Console.WriteLine("the puttag : " + addtag);
logger.LogInformation("Created Tag ID: {PutTag}", addtag);

#endregion


var getexisttag = new GetTag()
{
    ID = addtag
};


// GetTagById
var gettag = await TeedyApiMethods.GetTagById(getexisttag.ID, authToken);
Console.WriteLine($"Tag :\n TagId : {gettag.Id} , TagName : {gettag.Name} , TagColor : {gettag.Color}");
logger.LogInformation("Fetched Tag: ID = {TagId}, Name = {TagName},Color = {TagColorName}", gettag.Id, gettag.Name,gettag.Color);

var getTagbyname = new GetTag()
{
    ID = addtag,
    Name=createtag.Name,
    Color= createtag.Color
};

#region GetTagByName
var tagbyname = await TeedyApiMethods.GetTagByName(getTagbyname.Name, authToken);

// Logging to the console
Console.WriteLine($"Tag : \n TagId :{tagbyname.Id} :: TagName : {tagbyname.Name}  :: TagColor : {tagbyname.Color}");

// Logging with the logger
logger.LogInformation("Fetched Tag: TagId: {TagId}, TagName: {TagName}, TagColor: {TagColor}",
    tagbyname.Id, tagbyname.Name, tagbyname.Color);

#endregion

#region GetAllTags

var tags = await TeedyApiMethods.GetAllTags(authToken);
var counter = 0;
foreach (var tag in tags)
{
    Console.WriteLine($"Tag {++counter} : {tag.Id} :: {tag.Name} :: {tag.Color}");
    // Logging with the logger
    logger.LogInformation("Fetched Tag {Counter}: TagId: {TagId}, TagName: {TagName}, TagColor: {TagColor}",
        counter, tag.Id, tag.Name, tag.Color);
}

#endregion




























