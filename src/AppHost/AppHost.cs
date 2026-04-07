var builder = DistributedApplication.CreateBuilder(args);

var foundry = builder.AddAzureAIFoundry("foundry").RunAsFoundryLocal();
var chat = foundry.AddDeployment("chat", "qwen2.5-1.5b", "1", "Microsoft");

builder.AddProject<Projects.WebApp>("webapp").WithReference(chat).WaitFor(chat);

builder.Build().Run();
