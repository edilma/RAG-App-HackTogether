## Technical Overview 
Chat with own Data - ChatGPT-like Application using RAG pattern that allows to ask question to my own documents.  The user can ask the question in any language  - I Used Semantic Kernel to integrate a LLM (OpenAI) using C# to orchestrate AI pluggins (Azure Cognitive Services).  For the document embeddings I used Qdrant as  the vector database and Pdfpig to extract the text content fromt the pdf files. I used Docker to host the database.

## Tech Stack 
### .Net 8.0
### Semantic Kernel
### Azure Cognitive Service
### Qdrant Vector Database
### Docker 
### PdfPig


## Why I created the App

Created to participate in the Microsoft Hackathon for .NET.  The objective was to use .Net 8.0 capabilities and Artificial Intelligent tools within the .NET universe only...

Bridging the gap to legal immigration: I built this app driven by the belief that everyone deserves a safe and legal path to opportunity. 

The app is an AI-powered chatbot. It leverages Retrieval-Augmented Generation (RAG) technology. Similar to ChatGPT, my app only answer  questions (in any language) based on official documentation (that I provided), offering reliable and personalized guidance. 

Beyond showcasing my coding skills in AI integration, data management, and chatbot design, this project demonstrates my passion for creating impactful solutions. 

Ready to leverage AI for positive change? Let's discuss how my skills can contribute to your team's success.

## Overview

This application is a command-line tool that leverages the Microsoft Semantic Kernel to create a conversational AI assistant. The assistant is designed to help users find information about a new humanitarian program that the government of Canada created for 3 countries.  I have included PDF documents with information about the program. It utilizes the OpenAI GPT-3.5 Turbo model for chat completion and integrates with a Qdrant-based memory store for document embeddings and retrieval.

## Features

* Accurate and Reliable Information
* Hallucination-Free Answers
* Multilingual Interface
* Low API token consumption
* Scalability
* Portability across platforms
* Quick Deployment
* Environment-Independent
* Targeted and Impactful Assistance


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
