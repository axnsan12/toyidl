using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToyIDL
{
    public class IncompleteType
    {
        public string TypeName { get; }
        public Type DotnetType { get; }

        public IncompleteType(string typeName, Type dotnetType)
        {
            TypeName = typeName;
            DotnetType = dotnetType;
        }

        public static readonly IncompleteType VOID = new IncompleteType("void", typeof(void));

        public static readonly IReadOnlyList<IncompleteType> AllTypes = new IncompleteType[] { VOID, CompleteType.INT, CompleteType.FLOAT, CompleteType.STRING };
        public static readonly IReadOnlyDictionary<string, IncompleteType> ByTypeName = AllTypes.ToDictionary(t => t.TypeName);

        public override string ToString()
        {
            return TypeName;
        }
    }

    public class CompleteType : IncompleteType
    {
        public CompleteType(string typeName, Type dotnetType) : base(typeName, dotnetType) { }

        public static readonly CompleteType STRING = new CompleteType("string", typeof(string));
        public static readonly CompleteType INT = new CompleteType("int", typeof(int));
        public static readonly CompleteType FLOAT = new CompleteType("float", typeof(float));

        public static new readonly IReadOnlyList<CompleteType> AllTypes = new CompleteType[] { INT, FLOAT, STRING };
        public static new readonly IReadOnlyDictionary<string, CompleteType> ByTypeName = AllTypes.ToDictionary(t => t.TypeName);
    }

    public class ArgumentDefinition
    {
        public CompleteType Type { get; }
        public string Name { get; }

        public ArgumentDefinition(CompleteType type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }

    public class MethodDeclaration
    {
        public IncompleteType ReturnType { get; }
        public string Name { get; }
        public IReadOnlyList<ArgumentDefinition> Arguments { get; }

        public MethodDeclaration(IncompleteType returnType, string name, IList<ArgumentDefinition> arguments)
        {
            ReturnType = returnType;
            Name = name;
            Arguments = new List<ArgumentDefinition>(arguments);
        }

        public override string ToString()
        {
            return $"{ReturnType} {Name}({string.Join(", ", Arguments)})";
        }
    }

    public class InterfaceDeclaration
    {
        public string Name { get; }
        public IReadOnlyList<MethodDeclaration> Methods { get; }

        public InterfaceDeclaration(string name, IList<MethodDeclaration> methods)
        {
            Name = name;
            Methods = new List<MethodDeclaration>(methods);
        }

        public override string ToString()
        {
            return $"interface {Name};\n" + string.Join(";\n", Methods) + (Methods.Count > 0 ? ";" : "");
        }
    }
}
