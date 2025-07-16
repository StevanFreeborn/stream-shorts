// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Resources;

var resourceManager = new ResourceManager("StreamShorts.Console.Resources.Resources", typeof(Program).Assembly);

Console.WriteLine(resourceManager.GetString("WelcomeMessage", CultureInfo.CurrentCulture));
