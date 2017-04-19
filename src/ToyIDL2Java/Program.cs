using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace ToyIDL2Java
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Program.exe <-g|--greeting|-$ <greeting>> [name <fullname>]
            // [-?|-h|--help] [-u|--uppercase]
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            CommandArgument files = null;
            commandLineApplication.Argument(
                  "file",
                  "Path to the .tidl file you want to compile.",
                  multipleValues: true);

            CommandOption package = commandLineApplication.Option(
              "-p |--package <package>",
              "The name of the package the generated interface will be a part of"
              + " a format string where {fullname} will be "
              + "substituted with the full name.",
              CommandOptionType.SingleValue);
            CommandOption uppercase = commandLineApplication.Option(
              "-u | --uppercase", "Display the greeting in uppercase.",
              CommandOptionType.NoValue);
            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                if (greeting.HasValue())
                {
                    Greet(greeting.Value(), names.Values, uppercase.HasValue());
                }
                return 0;
            });
            commandLineApplication.Execute(args);
        }
    }
}
