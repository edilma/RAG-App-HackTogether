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
string collectionName = "net7perf";
if (collections.Contains("net7perf"))
{
    Console.WriteLine("Found Database");
}
else
{
    using (HttpClient client = new())
    {
        string s = await client.GetStringAsync("https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7");
        List<string> paragraphs =
            TextChunker.SplitPlainTextParagraphs(
                TextChunker.SplitPlainTextLines(
                    WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", "")),
                    128),
                1024);
        for (int i = 0; i < paragraphs.Count; i++)
            await memory.SaveInformationAsync(collectionName, paragraphs[i], $"paragraph{i}");
        Console.WriteLine("Generated Database");
    }
}

// Create a new chat 
IChatCompletion ai = kernel.GetService<IChatCompletion>();
ChatHistory chat = ai.CreateNewChat(
    "You are an AI assistant that helps people find information.");
StringBuilder builder = new();


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
    string? plannerResult = (await kernel.RunAsync(plan)).GetValue<string>();
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