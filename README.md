## Technical Overview 
Chat with own Data - ChatGPT-like Application using RAG pattern that allows to ask question to my own documents - I Used Semantic Kernel to integrate a LLM (OpenAI) using C# to orchestrate AI pluggins (Azure Cognitive Services).  For the document embeddings I used Qdrant for the vector database and Pdfpig to extract the text content fromt the pdf files. I used Docker to host the database.

## Tech Stack 
### .Net 8.0
### Semantic Kernel
### Azure Cognitive Service
### Qdrant Vector Database
### Docker 
### PdfPig


## Why I created the App

Created to participate in the Microwoft Hackathon for .NET.  The objective was to use .Net 8.0 capabilities and Artificial Intelligent tools within the .NET universe only...

I developed this application with a specific focus in mind - to assist individuals who are seeking vital information and guidance, particularly in situations where access to accurate and reliable data can be life-changing.

The recent launch of the Canadian government's program, aimed at selecting 11,000 people, underscores the critical need for accessible, trustworthy information. Many individuals may find themselves overwhelmed by the complexity of the application process, worried about potential errors, or concerned about the risk of exploitation.

My app was designed with the intention of providing a helping hand to those who need it the most. It seeks to empower users by offering them a reliable source of information and assistance, ensuring that they can navigate such programs with confidence and without fear of making mistakes.

In a world where information is abundant but accuracy isn't always guaranteed, this application serves as a beacon of trustworthiness. It strives to create a supportive environment where individuals can seek guidance and find the answers they need without fear of being misled or abused.

Ultimately, the goal of this app is to make a positive impact on the lives of those who use it, especially during crucial moments like applying for government programs. It is my hope that by offering this tool, I can contribute to a safer, more informed, and empowered community of users.  I couldn't finish the front end but i am working on it...

## Overview

This application is a command-line tool that leverages the Microsoft Semantic Kernel to create a conversational AI assistant. The assistant is designed to help users find information about a new humanitarian program that the government of Canada created for 3 countries.  I have included PDF documents with information about the program. It utilizes the OpenAI GPT-3.5 Turbo model for chat completion and integrates with a Qdrant-based memory store for document embeddings and retrieval.

## App in Action

[![Click to watch the video](https://img.youtube.com/vi/qjx3xT90kTg/0.jpg)](https://www.youtube.com/watch?v=qjx3xT90kTg)

Click the image above to watch a video demonstration of the app in action.

[![Click to watch the video](https://img.youtube.com/vi/tbobjZFNelA/0.jpg)](https://www.youtube.com/watch?v=tbobjZFNelA)

Click the image above to watch a video demonstration of the app being used in English, Spanish and Creole.

## Prerequisites

Before running this application, make sure you have the following prerequisites in place:

1. **Azure OpenAI API Key**: You need to obtain an Azure OpenAI API key and set it as an environment variable named `AZURE_OPENAI_API_KEY`.

## Dependencies

This application relies on the following libraries and packages:

- `Microsoft.Extensions.Logging`: For logging purposes.
- `Microsoft.SemanticKernel`: The core library for the Semantic Kernel.
- `Microsoft.SemanticKernel.Plugins`: The memory library for the Semantic Kernel.
- `UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor`: For extracting text content from PDF files.
- 'Qdrant': as the Vector Database
- 'Docker' to run the Vector Database

## Usage

### Configuration

1. Initialize the Semantic Kernel using the provided configuration parameters, including the OpenAI model, endpoint, and API key.
2. Register custom functions with the kernel to extend its capabilities.

### Document Embeddings

The application ensures that document embeddings are available for use. It does this by checking if a specific collection exists in the Qdrant memory store. If the collection doesn't exist, the application performs the following steps:

1. Parse PDF files located in a specified directory.
2. Extract text content from each PDF document and break it into smaller pieces.
3. Store the extracted text in the Qdrant memory store.

### Chat Interaction

The application enables users to interact with the AI assistant in a conversational manner. The chat interaction includes the following steps:

1. User enters a question or query.
2. The application retrieves additional context from document embeddings.
3. The application also checks for any functions that the Semantic Kernel suggests invoking based on the question.
4. The question and context are used to generate a response from the AI assistant.
5. The assistant's response is displayed, and the conversation continues.

## Running the Application

To run the application, execute the code provided in the readme in a compatible development environment or IDE. Ensure that the required dependencies and prerequisites are set up as mentioned above.

## Note

- The application uses the OpenAI GPT-3.5 Turbo model for chat completion. Make sure your Azure OpenAI API key is correctly configured for authentication.
- The Qdrant memory store is used for document embeddings. Ensure that the store is properly set up and running at the specified endpoint.

Feel free to modify and extend this application according to your specific use case and requirements.
