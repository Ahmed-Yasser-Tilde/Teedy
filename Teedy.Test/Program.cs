using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Teedy.ApiClient;
using Teedy.ApiClient.Models;
using Teedy.ApiClient.Models.Document;
using Teedy.ApiClient.Models.Tags;


#region Almasrya solve production issue
IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsetting.json")
            .Build();

TeedyApiMethods apiMethods = new TeedyApiMethods(configuration);

string? authToken = await TeedyApiMethods.Login("admin", "admin");
if(authToken == default)
{
    Console.WriteLine("Not Allow To login");
    return;
}

string almasryaTagId = await TeedyApiMethods.CreateTag(new CreateTag
{
    Name = "Almasrya",
    Color = "#008000"
}, authToken);

int limit = 10;
int offset = 0;
int totalDocuments = 0;

List<(string docId, string docTitle, string docDescription)> documentsToUpdate = new List<(string, string, string)>();

do
{
    GetAllDocumentsResponse getAllDocumentsResponse = await TeedyApiMethods.GetDocuments(authToken, limit, offset);
    totalDocuments = getAllDocumentsResponse.total;
    foreach (GetDocument document in getAllDocumentsResponse.documents)
    {
        if(document.title.StartsWith("رقم الحركة"))
        {
            documentsToUpdate.Add((document.id, document.title, document.description));
            if (document.tags != null && document.tags.Count > 0)
            {
                await TeedyApiMethods.UpdateDoc(authToken, document.id, document.title, [almasryaTagId]);
            }
        }
    }
    offset += limit;
} while (offset < totalDocuments);

List<Tag> teedyExistTags = await TeedyApiMethods.GetAllTags(authToken);
foreach(Tag tag in teedyExistTags)
{
    if (tag.Id != almasryaTagId)
    {
        await TeedyApiMethods.DeleteTag(authToken, tag.Id);
    }
}

foreach(var (docId, docTitle, docDescription) in documentsToUpdate)
{

    ReceiptJson receiptJson = JsonSerializer.Deserialize<ReceiptJson>(docDescription);
    // get from db receipt creation date (year/month/day), get branch, get cashboxs

    await TeedyApiMethods.UpdateDoc(authToken, docId, docTitle, []);
}

async Task<List<string>> HandleCreateTages(string authToken, List<string> tagsNamesFromAlmasryaForm)
{
    try
    {
        List<Tag> teedyExistTags = await TeedyApiMethods.GetAllTags(authToken);
        List<(string tagId, string parentId)> tagsId = new List<(string, string)>();

        #region Check Exists Almesrya Tags In Teedy
        Tag branchTag = teedyExistTags.FirstOrDefault(tag => tag.Name == tagsNamesFromAlmasryaForm[0]);
        if (branchTag == null)
        {
            // Branch tag does not exist, so all tages should be created from scratch and ignore all tags with same name
            for (int i = 0; i < tagsNamesFromAlmasryaForm.Count; i++)
            {
                string tagId = string.Empty;
                string parentId = string.Empty;
                tagsId.Add((tagId, parentId));
            }
        }
        else
        {
            tagsId.Add((branchTag.Id, string.Empty)); // Add branch tag with no parent
            string lastTagId = branchTag.Id;
            for (int i = 1; i < tagsNamesFromAlmasryaForm.Count; i++)
            {
                string tagId = string.Empty;
                string parentId = string.Empty;
                Tag tag = teedyExistTags.FirstOrDefault(t => t.Name == tagsNamesFromAlmasryaForm[i] && t.Parent == lastTagId);
                if (tag == null)
                {
                    // Tag does not exist, so create it and all incoming tages should be created from scratch and ignore all tags with same name 
                    for (int j = i; j < tagsNamesFromAlmasryaForm.Count; j++)
                    {
                        tagId = string.Empty;
                        parentId = string.Empty;
                        tagsId.Add((tagId, parentId));
                    }
                    break; // No need to continue checking for further tags
                }
                else
                {
                    tagId = tag.Id;
                    parentId = tag.Parent;
                    lastTagId = tagId; // Update lastTagId for next iteration
                }
                tagsId.Add((tagId, parentId));
            }
        }
        #endregion

        #region Handle Tags Tree
        for (int i = 0; i < tagsNamesFromAlmasryaForm.Count; i++)
        {
            if (tagsId[i].tagId != string.Empty)
            {
                if (i > 0 && tagsId[i].parentId != tagsId[i - 1].tagId)
                {
                    CreateTag tag = new CreateTag()
                    {
                        Name = tagsNamesFromAlmasryaForm[i],
                        Color = "#008000",
                        ParentId = i == 0 ? null : tagsId[i - 1].tagId
                    };
                    string Id = await TeedyApiMethods.CreateTag(tag, authToken);
                    tagsId[i] = (Id, tag.ParentId);
                }
                continue;
            }
            else
            {
                CreateTag createtag = new CreateTag()
                {
                    Name = tagsNamesFromAlmasryaForm[i],
                    Color = "#008000",
                    ParentId = i == 0 ? null : tagsId[i - 1].tagId
                };
                string tagId = await TeedyApiMethods.CreateTag(createtag, authToken);
                tagsId[i] = (tagId, createtag.ParentId);
            }
        }
        #endregion

        return tagsId.Select(x => x.tagId).ToList();
    }
    catch (Exception ex)
    {
       /* ClsGlobal.LogError(ex);
        ClsGlobal.LogError($"Error: {ex.Message}", "TeedyApiMethod");*/
        return null;
    }
}


