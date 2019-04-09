﻿using CommandLine;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SigStat.Benchmark
{
    class Program
    {
        public static CloudStorageAccount Account;
        public static string Experiment;
        public static string Input;
        public static string Output;

        public abstract class Options
        {
            [Option('k', "key", Required = true, HelpText = "Azure key to use for managing the job.")]
            public string AccountKey { get; set; }

            [Option('a', "account", Required = false, Default = "preprocessingbenchmark", HelpText = "Azure storage account to use for managing the job.")]
            public string AccountName { get; set; }

            [Option('e', "experiment", Required = false, Default = "default", HelpText = "Unique name for the experiment")]
            public string Experiment { get; set; }

            public abstract Task getTask();
        }

        [Verb("monitor", HelpText = "Start monitoring")]
        class MonitorOptions : Options
        {
            public override Task getTask()
            {
                return Monitor.RunAsync();
            }
        }

        [Verb("work", HelpText = "Start worker")]
        class WorkOptions : Options
        {
            [Option('o', "outputDir", Required = false, Default = "", HelpText = "Output directory")]
            public string OutputDirectory { get; set; }

            public override Task getTask()
            {
                Output = OutputDirectory;
                return Worker.RunAsync();
            }
        }

        [Verb("generatejobs", HelpText = "Start job generator")]
        class GenerateJobsOptions : Options
        {
            [Option('o', "inputDir", Required = false, Default = "", HelpText = "Input directory")]
            public string InputDirectory { get; set; }

            public override Task getTask()
            {
                Input = InputDirectory;
                return Worker.RunAsync();
            }
        }

        [Verb("analyse", HelpText = "Start analyser")]
        class AnalyseOptions : Options
        {
            [Option('o', "inputDir", Required = false, Default = "", HelpText = "Input directory")]
            public string InputDirectory { get; set; }
            [Option('o', "output", Required = false, Default = "", HelpText = "Output file")]
            public string OutputFile { get; set; }

            public override Task getTask()
            {
                Input = InputDirectory;
                Output = OutputFile;
                return Worker.RunAsync();
            }
        }

        public static bool getCommonOptions(Options o)
        {
            if (o.AccountKey.EndsWith(".txt"))
            {
                if (!File.Exists(o.AccountKey))
                {
                    Console.WriteLine($"Account key file '{o.AccountKey}' does not exist");
                    return false;
                }
                o.AccountKey = File.ReadAllText(o.AccountKey);
            }

            try
            {
                var credentials = new StorageCredentials(o.AccountName, o.AccountKey);
                Account = new CloudStorageAccount(credentials, true);
                Experiment = o.Experiment;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Account key is invalid");
                return false;
            }
            return true;
        }

        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<MonitorOptions, WorkOptions, GenerateJobsOptions, AnalyseOptions>(args)
                .MapResult<Options, Task>(o =>
                {
                    if (!getCommonOptions(o)) return Task.FromResult(-1);

                    return o.getTask();
                },
                   errs => Task.FromResult(-1));
            Console.WriteLine("Execution finished. Press any key to exit the application...");
            Console.ReadKey();
        }
    }
    
}

