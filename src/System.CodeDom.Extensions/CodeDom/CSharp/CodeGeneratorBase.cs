// -----------------------------------------------------------------------
// <copyright file="CodeGeneratorBase.cs" company="sped.mobi">
//     Copyright (c) 2019 Brad Marshall. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.CodeDom.CSharp
{
    public abstract class CodeGeneratorBase : ICodeGenerator, IDisposable
    {
        private const int ParameterMultilineThreshold = 15;


        public bool MoveUsingsOutsideNamespace { get; set; }

        public bool MultiLineDocComments { get; set; }

        protected CodeGeneratorOptions Options { get; set; }

        protected IndentedTextWriter Output { get; set; }

        protected CodeTypeDeclaration CurrentClass { get; set; }

        protected CodeTypeMember CurrentMember { get; set; }

        protected string CurrentMemberName
        {
            get
            {
                if (CurrentMember != null)
                {
                    return CurrentMember.Name;
                }

                return "<% unknown %>";
            }
        }

        protected string CurrentTypeName
        {
            get
            {
                if (CurrentClass != null)
                {
                    return CurrentClass.Name;
                }

                return "<% unknown %>";
            }
        }

        protected int Indent { get => Output.Indent; set => Output.Indent = value; }

        protected bool IsCurrentClass
        {
            get
            {
                if (CurrentClass != null && !(CurrentClass is CodeTypeDelegate))
                {
                    return CurrentClass.IsClass;
                }

                return false;
            }
        }

        protected bool IsCurrentDelegate => CurrentClass is CodeTypeDelegate;

        protected bool IsCurrentEnum
        {
            get
            {
                if (CurrentClass != null && !(CurrentClass is CodeTypeDelegate))
                {
                    return CurrentClass.IsEnum;
                }

                return false;
            }
        }

        protected bool IsCurrentInterface
        {
            get
            {
                if (CurrentClass != null && !(CurrentClass is CodeTypeDelegate))
                {
                    return CurrentClass.IsInterface;
                }

                return false;
            }
        }

        protected bool IsCurrentStruct
        {
            get
            {
                if (CurrentClass != null && !(CurrentClass is CodeTypeDelegate))
                {
                    return CurrentClass.IsStruct;
                }

                return false;
            }
        }

        protected virtual T GetOption<T>(string name, T defaultValue)
        {
            object o = Options[name];
            if (o != null && o is T option)
            {
                return option;
            }

            return defaultValue;
        }

        protected virtual string NullToken => "null";

        protected bool InNestedBinary { get; set; }

        protected virtual bool IsValidLanguageIndependentIdentifier(string value)
        {
            return IsValidTypeNameOrIdentifier(value, false);
        }

        public static void ValidateIdentifiers(CodeObject e)
        {
        }

        public virtual void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            if (Output != null)
            {
                throw new InvalidOperationException(SR.CodeGenOutputWriter);
            }

            Options = options ?? new CodeGeneratorOptions();
            Output = new IndentedTextWriter(writer, Options.IndentString);
            try
            {
                CodeTypeDeclaration dummyClass = new CodeTypeDeclaration();
                CurrentClass = dummyClass;
                GenerateTypeMember(member, dummyClass);
            }
            finally
            {
                CurrentClass = null;
                Output = null;
                Options = null;
            }
        }

        protected virtual bool IsValidLanguageIndependentTypeName(string value)
        {
            return IsValidTypeNameOrIdentifier(value, true);
        }

        protected virtual void ContinueOnNewLine(string st)
        {
            Output.WriteLine(st);
        }

        public virtual string CreateEscapedIdentifier(string value)
        {
            return value;
        }

        public virtual string CreateValidIdentifier(string value)
        {
            return value;
        }

        protected virtual void GenerateDocComment(CodeTypeMember member, CodeComment e)
        {
            switch (member)
            {
                case CodeConstructor codeConstructor:
                    {
                        GenerateConstructorComment(e, codeConstructor);
                        break;
                    }
                case CodeMemberMethod codeMemberMethod:
                    {
                        GenerateMethodComment(e, codeMemberMethod);
                        break;
                    }
                case CodeMemberProperty codeMemberProperty:
                    {
                        GeneratePropertyComment(e, codeMemberProperty);
                        break;
                    }
                case CodeTypeDeclaration codeTypeDeclaration:
                    {
                        GenerateTypeComent(e, codeTypeDeclaration);
                        break;
                    }
            }
        }

        protected virtual void GenerateTypeComent(CodeComment comment, CodeTypeDeclaration type)
        {
            Output.WriteLine("/// <summary>");
            Output.Write("/// ");
            Output.WriteLine(comment.Text);
            Output.WriteLine("/// </summary>");
        }

        protected virtual void GeneratePropertyComment(CodeComment comment, CodeMemberProperty property)
        {
            Output.WriteLine("/// <summary>");
            Output.Write("/// ");
            Output.WriteLine(comment.Text);
            Output.WriteLine("/// </summary>");
        }

        protected virtual void GenerateConstructorComment(CodeComment comment, CodeConstructor codeConstructor)
        {
            Output.WriteLine("/// <summary>");
            Output.Write("/// ");
            Output.WriteLine(comment.Text);
            Output.WriteLine("/// </summary>");
            foreach (CodeParameterDeclarationExpression parameter in codeConstructor.Parameters)
            {
                Output.WriteLine($"/// <param name=\"{parameter.Name}\">the <see cref=\"{GetTypeOutput(parameter.Type)}\"/></param>");
            }
        }

        protected virtual void GenerateMethodComment(CodeComment comment, CodeMemberMethod codeMemberMethod)
        {
            Output.WriteLine("/// <summary>");
            Output.Write("/// ");
            Output.WriteLine(comment.Text);
            Output.WriteLine("/// </summary>");
            foreach (CodeParameterDeclarationExpression parameter in codeMemberMethod.Parameters)
            {
                Output.WriteLine($"/// <param name=\"{parameter.Name}\">the {parameter.Type}</param>");
            }
            Output.WriteLine($"/// <returns>{codeMemberMethod.ReturnType}</returns>");
        }

        protected abstract void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e);

        protected abstract void GenerateArrayCreateExpression(CodeArrayCreateExpression e);

        protected abstract void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e);

        protected abstract void GenerateAssignStatement(CodeAssignStatement e);

        protected abstract void GenerateAttachEventStatement(CodeAttachEventStatement e);

        protected abstract void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes);

        protected abstract void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes);

        protected abstract void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e);

        protected virtual void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
        {
            bool indentedExpression = false;
            Output.Write("(");
            GenerateExpression(e.Left);
            Output.Write(" ");
            if (e.Left is CodeBinaryOperatorExpression || e.Right is CodeBinaryOperatorExpression)
            {
                if (!InNestedBinary)
                {
                    indentedExpression = true;
                    InNestedBinary = true;
                    Indent += 3;
                }

                ContinueOnNewLine("");
            }

            OutputOperator(e.Operator);
            Output.Write(" ");
            GenerateExpression(e.Right);
            Output.Write(")");
            if (indentedExpression)
            {
                Indent -= 3;
                InNestedBinary = false;
            }
        }

        protected abstract void GenerateCastExpression(CodeCastExpression e);

        protected virtual void GenerateComment(CodeComment e)
        {
            Output.WriteLine();

            if (e.DocComment)
            {
                if (MultiLineDocComments)
                {
                    Output.WriteLine("/// <summary>");
                    Output.Write("/// ");
                    Output.WriteLine(e.Text);
                    Output.WriteLine("/// </summary>");
                }
                else
                {
                    Output.Write("/// <summary>");
                    Output.Write(e.Text);
                    Output.WriteLine("</summary>");
                }

            }
            else
            {
                Output.Write("// ");
                Output.WriteLine(e.Text);
            }
        }

        protected virtual void GenerateCommentStatement(CodeCommentStatement e)
        {
            if (e.Comment == null)
            {
                throw new ArgumentException(SR.Argument_NullComment, "e");
            }

            GenerateComment(e.Comment);
        }

        protected virtual void GenerateCommentStatements(CodeCommentStatementCollection e)
        {
            foreach (CodeCommentStatement comment in e)
            {
                GenerateCommentStatement(comment);
            }
        }

        protected virtual void GenerateCompileUnit(CodeCompileUnit e)
        {
            GenerateCompileUnitStart(e);
            GenerateNamespaces(e);
            GenerateCompileUnitEnd(e);
        }

        protected virtual void GenerateAttributes(CodeAttributeDeclarationCollection attributes)
        {
            GenerateAttributes(attributes, null, false);
        }

        protected virtual void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix)
        {
            GenerateAttributes(attributes, prefix, false);
        }

        protected virtual void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix, bool inLine)
        {
            if (attributes.Count == 0) return;
            IEnumerator en = attributes.GetEnumerator();
            bool paramArray = false;
            while (en.MoveNext())
            {
                // we need to convert paramArrayAttribute to params keyword to 
                // make csharp compiler happy. In addition, params keyword needs to be after 
                // other attributes.
                CodeAttributeDeclaration current = (CodeAttributeDeclaration)en.Current;
                if (current.Name.Equals("system.paramarrayattribute", StringComparison.OrdinalIgnoreCase))
                {
                    paramArray = true;
                    continue;
                }

                GenerateAttributeDeclarationsStart(attributes);
                if (prefix != null)
                {
                    Output.Write(prefix);
                }

                if (current.AttributeType != null)
                {
                    Output.Write(GetTypeOutput(current.AttributeType));
                }

                if (current.Arguments.Count != 0)
                {
                    Output.Write("(");
                    bool firstArg = true;
                    foreach (CodeAttributeArgument arg in current.Arguments)
                    {
                        if (firstArg)
                        {
                            firstArg = false;
                        }
                        else
                        {
                            Output.Write(", ");
                        }

                        OutputAttributeArgument(arg);
                    }

                    Output.Write(")");
                }
                GenerateAttributeDeclarationsEnd(attributes);
                if (inLine)
                {
                    Output.Write(" ");
                }
                else
                {
                    Output.WriteLine();
                }
            }

            if (paramArray)
            {
                if (prefix != null)
                {
                    Output.Write(prefix);
                }

                Output.Write("params");
                if (inLine)
                {
                    Output.Write(" ");
                }
                else
                {
                    Output.WriteLine();
                }
            }
        }

        protected abstract void GenerateCompileUnitEnd(CodeCompileUnit e);

        protected abstract void GenerateCompileUnitStart(CodeCompileUnit e);


        protected abstract void GenerateCodeSwitchStatement(CodeSwitchStatement e);

        protected abstract void GenerateDefaultBreakSwitchSectionStatement(CodeDefaultBreakSwitchSectionStatement e);

        protected abstract void GenerateDefaultReturnSwitchSectionStatement(CodeDefaultReturnSwitchSectionStatement e);

        protected abstract void GenerateFallThroughSwitchSectionStatement(CodeFallThroughSwitchSectionStatement e);

        protected abstract void GenerateReturnValueSwitchSectionStatement(CodeReturnValueSwitchSectionStatement e);

        protected abstract void GenerateSwitchSectionLabelExpression(CodeSwitchSectionLabelExpression e);

        protected abstract void GenerateBreakSwitchSectionStatement(CodeBreakSwitchSectionStatement e);

        protected virtual void GenerateSwitchSections(CodeSwitchSectionStatementCollection e)
        {
            foreach (var section in e)
            {
                GenerateSwitchSection(section);
            }
        }

        protected virtual void GenerateSwitchSection(CodeSwitchSectionStatement section)
        {
            switch (section)
            {
                case CodeReturnValueSwitchSectionStatement returnValueSection:
                    GenerateReturnValueSwitchSectionStatement(returnValueSection);
                    break;
                case CodeBreakSwitchSectionStatement breakSection:
                    GenerateBreakSwitchSectionStatement(breakSection);
                    break;
                case CodeFallThroughSwitchSectionStatement fallThroughSection:
                    GenerateFallThroughSwitchSectionStatement(fallThroughSection);
                    break;
                case CodeDefaultBreakSwitchSectionStatement defaultBreakSection:
                    GenerateDefaultBreakSwitchSectionStatement(defaultBreakSection);
                    break;
                case CodeDefaultReturnSwitchSectionStatement defaultReturnSection:
                    GenerateDefaultReturnSwitchSectionStatement(defaultReturnSection);
                    break;
            }
        }

        protected abstract void GenerateConditionStatement(CodeConditionStatement e);

        protected abstract void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c);

        protected virtual void GenerateDecimalValue(decimal d)
        {
            Output.Write(d.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
            Output.Write($"default({e.Type})");
        }

        protected abstract void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e);

        protected abstract void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e);

        protected virtual void GenerateDirectionExpression(CodeDirectionExpression e)
        {
            OutputDirection(e.Direction);
            GenerateExpression(e.Expression);
        }

        protected virtual void GenerateDirectives(CodeDirectiveCollection e)
        {
            foreach (CodeDirective directive in e)
            {
                GenerateDirective(directive);
            }
        }

        protected virtual void GenerateDirective(CodeDirective e)
        {
            switch (e)
            {
                case CodeChecksumPragma directive:
                    GenerateChecksumPragma(directive);
                    break;
                case CodeRegionDirective directive:
                    GenerateRegion(directive);
                    break;
            }
        }

        protected virtual void GenerateRegion(CodeRegionDirective e)
        {
            if (e.RegionMode == CodeRegionMode.Start)
            {
                Output.Write("#region ");
                Output.WriteLine(e.RegionText);
            }
            else if (e.RegionMode == CodeRegionMode.End)
            {
                Output.WriteLine("#endregion");
            }
        }


        protected virtual void GenerateChecksumPragma(CodeChecksumPragma e)
        {
            Output.Write("#pragma checksum \"");
            Output.Write(e.FileName);
            Output.Write("\" \"");
            Output.Write(e.ChecksumAlgorithmId.ToString("B", CultureInfo.InvariantCulture));
            Output.Write("\" \"");
            if (e.ChecksumData != null)
            {
                foreach (byte b in e.ChecksumData)
                {
                    Output.Write(b.ToString("X2", CultureInfo.InvariantCulture));
                }
            }

            Output.WriteLine("\"");
        }

        protected virtual void GenerateDoubleValue(double d)
        {
            Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
        }

        protected abstract void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c);

        protected abstract void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c);

        protected abstract void GenerateEventReferenceExpression(CodeEventReferenceExpression e);

        protected virtual void GenerateExpression(CodeExpression e)
        {
            switch (e)
            {
                case CodeArrayCreateExpression expression:
                    GenerateArrayCreateExpression(expression);
                    break;
                case CodeBaseReferenceExpression expression:
                    GenerateBaseReferenceExpression(expression);
                    break;
                case CodeBinaryOperatorExpression expression:
                    GenerateBinaryOperatorExpression(expression);
                    break;
                case CodeCastExpression expression:
                    GenerateCastExpression(expression);
                    break;
                case CodeDelegateCreateExpression expression:
                    GenerateDelegateCreateExpression(expression);
                    break;
                case CodeFieldReferenceExpression expression:
                    GenerateFieldReferenceExpression(expression);
                    break;
                case CodeArgumentReferenceExpression expression:
                    GenerateArgumentReferenceExpression(expression);
                    break;
                case CodeVariableReferenceExpression expression:
                    GenerateVariableReferenceExpression(expression);
                    break;
                case CodeIndexerExpression expression:
                    GenerateIndexerExpression(expression);
                    break;
                case CodeArrayIndexerExpression expression:
                    GenerateArrayIndexerExpression(expression);
                    break;
                case CodeSnippetExpression expression:
                    GenerateSnippetExpression(expression);
                    break;
                case CodeMethodInvokeExpression expression:
                    GenerateMethodInvokeExpression(expression);
                    break;
                case CodeMethodReferenceExpression expression:
                    GenerateMethodReferenceExpression(expression);
                    break;
                case CodeEventReferenceExpression expression:
                    GenerateEventReferenceExpression(expression);
                    break;
                case CodeDelegateInvokeExpression expression:
                    GenerateDelegateInvokeExpression(expression);
                    break;
                case CodeObjectCreateExpression expression:
                    GenerateObjectCreateExpression(expression);
                    break;
                case CodeParameterDeclarationExpression expression:
                    GenerateParameterDeclarationExpression(expression);
                    break;
                case CodeDirectionExpression expression:
                    GenerateDirectionExpression(expression);
                    break;
                case CodePrimitiveExpression expression:
                    GeneratePrimitiveExpression(expression);
                    break;
                case CodePropertyReferenceExpression expression:
                    GeneratePropertyReferenceExpression(expression);
                    break;
                case CodePropertySetValueReferenceExpression expression:
                    GeneratePropertySetValueReferenceExpression(expression);
                    break;
                case CodeThisReferenceExpression expression:
                    GenerateThisReferenceExpression(expression);
                    break;
                case CodeTypeReferenceExpression expression:
                    GenerateTypeReferenceExpression(expression);
                    break;
                case CodeTypeOfExpression expression:
                    GenerateTypeOfExpression(expression);
                    break;
                case CodeDefaultValueExpression expression:
                    GenerateDefaultValueExpression(expression);
                    break;
                default:
                    {
                        if (e == null)
                        {
                            throw new ArgumentNullException("e");
                        }

                        throw new ArgumentException(SR.InvalidElementType);
                    }
            }
        }

        protected abstract void GenerateExpressionStatement(CodeExpressionStatement e);

        protected abstract void GenerateField(CodeMemberField e);

        protected abstract void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e);

        protected abstract void GenerateGotoStatement(CodeGotoStatement e);

        protected abstract void GenerateIndexerExpression(CodeIndexerExpression e);

        protected abstract void GenerateIterationStatement(CodeIterationStatement e);

        protected abstract void GenerateLabeledStatement(CodeLabeledStatement e);

        protected abstract void GenerateLinePragmaEnd(CodeLinePragma e);

        protected abstract void GenerateLinePragmaStart(CodeLinePragma e);

        protected abstract void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c);

        protected abstract void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e);

        protected abstract void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e);

        protected abstract void GenerateMethodReturnStatement(CodeMethodReturnStatement e);

        protected virtual void GenerateNamespace(CodeNamespace e)
        {
            GenerateCommentStatements(e.Comments);
            GenerateNamespaceStart(e);
            GenerateNamespaceImports(e);
            Output.WriteLine("");
            GenerateTypes(e);
            GenerateNamespaceEnd(e);
        }

        protected abstract void GenerateNamespaceEnd(CodeNamespace e);

        protected abstract void GenerateNamespaceImport(CodeNamespaceImport e);

        protected virtual void GenerateNamespaceImports(CodeNamespace e)
        {
            IEnumerator en = e.Imports.GetEnumerator();
            while (en.MoveNext())
            {
                CodeNamespaceImport imp = (CodeNamespaceImport)en.Current;
                if (imp.LinePragma != null)
                {
                    GenerateLinePragmaStart(imp.LinePragma);
                }

                GenerateNamespaceImport(imp);
                if (imp.LinePragma != null)
                {
                    GenerateLinePragmaEnd(imp.LinePragma);
                }
            }
        }

        protected virtual void GenerateNamespaces(CodeCompileUnit e)
        {
            foreach (CodeNamespace n in e.Namespaces)
            {
                GenerateCodeFromNamespace(n, Output, Options);
            }
        }

        protected abstract void GenerateNamespaceStart(CodeNamespace e);

        protected abstract void GenerateObjectCreateExpression(CodeObjectCreateExpression e);

        protected virtual void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                OutputAttributeDeclarations(e.CustomAttributes);
                Output.Write(" ");
            }

            OutputDirection(e.Direction);
            OutputTypeNamePair(e.Type, e.Name);
        }

        protected virtual void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            switch (e.Value)
            {
                case null:
                    Output.Write(NullToken);
                    break;
                case string value:
                    Output.Write(QuoteSnippetString(value));
                    break;
                default:
                    switch (e.Value)
                    {
                        case char _:
                            Output.Write("'" + e.Value + "'");
                            break;
                        case byte b:
                            Output.Write(b.ToString(CultureInfo.InvariantCulture));
                            break;
                        case short s:
                            Output.Write(s.ToString(CultureInfo.InvariantCulture));
                            break;
                        case int i:
                            Output.Write(i.ToString(CultureInfo.InvariantCulture));
                            break;
                        case long l:
                            Output.Write(l.ToString(CultureInfo.InvariantCulture));
                            break;
                        case float value:
                            GenerateSingleFloatValue(value);
                            break;
                        case double value:
                            GenerateDoubleValue(value);
                            break;
                        case decimal value:
                            GenerateDecimalValue(value);
                            break;
                        case bool value when value:
                            Output.Write("true");
                            break;
                        case bool _:
                            Output.Write("false");
                            break;
                        default:
                            throw new ArgumentException(SR.InvalidPrimitiveType, e.Value.GetType().ToString());
                    }

                    break;
            }
        }

        protected abstract void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c);

        protected abstract void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e);

        protected abstract void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e);

        protected abstract void GenerateRemoveEventStatement(CodeRemoveEventStatement e);

        protected virtual void GenerateSingleFloatValue(float s)
        {
            Output.Write(s.ToString("R", CultureInfo.InvariantCulture));
        }

        protected virtual void GenerateSnippetCompileUnit(CodeSnippetCompileUnit e)
        {
            GenerateDirectives(e.StartDirectives);
            if (e.LinePragma != null)
            {
                GenerateLinePragmaStart(e.LinePragma);
            }

            Output.WriteLine(e.Value);
            if (e.LinePragma != null)
            {
                GenerateLinePragmaEnd(e.LinePragma);
            }

            if (e.EndDirectives.Count > 0)
            {
                GenerateDirectives(e.EndDirectives);
            }
        }

        protected abstract void GenerateSnippetExpression(CodeSnippetExpression e);

        protected abstract void GenerateSnippetMember(CodeSnippetTypeMember e);

        protected virtual void GenerateSnippetStatement(CodeSnippetStatement e)
        {
            Output.WriteLine(e.Value);
        }

        protected virtual void GenerateStatement(CodeStatement e)
        {
            if (e.StartDirectives.Count > 0)
            {
                GenerateDirectives(e.StartDirectives);
            }

            if (e.LinePragma != null)
            {
                GenerateLinePragmaStart(e.LinePragma);
            }

            switch (e)
            {
                case CodeCommentStatement statement:
                    GenerateCommentStatement(statement);
                    break;
                case CodeMethodReturnStatement statement:
                    GenerateMethodReturnStatement(statement);
                    break;
                case CodeConditionStatement statement:
                    GenerateConditionStatement(statement);
                    break;
                case CodeTryCatchFinallyStatement statement:
                    GenerateTryCatchFinallyStatement(statement);
                    break;
                case CodeAssignStatement statement:
                    GenerateAssignStatement(statement);
                    break;
                case CodeExpressionStatement statement:
                    GenerateExpressionStatement(statement);
                    break;
                case CodeIterationStatement statement:
                    GenerateIterationStatement(statement);
                    break;
                case CodeThrowExceptionStatement statement:
                    GenerateThrowExceptionStatement(statement);
                    break;
                case CodeSnippetStatement statement:
                    {
                        int savedIndent = Indent;
                        Indent = 0;
                        GenerateSnippetStatement(statement);

                        Indent = savedIndent;
                        break;
                    }
                case CodeVariableDeclarationStatement statement:
                    GenerateVariableDeclarationStatement(statement);
                    break;
                case CodeAttachEventStatement statement:
                    GenerateAttachEventStatement(statement);
                    break;
                case CodeRemoveEventStatement statement:
                    GenerateRemoveEventStatement(statement);
                    break;
                case CodeGotoStatement statement:
                    GenerateGotoStatement(statement);
                    break;
                case CodeLabeledStatement statement:
                    GenerateLabeledStatement(statement);
                    break;
                case CodeSwitchStatement statement:
                    GenerateCodeSwitchStatement(statement);
                    break;
                default:
                    throw new ArgumentException(SR.ResourceManager.GetString(SR.InvalidElementType), e.GetType().FullName);
            }

            if (e.LinePragma != null)
            {
                GenerateLinePragmaEnd(e.LinePragma);
            }

            if (e.EndDirectives.Count > 0)
            {
                GenerateDirectives(e.EndDirectives);
            }
        }

        protected virtual void GenerateStatements(CodeStatementCollection stms)
        {
            IEnumerator en = stms.GetEnumerator();
            while (en.MoveNext())
            {
                ((ICodeGenerator)this).GenerateCodeFromStatement((CodeStatement)en.Current, Output.InnerWriter, Options);
            }
        }

        protected abstract void GenerateThisReferenceExpression(CodeThisReferenceExpression e);

        protected abstract void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e);

        protected abstract void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e);

        protected abstract void GenerateTypeConstructor(CodeTypeConstructor e);

        protected abstract void GenerateTypeEnd(CodeTypeDeclaration e);

        protected virtual void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            Output.Write("typeof(");
            OutputType(e.Type);
            Output.Write(")");
        }

        protected virtual void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e)
        {
            OutputType(e.Type);
        }

        protected virtual void GenerateTypes(CodeNamespace e)
        {
            foreach (CodeTypeDeclaration c in e.Types)
            {
                if (Options.BlankLinesBetweenMembers)
                {
                    Output.WriteLine();
                }

                ((ICodeGenerator)this).GenerateCodeFromType(c, Output.InnerWriter, Options);
            }
        }

        protected abstract void GenerateTypeStart(CodeTypeDeclaration e);

        protected abstract void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e);

        protected abstract void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e);

        public abstract string GetTypeOutput(CodeTypeReference type);

        public virtual bool IsValidIdentifier(string value)
        {
            return true;
        }

        protected virtual void OutputAttributeArgument(CodeAttributeArgument arg)
        {
            if (!string.IsNullOrEmpty(arg.Name))
            {
                OutputIdentifier(arg.Name);
                Output.Write("=");
            }

            GenerateCodeFromExpression(arg.Value, Output.InnerWriter, Options);
        }

        protected virtual void OutputAttributeDeclarations(CodeAttributeDeclarationCollection attributes)
        {
            if (attributes.Count == 0)
            {
                return;
            }

            GenerateAttributeDeclarationsStart(attributes);
            bool first = true;
            IEnumerator en = attributes.GetEnumerator();
            while (en.MoveNext())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    ContinueOnNewLine(", ");
                }

                CodeAttributeDeclaration current = (CodeAttributeDeclaration)en.Current;
                Output.Write(current.Name);
                Output.Write("(");
                bool firstArg = true;
                foreach (CodeAttributeArgument arg in current.Arguments)
                {
                    if (firstArg)
                    {
                        firstArg = false;
                    }
                    else
                    {
                        Output.Write(", ");
                    }

                    OutputAttributeArgument(arg);
                }

                Output.Write(")");
            }

            GenerateAttributeDeclarationsEnd(attributes);
        }

        protected virtual void OutputDirection(FieldDirection dir)
        {
            switch (dir)
            {
                case FieldDirection.In:
                    break;
                case FieldDirection.Out:
                    Output.Write("out ");
                    break;
                case FieldDirection.Ref:
                    Output.Write("ref ");
                    break;
            }
        }

        protected virtual void OutputExpressionList(CodeExpressionCollection expressions)
        {
            OutputExpressionList(expressions, false);
        }

        protected virtual void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems)
        {
            bool first = true;
            IEnumerator en = expressions.GetEnumerator();
            Indent++;
            while (en.MoveNext())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (newlineBetweenItems)
                    {
                        ContinueOnNewLine(",");
                    }
                    else
                    {
                        Output.Write(", ");
                    }
                }

                GenerateCodeFromExpression((CodeExpression)en.Current, Output.InnerWriter, Options);
            }

            Indent--;
        }

        protected virtual void OutputFieldScopeModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.VTableMask)
            {
                case MemberAttributes.New:
                    Output.Write("new ");
                    break;
            }

            switch (attributes & MemberAttributes.ScopeMask)
            {
                case MemberAttributes.Final:
                    Output.Write("sealed ");
                    break;
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Const:
                    Output.Write("const ");
                    break;
            }
        }

        protected virtual void OutputIdentifier(string ident)
        {
            Output.Write(ident);
        }

        protected virtual void OutputMemberAccessModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.AccessMask)
            {
                case MemberAttributes.Assembly:
                case MemberAttributes.FamilyAndAssembly:
                    Output.Write("internal ");
                    break;
                case MemberAttributes.Family:
                    Output.Write("protected ");
                    break;
                case MemberAttributes.FamilyOrAssembly:
                    Output.Write("protected internal ");
                    break;
                case MemberAttributes.Private:
                    Output.Write("private ");
                    break;
                case MemberAttributes.Public:
                    Output.Write("public ");
                    break;
            }
        }

        protected virtual void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.VTableMask)
            {
                case MemberAttributes.New:
                    Output.Write("new ");
                    break;
            }

            switch (attributes & MemberAttributes.ScopeMask)
            {
                case MemberAttributes.Abstract:
                    Output.Write("abstract ");
                    break;
                case MemberAttributes.Final:
                    Output.Write("sealed ");
                    break;
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Override:
                    Output.Write("override ");
                    break;
                default:
                    switch (attributes & MemberAttributes.AccessMask)
                    {
                        case MemberAttributes.Family:
                            //case MemberAttributes.Public:
                            Output.Write("virtual ");
                            break;
                    }

                    break;
            }
        }

        protected virtual void OutputOperator(CodeBinaryOperatorType op)
        {
            switch (op)
            {
                case CodeBinaryOperatorType.Add:
                    Output.Write("+");
                    break;
                case CodeBinaryOperatorType.Subtract:
                    Output.Write("-");
                    break;
                case CodeBinaryOperatorType.Multiply:
                    Output.Write("*");
                    break;
                case CodeBinaryOperatorType.Divide:
                    Output.Write("/");
                    break;
                case CodeBinaryOperatorType.Modulus:
                    Output.Write("%");
                    break;
                case CodeBinaryOperatorType.Assign:
                    Output.Write("=");
                    break;
                case CodeBinaryOperatorType.IdentityInequality:
                    Output.Write("!=");
                    break;
                case CodeBinaryOperatorType.IdentityEquality:
                    Output.Write("==");
                    break;
                case CodeBinaryOperatorType.ValueEquality:
                    Output.Write("==");
                    break;
                case CodeBinaryOperatorType.BitwiseOr:
                    Output.Write("|");
                    break;
                case CodeBinaryOperatorType.BitwiseAnd:
                    Output.Write("&");
                    break;
                case CodeBinaryOperatorType.BooleanOr:
                    Output.Write("||");
                    break;
                case CodeBinaryOperatorType.BooleanAnd:
                    Output.Write("&&");
                    break;
                case CodeBinaryOperatorType.LessThan:
                    Output.Write("<");
                    break;
                case CodeBinaryOperatorType.LessThanOrEqual:
                    Output.Write("<=");
                    break;
                case CodeBinaryOperatorType.GreaterThan:
                    Output.Write(">");
                    break;
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    Output.Write(">=");
                    break;
            }
        }

        protected virtual void OutputParameters(CodeParameterDeclarationExpressionCollection parameters)
        {
            bool first = true;
            bool multiline = parameters.Count > ParameterMultilineThreshold;
            if (multiline)
            {
                Indent += 3;
            }

            IEnumerator en = parameters.GetEnumerator();
            while (en.MoveNext())
            {
                CodeParameterDeclarationExpression current = (CodeParameterDeclarationExpression)en.Current;
                if (first)
                {
                    first = false;
                }
                else
                {
                    Output.Write(", ");
                }

                if (multiline)
                {
                    ContinueOnNewLine("");
                }

                GenerateExpression(current);
            }

            if (multiline)
            {
                Indent -= 3;
            }
        }

        protected abstract void OutputType(CodeTypeReference typeRef);

        protected virtual void OutputTypeAttributes(TypeAttributes attributes, bool isStruct, bool isEnum)
        {
            switch (attributes & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    Output.Write("public ");
                    break;
                case TypeAttributes.NestedPrivate:
                    Output.Write("private ");
                    break;
            }

            if (isStruct)
            {
                Output.Write("struct ");
            }
            else if (isEnum)
            {
                Output.Write("enum ");
            }
            else
            {
                switch (attributes & TypeAttributes.ClassSemanticsMask)
                {
                    case TypeAttributes.Class:
                        if ((attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                        {
                            Output.Write("sealed ");
                        }

                        if ((attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
                        {
                            Output.Write("abstract ");
                        }

                        Output.Write("class ");
                        break;
                    case TypeAttributes.Interface:
                        Output.Write("interface ");
                        break;
                }
            }
        }

        protected virtual void OutputTypeNamePair(CodeTypeReference typeRef, string name)
        {
            OutputType(typeRef);
            Output.Write(" ");
            OutputIdentifier(name);
        }

        protected abstract string QuoteSnippetString(string value);

        public virtual bool Supports(GeneratorSupport supports)
        {
            return true;
        }

        public virtual void ValidateIdentifier(string value)
        {
            if (!IsValidIdentifier(value))
            {
                throw new ArgumentException(SR.InvalidIdentifier, value);
            }
        }

        private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar)
        {
            switch (ch)
            {
                case ':':
                case '.':
                case '$':
                case '+':
                case '<':
                case '>':
                case '-':
                case '[':
                case ']':
                case ',':
                case '&':
                case '*':
                    nextMustBeStartChar = true;
                    return true;
                case '`':
                    return true;
            }

            return false;
        }

        private static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName)
        {
            bool nextMustBeStartChar = true;
            if (value.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                UnicodeCategory uc = char.GetUnicodeCategory(ch);
                switch (uc)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.OtherLetter:
                        nextMustBeStartChar = false;
                        break;
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DecimalDigitNumber:

                        if (nextMustBeStartChar && ch != '_')
                        {
                            return false;
                        }

                        nextMustBeStartChar = false;
                        break;
                    default:

                        if (isTypeName && IsSpecialTypeChar(ch, ref nextMustBeStartChar))
                        {
                            break;
                        }

                        return false;
                }
            }

            return true;
        }

        protected virtual void GenerateConstructors(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is CodeConstructor)
                {
                    CurrentMember = (CodeTypeMember)en.Current;
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    if (CurrentMember.StartDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.StartDirectives);
                    }

                    GenerateCommentStatements(CurrentMember.Comments);
                    CodeConstructor imp = (CodeConstructor)en.Current;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaStart(imp.LinePragma);
                    }

                    GenerateConstructor(imp, e);
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaEnd(imp.LinePragma);
                    }

                    if (CurrentMember.EndDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateEvents(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is CodeMemberEvent)
                {
                    CurrentMember = (CodeTypeMember)en.Current;
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    if (CurrentMember.StartDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.StartDirectives);
                    }

                    GenerateCommentStatements(CurrentMember.Comments);
                    CodeMemberEvent imp = (CodeMemberEvent)en.Current;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaStart(imp.LinePragma);
                    }

                    GenerateEvent(imp, e);
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaEnd(imp.LinePragma);
                    }

                    if (CurrentMember.EndDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateFields(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is CodeMemberField)
                {
                    CurrentMember = (CodeTypeMember)en.Current;
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    if (CurrentMember.StartDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.StartDirectives);
                    }

                    GenerateCommentStatements(CurrentMember.Comments);
                    CodeMemberField imp = (CodeMemberField)en.Current;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaStart(imp.LinePragma);
                    }

                    GenerateField(imp);
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaEnd(imp.LinePragma);
                    }

                    if (CurrentMember.EndDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateMethods(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is CodeMemberMethod && !(en.Current is CodeTypeConstructor) && !(en.Current is CodeConstructor))
                {
                    CurrentMember = (CodeTypeMember)en.Current;
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    if (CurrentMember.StartDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.StartDirectives);
                    }

                    GenerateCommentStatements(CurrentMember.Comments);
                    CodeMemberMethod imp = (CodeMemberMethod)en.Current;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaStart(imp.LinePragma);
                    }

                    if (en.Current is CodeEntryPointMethod)
                    {
                        GenerateEntryPointMethod((CodeEntryPointMethod)en.Current, e);
                    }
                    else
                    {
                        GenerateMethod(imp, e);
                    }

                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaEnd(imp.LinePragma);
                    }

                    if (CurrentMember.EndDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateNestedTypes(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is CodeTypeDeclaration)
                {
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    CodeTypeDeclaration currentClass = (CodeTypeDeclaration)en.Current;
                    GenerateCodeFromType(currentClass, Output.InnerWriter, Options);
                }
            }
        }

        protected virtual void GenerateProperties(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is CodeMemberProperty)
                {
                    CurrentMember = (CodeTypeMember)en.Current;
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    if (CurrentMember.StartDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.StartDirectives);
                    }

                    GenerateCommentStatements(CurrentMember.Comments);
                    CodeMemberProperty imp = (CodeMemberProperty)en.Current;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaStart(imp.LinePragma);
                    }

                    GenerateProperty(imp, e);
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaEnd(imp.LinePragma);
                    }

                    if (CurrentMember.EndDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateSnippetMembers(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            bool hasSnippet = false;
            while (en.MoveNext())
            {
                if (en.Current is CodeSnippetTypeMember)
                {
                    hasSnippet = true;
                    CurrentMember = (CodeTypeMember)en.Current;
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    if (CurrentMember.StartDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.StartDirectives);
                    }

                    GenerateCommentStatements(CurrentMember.Comments);
                    CodeSnippetTypeMember imp = (CodeSnippetTypeMember)en.Current;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaStart(imp.LinePragma);
                    }

                    int savedIndent = Indent;
                    Indent = 0;
                    GenerateSnippetMember(imp);

                    Indent = savedIndent;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaEnd(imp.LinePragma);
                    }

                    if (CurrentMember.EndDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.EndDirectives);
                    }
                }
            }

            if (hasSnippet)
            {
                Output.WriteLine();
            }
        }

        protected virtual void GenerateType(CodeTypeDeclaration e)
        {
            CurrentClass = e;
            if (e.StartDirectives.Count > 0)
            {
                GenerateDirectives(e.StartDirectives);
            }

            GenerateCommentStatements(e.Comments);
            if (e.LinePragma != null)
            {
                GenerateLinePragmaStart(e.LinePragma);
            }

            GenerateTypeStart(e);
            if (Options.VerbatimOrder)
            {
                foreach (CodeTypeMember member in e.Members)
                {
                    GenerateTypeMember(member, e);
                }
            }
            else
            {
                GenerateFields(e);
                GenerateSnippetMembers(e);
                GenerateTypeConstructors(e);
                GenerateConstructors(e);
                GenerateProperties(e);
                GenerateEvents(e);
                GenerateMethods(e);
                GenerateNestedTypes(e);
            }

            CurrentClass = e;
            GenerateTypeEnd(e);
            if (e.LinePragma != null)
            {
                GenerateLinePragmaEnd(e.LinePragma);
            }

            if (e.EndDirectives.Count > 0)
            {
                GenerateDirectives(e.EndDirectives);
            }
        }

        protected virtual void GenerateTypeConstructors(CodeTypeDeclaration e)
        {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current is CodeTypeConstructor)
                {
                    CurrentMember = (CodeTypeMember)en.Current;
                    if (Options.BlankLinesBetweenMembers)
                    {
                        Output.WriteLine();
                    }

                    if (CurrentMember.StartDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.StartDirectives);
                    }

                    GenerateCommentStatements(CurrentMember.Comments);
                    CodeTypeConstructor imp = (CodeTypeConstructor)en.Current;
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaStart(imp.LinePragma);
                    }

                    GenerateTypeConstructor(imp);
                    if (imp.LinePragma != null)
                    {
                        GenerateLinePragmaEnd(imp.LinePragma);
                    }

                    if (CurrentMember.EndDirectives.Count > 0)
                    {
                        GenerateDirectives(CurrentMember.EndDirectives);
                    }
                }
            }
        }

        protected virtual void GenerateTypeMember(CodeTypeMember member, CodeTypeDeclaration declaredType)
        {
            if (Options.BlankLinesBetweenMembers)
            {
                Output.WriteLine();
            }

            if (member is CodeTypeDeclaration)
            {
                ((ICodeGenerator)this).GenerateCodeFromType((CodeTypeDeclaration)member, Output.InnerWriter, Options);

                CurrentClass = declaredType;

                return;
            }

            if (member.StartDirectives.Count > 0)
            {
                GenerateDirectives(member.StartDirectives);
            }

            GenerateCommentStatements(member.Comments);

            GenerateAttributes(member.CustomAttributes);

            if (member.LinePragma != null)
            {
                GenerateLinePragmaStart(member.LinePragma);
            }

            if (member is CodeMemberField)
            {
                GenerateField((CodeMemberField)member);
            }
            else if (member is CodeMemberProperty)
            {
                GenerateProperty((CodeMemberProperty)member, declaredType);
            }
            else if (member is CodeMemberMethod)
            {
                if (member is CodeConstructor)
                {
                    GenerateConstructor((CodeConstructor)member, declaredType);
                }
                else if (member is CodeTypeConstructor)
                {
                    GenerateTypeConstructor((CodeTypeConstructor)member);
                }
                else if (member is CodeEntryPointMethod)
                {
                    GenerateEntryPointMethod((CodeEntryPointMethod)member, declaredType);
                }
                else
                {
                    GenerateMethod((CodeMemberMethod)member, declaredType);
                }
            }
            else if (member is CodeMemberEvent)
            {
                GenerateEvent((CodeMemberEvent)member, declaredType);
            }
            else if (member is CodeSnippetTypeMember)
            {
                int savedIndent = Indent;
                Indent = 0;
                GenerateSnippetMember((CodeSnippetTypeMember)member);

                Indent = savedIndent;

                Output.WriteLine();
            }

            if (member.LinePragma != null)
            {
                GenerateLinePragmaEnd(member.LinePragma);
            }

            if (member.EndDirectives.Count > 0)
            {
                GenerateDirectives(member.EndDirectives);
            }
        }

        public virtual void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            bool setLocal = false;

            if (Output == null)
            {
                setLocal = true;
                Options = o ?? new CodeGeneratorOptions();
                Output = new IndentedTextWriter(w, Options.IndentString);
            }

            try
            {
                if (e is CodeSnippetCompileUnit)
                {
                    GenerateSnippetCompileUnit((CodeSnippetCompileUnit)e);
                }
                else
                {
                    GenerateCompileUnit(e);
                }
            }
            finally
            {
                if (setLocal)
                {
                    Output = null;
                    Options = null;
                }
            }
        }

        public virtual void GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            bool setLocal = false;

            if (Output == null)
            {
                setLocal = true;
                Options = o ?? new CodeGeneratorOptions();
                Output = new IndentedTextWriter(w, Options.IndentString);
            }

            try
            {
                GenerateExpression(e);
            }
            finally
            {
                if (setLocal)
                {
                    Output = null;
                    Options = null;
                }
            }
        }

        public virtual void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
            bool setLocal = false;

            if (Output == null)
            {
                setLocal = true;
                Options = o ?? new CodeGeneratorOptions();
                Output = new IndentedTextWriter(w, Options.IndentString);
            }

            try
            {
                GenerateNamespace(e);
            }
            finally
            {
                if (setLocal)
                {
                    Output = null;
                    Options = null;
                }
            }
        }

        public virtual void GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            bool setLocal = false;
            if (Output == null)
            {
                setLocal = true;
                Options = o ?? new CodeGeneratorOptions();
                Output = new IndentedTextWriter(w, Options.IndentString);
            }

            try
            {
                GenerateStatement(e);
            }
            finally
            {
                if (setLocal)
                {
                    Output = null;
                    Options = null;
                }
            }
        }

        public virtual void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            bool setLocal = false;

            if (Output == null)
            {
                setLocal = true;
                Options = o ?? new CodeGeneratorOptions();
                Output = new IndentedTextWriter(w, Options.IndentString);
            }

            try
            {
                GenerateType(e);
            }
            finally
            {
                if (setLocal)
                {
                    Output = null;
                    Options = null;
                }
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }

        protected virtual string GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments)
        {
            StringBuilder sb = new StringBuilder(128);
            GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
        }

        protected virtual void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
        {
            sb.Append('<');
            bool first = true;
            for (int i = start; i < start + length; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                // it's possible that we call GetTypeArgumentsOutput with an empty typeArguments collection.  This is the case
                // for open types, so we want to just output the brackets and commas. 
                if (i < typeArguments.Count)
                {
                    sb.Append(GetTypeOutput(typeArguments[i]));
                }
            }

            sb.Append('>');
        }
    }
}