#region test 
// Inject Json File
//var configuration = new ConfigurationBuilder()
//            .SetBasePath(Directory.GetCurrentDirectory())
//            .AddJsonFile("appsetting.json")
//            .Build();

// //Instantiate TeedyApiMethods with configuration
//var apiMethods = new TeedyApiMethods(configuration);

//// Get Token
//var authToken = await TeedyApiMethods.Login("admin", "admin");
//Console.WriteLine("Authentication Token : " + authToken);
//logger.LogInformation("Authentication Token: {AuthToken}", authToken);

//// File
//var fileID = await TeedyApiMethods.PutFile("C:\\Users\\user\\Desktop\\Caching.txt", authToken);
//Console.WriteLine("the File Added With ID : " + fileID);
//logger.LogInformation("Added File with ID: {FileID}", fileID);

//// Document
//var document = new Document
//{
//    Title = "Today",   // Required field
//    Language = "eng",            // Required field

//};
//var documentId = await TeedyApiMethods.AddDocument(document, authToken);
//Console.WriteLine($"Document ID : {documentId}");
//logger.LogInformation("Document ID: {DocumentId}", documentId);

//// Attach File To Document
//var status = await TeedyApiMethods.AttachFileToDoc(fileID, documentId, authToken);
//Console.WriteLine("Status : " + status);
//logger.LogInformation("Attach File to Document Status: {Status}", status);

//List<string> files = new List<string>()
//{
//    "C:\\Users\\user\\Desktop\\Response.txt",
//    "C:\\Users\\user\\Desktop\\Comments.txt",
//   "C:\\Users\\user\\Desktop\\Book.txt"
//};

//var attaches = await TeedyApiMethods.AddFilesToDocument(document, files, authToken);
//Console.WriteLine(attaches);

//#region Create Tag
//// Add Tag


//var createtag = new CreateTag()
//{
//    Name = "Ali",
//    Color = "#008000"
//};
//var addtag = await TeedyApiMethods.CreateTag(createtag, authToken);
//Console.WriteLine("the puttag : " + addtag);
//logger.LogInformation("Created Tag ID: {PutTag}", addtag);

//#endregion


//var getexisttag = new GetTag()
//{
//    ID = addtag
//};


//// GetTagById
//var gettag = await TeedyApiMethods.GetTagById(getexisttag.ID, authToken);
//Console.WriteLine($"Tag :\n TagId : {gettag.Id} , TagName : {gettag.Name} , TagColor : {gettag.Color}");
//logger.LogInformation("Fetched Tag: ID = {TagId}, Name = {TagName},Color = {TagColorName}", gettag.Id, gettag.Name,gettag.Color);

//var getTagbyname = new GetTag()
//{
//    ID = addtag,
//    Name=createtag.Name,
//    Color= createtag.Color
//};

//#region GetTagByName
//var tagbyname = await TeedyApiMethods.GetTagByName(getTagbyname.Name, authToken);

//// Logging to the console
//Console.WriteLine($"Tag : \n TagId :{tagbyname.Id} :: TagName : {tagbyname.Name}  :: TagColor : {tagbyname.Color}");

//// Logging with the logger
//logger.LogInformation("Fetched Tag: TagId: {TagId}, TagName: {TagName}, TagColor: {TagColor}",
//    tagbyname.Id, tagbyname.Name, tagbyname.Color);

//#endregion

//#region GetAllTags

//var tags = await TeedyApiMethods.GetAllTags(authToken);
//var counter = 0;
//foreach (var tag in tags)
//{
//    Console.WriteLine($"Tag {++counter} : {tag.Id} :: {tag.Name} :: {tag.Color}");
//    // Logging with the logger
//    logger.LogInformation("Fetched Tag {Counter}: TagId: {TagId}, TagName: {TagName}, TagColor: {TagColor}",
//        counter, tag.Id, tag.Name, tag.Color);
//}


/*
List<Tag> tags = await TeedyApiMethods.GetAllTags(authToken);

List<string> tagsNamesFromAlmasryaForm =
[
    "NameOfBranch", "Year", "Month", "Day", "etc"
];

List<string> tagsId = new List<string>();
for (int i = 0; i < tagsNamesFromAlmasryaForm.Count; i++)
{
    string tagId = string.Empty;
    for (int j = 0; j < tags.Count; j++)
    {
        if (tagsNamesFromAlmasryaForm[i] == tags[j].Name)
        {
            tagId = tags[j].Id;
            break;
        }
    }
    tagsId.Add(tagId);
}

for(int i = 0; i < tagsNamesFromAlmasryaForm.Count; i++)
{
    if(tagsId[i] != string.Empty)
    {
        continue;
    }

    CreateTag createtag = new CreateTag()
    {
        Name = tagsNamesFromAlmasryaForm[i],
        Color = "#008000",
        ParentId = i == 0 ? null : tagsId[i - 1]
    };
    string tagId = await TeedyApiMethods.CreateTag(createtag, authToken);
    tagsId[i] = tagId;

}

Document document = new Document
{
    Title = "Today",
    Language = "eng",
    Description = "Description",
    Tags = tagsId
};
string documentId = await TeedyApiMethods.AddDocument(document, authToken);
Console.WriteLine($"save {documentId} in receipt table in currex");
Console.WriteLine("add new row equal {rec_id}, {documentation_type}, {documentation_path}, {title}, {description}");
*/
#endregion

#endregion




























