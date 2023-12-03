using Azure.AI.OpenAI;
using Azure;
using Microsoft.SemanticKernel;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography.X509Certificates;
using Humanizer;
using System.Numerics;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

string proxyUrl = "https://aoai.hacktogether.net";
string aoaiEndpoint = new(proxyUrl + "/v1/api"); ;

string aoaiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
string aoaiModel = "gpt-3.5-turbo";

//Initialize the kernel
var kBuilder = new KernelBuilder();

kBuilder.WithAzureOpenAIChatCompletionService(aoaiModel, aoaiEndpoint, aoaiApiKey);

var kernel = kBuilder.Build();

//Register function with the kernel
kernel.RegisterCustomFunction(SKFunction.Create(
    () => $"{DateTime.UtcNow:R}",
    "DateTime", "Now", "Gets the current date and time"));
ISKFunction qa = kernel.CreateSemanticFunction("""
    The current date and time is {{datetime.now}}.
    {{$input}}
    """);

// Create a new chat with history
IChatCompletion ai = kernel.GetService<IChatCompletion>();
ChatHistory chat = ai.CreateNewChat(
    "You are an AI assistant that helps people find information.");
StringBuilder builder = new();




// Q&A loop
while (true)
{
    Console.Write("Question: ");
    //Console.WriteLine((await qa.InvokeAsync(Console.ReadLine()!, kernel, functions: kernel.Functions)).GetValue<string>());
    chat.AddUserMessage(Console.ReadLine()!);

    builder.Clear();
    await foreach (string message in ai.GenerateMessageStreamAsync(chat))
    {
        Console.Write(message);
        builder.Append(message);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());

    Console.WriteLine();
}