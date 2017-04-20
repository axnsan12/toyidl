using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using ToyIDL;

namespace ToyIDL2Java
{
    public class JavaGenerator
    {
        private InterfaceDeclaration Interface { get; }


        public JavaGenerator(InterfaceDeclaration @interface)
        {
            Interface = @interface ?? throw new ArgumentNullException(nameof(@interface));

        }


        private string _interfaceTemplate;
        private string InterfaceTemplate
        {
            get
            {
                if (_interfaceTemplate != null)
                {
                    return _interfaceTemplate;
                }

                Assembly assembly = typeof(JavaGenerator).GetTypeInfo().Assembly;
                const string resourceName = "ToyIDL2Java.Interface.java.tpl";
                
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    return _interfaceTemplate = reader.ReadToEnd().Replace("\r\n", Environment.NewLine);
                }
            }
        }

        private string _methodTabs;
        private string MethodTabs
        {
            get
            {
                if (_methodTabs != null)
                {
                    return _methodTabs;
                }

                string template = InterfaceTemplate;
                int declIndex = template.IndexOf("{methodDeclarations}", StringComparison.Ordinal);
                string tabs = template.Substring(startIndex: 0, length: declIndex);
                int lineBeginIndex = tabs.LastIndexOf(Environment.NewLine, StringComparison.Ordinal) + Environment.NewLine.Length;
                return _methodTabs = tabs.Substring(lineBeginIndex, tabs.Length - lineBeginIndex);
            }
        }

        private static readonly IReadOnlyDictionary<IncompleteType, string> JavaTypeName = new Dictionary<IncompleteType, string>()
        {
            { IncompleteType.VOID, "void" },
            { CompleteType.STRING, "String" },
            { CompleteType.INT, "int" },
            { CompleteType.FLOAT, "float" }
        };

        private static readonly Func<ArgumentDefinition, string> ArgFormatter = 
            a => $"{JavaTypeName[a.Type]} {a.Name}";

        private static readonly Func<MethodDeclaration, string> MethodFormatter = 
            m => $"{JavaTypeName[m.ReturnType]} {m.Name}({string.Join(", ", m.Arguments.Select(ArgFormatter))});";

        public string GenerateCode(string packageName)
        {
            string javaCode = InterfaceTemplate;

            return javaCode.Replace("{packageName}", packageName)
                .Replace("{interfaceName}", Interface.Name)
                .Replace("{methodDeclarations}", string.Join($"\n{MethodTabs}", Interface.Methods.Select(MethodFormatter)));
        }

        public static void Main(string[] args)
        {
            // JavaGenerator.exe <-g|--greeting|-$ <greeting>> [name <fullname>]
            // [-?|-h|--help] [-u|--uppercase]
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);

            CommandArgument files = commandLineApplication.Argument(
                "file",
                "Path to the .tidl file you want to compile.",
                multipleValues: true);

            CommandOption package = commandLineApplication.Option(
                "-p |--package <package>",
                "The name of the package the generated interface will be a part of.",
                CommandOptionType.SingleValue);

            CommandOption directory = commandLineApplication.Option(
                "-d |--directory <directory>",
                "The path of the directory where the output files will be created.",
                CommandOptionType.SingleValue);

            commandLineApplication.HelpOption("-? | -h | --help");

            commandLineApplication.OnExecute(() =>
            {
                if (files.Values.Count == 0)
                {
                    Console.WriteLine("File name required.");
                    commandLineApplication.ShowHelp();
                    return 1;
                }
                if (!package.HasValue())
                {
                    Console.WriteLine("Package name is required.");
                    commandLineApplication.ShowHelp();
                    return 1;
                }
                if (directory.HasValue() && !Directory.Exists(directory.Value()))
                {
                    Console.WriteLine($"{directory.Value()} is not a directory.");
                    commandLineApplication.ShowHelp();
                    return 1;
                }
                
                foreach (string file in files.Values)
                    try
                    {
                        var parser = new Parser(File.ReadAllText(file));
                        var generator = new JavaGenerator(parser.Interface);
                        string javaCode = generator.GenerateCode(package.Value());

                        string outputDirectory = directory.HasValue() ? directory.Value() : Path.GetDirectoryName(file);
                        string outputFileName = parser.Interface.Name + ".java";
                        if (outputDirectory != null)
                        {
                            string outputPath = Path.Combine(outputDirectory, outputFileName);
                            File.WriteAllText(outputPath, javaCode);
                            Console.WriteLine($"{file} sucesfully compiled - written to {outputPath}");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to locate output directory.");
                            return 1;
                        }
                    }
                    catch (IdlParserException e)
                    {
                        Console.WriteLine($"{file} parse error - {e.Message}");
                    }

                return 0;
            });
            commandLineApplication.Execute(args);
        }
    }
}