using System.CodeDom.Compiler;
using System.IO;

namespace System.CodeDom.CSharp
{
    public class CSharpCodeDomProvider : CodeDomProvider
    {
        [Obsolete]
        public override ICodeCompiler CreateCompiler()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override ICodeGenerator CreateGenerator()
        {
            return new CSharpGenerator();
        }

        private static readonly CSharpGenerator Generator = new CSharpGenerator();

        public override void GenerateCodeFromCompileUnit(CodeCompileUnit compileUnit, TextWriter writer, CodeGeneratorOptions options)
        {
            Generator.GenerateCodeFromCompileUnit(compileUnit, writer, options);
        }

        public override void GenerateCodeFromExpression(CodeExpression expression, TextWriter writer, CodeGeneratorOptions options)
        {
            Generator.GenerateCodeFromExpression(expression, writer, options);
        }

        public override void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            Generator.GenerateCodeFromMember(member, writer, options);
        }

        public override void GenerateCodeFromNamespace(CodeNamespace codeNamespace, TextWriter writer, CodeGeneratorOptions options)
        {
            Generator.GenerateCodeFromNamespace(codeNamespace, writer, options);
        }

        public override void GenerateCodeFromStatement(CodeStatement statement, TextWriter writer, CodeGeneratorOptions options)
        {
            Generator.GenerateCodeFromStatement(statement, writer, options);
        }

        public override void GenerateCodeFromType(CodeTypeDeclaration codeType, TextWriter writer, CodeGeneratorOptions options)
        {
            Generator.GenerateCodeFromType(codeType, writer, options);
        }
    }
}
