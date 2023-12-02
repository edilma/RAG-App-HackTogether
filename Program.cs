using Azure.AI.OpenAI;
using Azure;
using Microsoft.SemanticKernel;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography.X509Certificates;
using Humanizer;
using System.Numerics;

string proxyUrl = "https://aoai.hacktogether.net";
string aoaiEndpoint = new(proxyUrl + "/v1/api"); ;

string aoaiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
string aoaiModel = "gpt-3.5-turbo";

//Initialize the kernel
var builder = new KernelBuilder();

builder.WithAzureOpenAIChatCompletionService(aoaiModel, aoaiEndpoint, aoaiApiKey);

var kernel = builder.Build();

//Register function with the kernel
kernel.RegisterCustomFunction(SKFunction.Create(
    () => $"{DateTime.UtcNow:R}",
    "DateTime", "Now", "Gets the current date and time"));
ISKFunction qa = kernel.CreateSemanticFunction("""
    The current date and time is {{datetime.now}}.
    {{$input}}
    """);

    

// Q&A loop
while (true)
{
    Console.Write("Question: ");
    Console.WriteLine((await qa.InvokeAsync(Console.ReadLine()!, kernel, functions: kernel.Functions)).GetValue<string>());
    Console.WriteLine();
}