﻿using System;
using System.Security.Permissions;
using System.Security.Principal;
using Serilog;
using Serilog.Core;
using WindowsPostSetupAssistant.Core;
using WindowsPostSetupAssistant.Main.CommandLine;
using WindowsPostSetupAssistant.UI;
using WindowsPostSetupAssistant.UI.MainWindowDependencies;

namespace WindowsPostSetupAssistant.Main;

public static class Program
{
    private static readonly Logger Logger;
    private static string ExecuteProfileArgument => "executeProfile";
    private static string ChooseProfileArgument => "chooseProfile";

    static Program()
    {
        Logger = new LoggerConfiguration()
            .Enrich.WithProperty("Application", "SerilogTestContext")
            //.MinimumLevel.Information()
            .MinimumLevel.Debug()
            .WriteTo.File(ApplicationPaths.LogPath, rollingInterval: RollingInterval.Day)
            .WriteTo.Debug()
            .CreateLogger();
    }
    
    [STAThread]
    public static void Main()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(id);
        var runningAsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        if (!runningAsAdmin) throw new Exception("This application must be run as administrator. " +
            "Please re-run application with administrator priveleges");

        var argumentsParser = new ArgumentsParser(new CommandLineInterface());

        // Check if both are present and warn user they are mutually exclusive
        if (argumentsParser.ArgumentPresent(ExecuteProfileArgument) &&
            argumentsParser.ArgumentPresent(ChooseProfileArgument))
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: Mutually exclusive arguments present");
            Console.WriteLine();
            Console.WriteLine($"Both /{ExecuteProfileArgument} and /{ChooseProfileArgument}");
            Console.WriteLine("Were found. They are mutually exclusive and cannot be run together.");
            Console.WriteLine();
            Console.WriteLine("Please modify arguments.");
            Console.WriteLine("Program will now exit.");

            Environment.Exit(0);
        }
        
        // If neither profile activation option is present, user is just running the GUI
        if (!argumentsParser.ArgumentPresent(ExecuteProfileArgument) &&
            !argumentsParser.ArgumentPresent(ChooseProfileArgument))
        {
            LaunchGui();
        }

        if (argumentsParser.ArgumentPresent(ChooseProfileArgument))
        {
            Console.WriteLine();
            Console.WriteLine("======== RUNNING CHOOSE PROFILE HERE ========");
            Console.WriteLine();
        }
        
        if (argumentsParser.ArgumentPresent(ExecuteProfileArgument))
        {
            Console.WriteLine();
            Console.WriteLine("======== EXECUTE SELECTED PROFILE HERE ========");
            Console.WriteLine($"Selected profile passed is: {argumentsParser.GetArgumentValue(ExecuteProfileArgument)}");
            Console.WriteLine();
        }
    }

    private static void LaunchGui()
    {
        var uiMainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(Logger)
        };

        uiMainWindow.ShowDialog();
    }
}