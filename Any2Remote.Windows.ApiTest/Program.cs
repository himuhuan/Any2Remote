using Grpc.Net.Client;
using Any2Remote.Windows.Grpc.Services;

var channel = GrpcChannel.ForAddress("https://localhost:7133");
var client = new Local.LocalClient(channel);
var response = client.GetLocalApps(new LocalAppsRequest { IncludeSystemComponent = false});

Console.WriteLine(response.Apps.Count);