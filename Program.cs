using Azure.AI.OpenAI;
using Azure;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography.X509Certificates;
using Humanizer;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Text;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

string proxyUrl = "https://aoai.hacktogether.net";
string aoaiEndpoint = new(proxyUrl + "/v1/api"); ;
string aoaiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
string aoaiModel = "gpt-3.5-turbo";

//Initialize the kernel
var kBuilder = new KernelBuilder();

kBuilder.WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())) // this is to show whats going on the console
    .WithAzureOpenAIChatCompletionService(aoaiModel, aoaiEndpoint, aoaiApiKey);
    
var kernel = kBuilder.Build();


// Register helpful functions with the kernel 

kernel.RegisterCustomFunction(SKFunction.Create(
    () => $"The current date and time are {DateTime.UtcNow:r}",
    "DateTime", "Now", "Gets the current date and time."));


//Ensure we have embeddings for our document
ISemanticTextMemory memory = new MemoryBuilder()
        .WithMemoryStore(new QdrantMemoryStore("http://localhost:6333/", 1536))
        .WithLoggerFactory(kernel.LoggerFactory)
        .WithAzureOpenAITextEmbeddingGenerationService("TextEmbeddingAda002_1", aoaiEndpoint, aoaiApiKey)
        .Build();

IList<string> collections = await memory.GetCollectionsAsync();

string collectionName = "canColVeHai";


if (collections.Contains("canColVeHai"))
{
    Console.WriteLine("Found Database");
}
else
{
    //Parse PDF files and initialize SK memory
    var dataFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "data");
    var pdfFiles = Directory.GetFiles(dataFolderPath, "*.pdf");

    Console.WriteLine($"The path of the directory is {pdfFiles.ToString()}");

    foreach (var pdfFileName in pdfFiles)
    {
        using var pdfDocument = UglyToad.PdfPig.PdfDocument.Open(pdfFileName);
        foreach (var pdfPage in pdfDocument.GetPages())
        {
            var pageText = ContentOrderTextExtractor.GetText(pdfPage);
            var paragraphs = new List<string>();

            //We need to break long content to smaller pieces
            if (pageText.Length > 2048)
            {
                var lines = TextChunker.SplitPlainTextLines(pageText, 128);
                paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 1024);
            }
            else
            {
                paragraphs.Add(pageText);
            }

            foreach (var paragraph in paragraphs)
            {
                var id = pdfFileName + pdfPage.Number + paragraphs.IndexOf(paragraph);
                await memory.SaveInformationAsync(collectionName, paragraph, id);
            }
            Console.WriteLine("Generated Database");

        }
    }
}

//End of Parse PDF files and initialize SK memory


// Create a new chat 
IChatCompletion ai = kernel.GetService<IChatCompletion>();
ChatHistory chat = ai.CreateNewChat(
    "You are an AI assistant that helps people find information.  Use only the data provided in the sources ");
StringBuilder builder = new();


//This is the minimal API

var WebBuilder = WebApplication.CreateBuilder(args);
var app = WebBuilder.Build();

app.MapGet("/", () => {
    try
    {
        Console.WriteLine("looking for banks!");
        SavingsAcount myAccount = new SavingsAcount("Todd Alberto", 300);
        return Results.Ok($"Your balance is ${myAccount.getBalance()}");

    }
    catch (Exception ex)
    {

        Console.WriteLine($"We got an error: {ex.Message}");
        return Results.Ok(ex.Message);

    }
});

app.Run();




//End of minimal API


// Q&A loop
while (true)
{
    Console.Write("Question: ");
    string question = Console.ReadLine()!;

    //Get additional context from embeddings
    builder.Clear();
    await foreach (var result in memory.SearchAsync(collectionName, question, limit: 3))
          builder.AppendLine(result.Metadata.Text);

    // Get additional context from any function the LLM thinks we should invoke

    Plan plan = await new ActionPlanner(kernel).CreatePlanAsync(question);
    string? plannerResult = (await kernel.RunAsync(plan)).GetValue<string>()!;
    
    
    //Console.WriteLine(plannerResult);
    
    if (!string.IsNullOrEmpty(plannerResult))
    {
        builder.AppendLine(plannerResult);
    }

    int contextToRemove = -1;
    if (builder.Length != 0)
    {
        builder.Insert(0, "Here's some additional information: ");
        contextToRemove = chat.Count;
        chat.AddUserMessage(builder.ToString());
    }
    
    chat.AddUserMessage(question);

    builder.Clear();
    await foreach (string message  in ai.GenerateMessageStreamAsync(chat))
    {
        Console.Write(message);
        builder.Append(message);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());

    if (contextToRemove >= 0) chat.RemoveAt(contextToRemove);
    Console.WriteLine();
}