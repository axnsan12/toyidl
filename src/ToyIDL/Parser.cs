using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToyIDL
{
    public class IdlParserException : Exception
    {
        public IdlParserException(string message) : base(message) { }
    }

    public class Parser
    {
        /* valid identifiers ~ numbers, letters and _, but not starting with number
         * valid data types ~ int, float string
         * functions can also return void (no return value)
         * first non blank line must contain interface declaration - `interface <identifier>;`
         * following lines must be method declaration - `<return_type> <identifier>(<arg_type> <identifier>, ...);`*/

        private static readonly Regex _idlInterfacePattern = new Regex(
            @"^\s*(?<interface>interface)\s+" + // keyword 'interface'
            @"(?<interface_name>[a-zA-Z_][a-zA-Z0-9_]*)" + // interface name
            @"\s*(?:;\s*)?$" // optional (redundant) terminating ;
        );

        private static readonly Regex _idlFunctionPattern = new Regex(
            @"^\s*(?<return_type>void|string|int|float)\s+" +  // function return type - void, string, int, float
            @"(?<function_name>[a-zA-Z_][a-zA-Z0-9_]+)\s*" +  // function name
            @"\(" + // argument list opening paranthesis
                @"(?:(?<arguments>\s*(?:string|int|float)\s+(?:[a-zA-Z_][a-zA-Z0-9_]*)\s*" +  // optional first argument
                @"(?:,\s*(?:string|int|float)\s+(?:[a-zA-Z_][a-zA-Z0-9_]*)\s*)*)" + // optional following arguments separated by comma
                @"|\s*)\)" + // or no argument at all
            @"\s*(?:;\s*)?$" // optional (redundant) terminating ;
        );

        private readonly string _idlRawCode;
        public InterfaceDeclaration Interface { get; }

        public Parser(string idlString)
        {
            _idlRawCode = idlString;
            Interface = Parse();
        }

        private InterfaceDeclaration Parse()
        {
            using (StringReader reader = new StringReader(_idlRawCode))
            {
                string line;
                int lineNo = 1;
                string interfaceName = null;
                var methods = new List<MethodDeclaration>();
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (interfaceName == null)
                        {
                            Match interfaceMatch = _idlInterfacePattern.Match(line);
                            if (!interfaceMatch.Success)
                                throw new IdlParserException($"Line {lineNo} is not a valid interface declaration");

                            interfaceName = interfaceMatch.Groups["interface_name"].Value;
                        }
                        else
                        {
                            Match functionMatch = _idlFunctionPattern.Match(line);
                            if (!functionMatch.Success)
                                throw new IdlParserException($"Line {lineNo} is not a valid method declaration");

                            IncompleteType returnType = IncompleteType.ByTypeName[functionMatch.Groups["return_type"].Value];
                            string functionName = functionMatch.Groups["function_name"].Value;
                            var arguments = new List<ArgumentDefinition>();
                            
                            if (functionMatch.Groups["arguments"].Success)
                            {
                                string argumentList = functionMatch.Groups["arguments"].Value;
                                foreach (string argString in argumentList.Split(','))
                                {
                                    var arg = argString.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                                    arguments.Add(new ArgumentDefinition(CompleteType.ByTypeName[arg[0]], arg[1]));
                                }
                            }

                            methods.Add(new MethodDeclaration(returnType, functionName, arguments));
                        }
                    }
                    lineNo += 1;
                }

                if (interfaceName == null)
                    throw new IdlParserException("Interface declaration missing");

                if (methods.Count == 0)
                    throw new IdlParserException("Interface with no method declarations");

                InterfaceDeclaration result = new InterfaceDeclaration(interfaceName, methods);
                return result;
            }
        }
    }
}
