

// Q&A loop
while (true)
{


    Console.Write("AiCore: ");
    //Input question
    string question = Console.ReadLine()!;

    AiCore aiCore = new AiCore();
    string answer = aiCore.AskQuestion(question);
    Console.WriteLine(answer);

    // output the response
    /*Console.WriteLine(builder.ToString())*/;
  

    //if (contextToRemove >= 0) chat.RemoveAt(contextToRemove);
    //Console.WriteLine();
}