using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Azure.AI.OpenAI;
using Azure;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography.X509Certificates;
using Humanizer;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using System.Text.RegularExpressions;

public class AiCore
    {

    StringBuilder builder = new();
    IKernel kernel;

   
    ISemanticTextMemory memory;
    string collectionName = "canColVeHai";
    ChatHistory chat;
    IChatCompletion ai;





    //constructor
    public AiCore()
    {
    }
    public async Task SetUp() { 

        //string proxyUrl = "https://aoai.hacktogether.net";
        //string aoaiEndpoint = new(proxyUrl + "/v1/api"); ;

        string aoaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
        string aoaiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
        //string aoaiModel = "gpt-3.5-turbo";
        string aoaiModel = "Gpt35Turbo_0301"; // LLM for the chat

        //Initialize the kernel
        var kBuilder = new KernelBuilder();

        //kBuilder.WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())) // this is to show whats going on the console
        //    .WithAzureOpenAIChatCompletionService(aoaiModel, aoaiEndpoint, aoaiApiKey);



        kBuilder.WithAzureOpenAIChatCompletionService(aoaiModel, aoaiEndpoint, aoaiApiKey);

         kernel = kBuilder.Build();

        //Ensure we have embeddings for our document
        memory = new MemoryBuilder()
            .WithMemoryStore(new QdrantMemoryStore("http://localhost:6333/", 1536))
            .WithLoggerFactory(kernel.LoggerFactory)
            .WithAzureOpenAITextEmbeddingGenerationService("TextEmbeddingAda002_1", aoaiEndpoint, aoaiApiKey)
            .Build();


        // Register helpful functions with the kernel 

        kernel.RegisterCustomFunction(SKFunction.Create(
            () => $"The current date and time are {DateTime.UtcNow:r}",
            "DateTime", "Now", "Gets the current date and time."));



        IList<string> collections = await memory.GetCollectionsAsync();

        


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

        //End of Parse PDF files 


        // Create a new chat 
         ai = kernel.GetService<IChatCompletion>();
         chat = ai.CreateNewChat(
            "You are an AI assistant that helps people find information.  Use only the data provided in the sources ");
        


    }
    public async Task<string> AskQuestion(string question)
        {

        //Get additional context from embeddings
        builder.Clear();
        await foreach (var result in memory.SearchAsync(collectionName, question, limit: 3))
            builder.AppendLine(result.Metadata.Text);

        // Get additional context from any function the LLM thinks we should invoke

        var plan = await new ActionPlanner(kernel).CreatePlanAsync(question);
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
        await foreach (string message in ai.GenerateMessageStreamAsync(chat))
        {
            //Console.Write(message);
            builder.Append(message);
        }
        chat.AddAssistantMessage(builder.ToString());
        return builder.ToString();
    }

}

