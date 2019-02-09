﻿// -----------------------------------------------------------------------
// <copyright file="CSharpCodeGenerator.cs" company="sped.mobi">
//     Copyright (c) 2019 Brad Marshall. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.CodeDom.CSharp
{
    public class CSharpCodeGenerator : CodeGenerator, IDisposable
    {
        public CSharpCodeGeneratorOptions CSharpOptions;

        private bool generatingForLoop;

        protected override string NullToken => "null";

        public void Dispose()
        {

        }

        public void Flush()
        {
            Output.Flush();
        }

        public async Task FlushAsync()
        {
            await Output
                .FlushAsync()
                .ConfigureAwait(false);
        }

        public virtual void GenerateBreakSwitchSectionStatement(CodeBreakSwitchSectionStatement e)
        {
            GenerateSwitchSectionLabelExpression(e.Label);
            Output.WriteLine();
            Output.WriteLine("{");
            Indent++;
            if (e.BodyStatements.Count > 0)
            {
                GenerateStatements(e.BodyStatements);
            }
            Output.WriteLine("break;");
            Indent--;
            Output.WriteLine("}");
        }

        public void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            SetOptions(o as CSharpCodeGeneratorOptions);
            ((ICodeGenerator)this).GenerateCodeFromCompileUnit(e, w, o);
        }

        public override void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            SetOptions(options as CSharpCodeGeneratorOptions);

            switch (member)
            {
                case CodeMemberEvent memberEvent:
                    GenerateEvent(memberEvent, CurrentClass);
                    break;
                case CodeEntryPointMethod entryPointMethod:
                    GenerateEntryPointMethod(entryPointMethod, CurrentClass);
                    break;
                case CodeConstructor constructor:
                    GenerateConstructor(constructor, CurrentClass);
                    break;
                case CodeMemberField memberField:
                    GenerateField(memberField);
                    break;
                case CodeTypeConstructor typeConstructor:
                    GenerateTypeConstructor(typeConstructor);
                    break;
                case CodeMemberMethod memberMethod:
                    GenerateMethod(memberMethod, CurrentClass);
                    break;
                case CodeMemberProperty memberProperty:
                    GenerateProperty(memberProperty, CurrentClass);
                    break;
                case CodeSnippetTypeMember snippetTypeMember:
                    GenerateSnippetMember(snippetTypeMember);
                    break;
                case CodeTypeDelegate typeDelegate:
                    GenerateDelegate(typeDelegate);
                    break;
                case CodeTypeDeclaration typeDeclaration:
                    GenerateCodeFromType(typeDeclaration, writer, options);
                    break;
            }
        }

        public virtual void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
            if (o is CSharpCodeGeneratorOptions oo)
            {
                SetOptions(oo);
            }

            ((ICodeGenerator)this).GenerateCodeFromNamespace(e, w, o);
        }

        public virtual void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            if (o is CSharpCodeGeneratorOptions oo)
            {
                SetOptions(oo);
            }

            ((ICodeGenerator)this).GenerateCodeFromType(e, w, o);
        }

        public virtual void GenerateCodeSwitchStatement(CodeSwitchStatement e)
        {
            Output.Write("switch(");
            GenerateExpression(e.CheckExpression);
            Output.WriteLine(")");
            Output.WriteLine("{");
            Indent++;
            GenerateSwitchSections(e.Sections);
            Indent--;
            Output.WriteLine("}");
        }

        public virtual void GenerateDefaultBreakSwitchSectionStatement(CodeDefaultBreakSwitchSectionStatement e)
        {
            Output.WriteLine("default:");
            Output.WriteLine("{");
            Indent++;
            if (e.BodyStatements.Count > 0)
            {
                GenerateStatements(e.BodyStatements);
            }
            Output.WriteLine("break;");
            Indent--;
            Output.WriteLine("}");
        }

        public virtual void GenerateDefaultReturnSwitchSectionStatement(CodeDefaultReturnSwitchSectionStatement e)
        {
            Output.WriteLine("default:");
            Output.WriteLine("{");
            Indent++;
            if (e.BodyStatements.Count > 0)
            {
                GenerateStatements(e.BodyStatements);
            }
            GenerateMethodReturnStatement(e.ReturnStatement);
            Indent--;
            Output.WriteLine("}");
        }

        public virtual void GenerateFallThroughSwitchSectionStatement(CodeFallThroughSwitchSectionStatement e)
        {
            GenerateSwitchSectionLabelExpression(e.Label);
            Output.WriteLine();
        }

        public void GenerateReturnValueSwitchSectionStatement(CodeReturnValueSwitchSectionStatement e)
        {
            GenerateSwitchSectionLabelExpression(e.Label);
            if (e.SingleLine)
            {
                GenerateMethodReturnStatement(e.ReturnStatement);
            }
            else
            {
                Output.WriteLine();
                Output.WriteLine("{");
                Indent++;
                if (e.BodyStatements.Count > 0)
                {
                    GenerateStatements(e.BodyStatements);
                }
                GenerateMethodReturnStatement(e.ReturnStatement);
                Indent--;
                Output.WriteLine("}");
            }
        }

        public virtual void GenerateSwitchSectionLabelExpression(CodeSwitchSectionLabelExpression e)
        {
            Output.Write("case ");
            GenerateExpression(e.Expression);
            Output.Write(": ");
        }

        public virtual void GenerateSwitchSections(CodeSwitchSectionStatementCollection e)
        {
            foreach (var section in e)
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
        }

        protected void CloseBlock()
        {
            Indent--;
            Output.WriteLine("}");
        }

        protected override string CreateEscapedIdentifier(string value)
        {
            //if (IsKeyword(value) || IsPrefixTwoUnderscore(value))
            //{
            //    return "@" + value;
            //}

            return value;
        }

        protected override string CreateValidIdentifier(string value)
        {
            if (IsPrefixTwoUnderscore(value))
            {
                value = "_" + value;
            }

            while (IsKeyword(value))
            {
                value = "_" + value;
            }

            return value;
        }

        protected override void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e)
        {
            OutputIdentifier(e.ParameterName);
        }

        protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            Output.Write("new ");
            CodeExpressionCollection init = e.Initializers;
            if (init.Count > 0)
            {
                OutputType(e.CreateType);
                if (e.CreateType.ArrayRank == 0)
                {
                    Output.Write("[]");
                }

                Output.WriteLine(" {");
                Indent++;
                OutputExpressionList(init, true);
                Indent--;
                Output.Write("}");
            }
            else
            {
                Output.Write(GetBaseTypeOutput(e.CreateType));
                Output.Write("[");
                if (e.SizeExpression != null)
                {
                    GenerateExpression(e.SizeExpression);
                }
                else
                {
                    Output.Write(e.Size);
                }

                Output.Write("]");
            }
        }

        protected override void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e)
        {
            GenerateExpression(e.TargetObject);
            Output.Write("[");
            bool first = true;
            foreach (CodeExpression exp in e.Indices)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Output.Write(", ");
                }

                GenerateExpression(exp);
            }

            Output.Write("]");
        }

        protected override void GenerateAssignStatement(CodeAssignStatement e)
        {
            GenerateExpression(e.Left);
            Output.Write(" = ");
            GenerateExpression(e.Right);
            if (!generatingForLoop)
            {
                Output.WriteLine(";");
            }
        }

        protected override void GenerateAttachEventStatement(CodeAttachEventStatement e)
        {
            GenerateEventReferenceExpression(e.Event);
            Output.Write(" += ");
            GenerateExpression(e.Listener);
            Output.WriteLine(";");
        }

        protected override void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes)
        {
            Output.Write("]");
        }

        protected override void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes)
        {
            Output.Write("[");
        }

        protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e)
        {
            Output.Write("base");
        }

        protected override void GenerateCastExpression(CodeCastExpression e)
        {
            Output.Write("((");
            OutputType(e.TargetType);
            Output.Write(")(");
            GenerateExpression(e.Expression);
            Output.Write("))");
        }

        protected override void GenerateComment(CodeComment e)
        {
            Output.WriteLine();

            if (e.DocComment)
            {
                if (GetOption("MultilineDocComments", false))
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
                    Output.WriteLine("/// </summary>");
                }
            }
            else
            {
                Output.Write("// ");
                Output.WriteLine(e.Text);
            }
        }

        protected override void GenerateCommentStatement(CodeCommentStatement e)
        {
            if (e.Comment != null)
            {
                GenerateComment(e.Comment);
            }
        }

        protected override void GenerateCompileUnit(CodeCompileUnit e)
        {
            GenerateCompileUnitStart(e);
            GenerateNamespaces(e);
            GenerateCompileUnitEnd(e);
        }

        protected override void GenerateCompileUnitEnd(CodeCompileUnit e)
        {
            if (e.EndDirectives.Count > 0)
            {
                GenerateDirectives(e.EndDirectives);
            }
        }

        protected override void GenerateCompileUnitStart(CodeCompileUnit e)
        {
            if (e.StartDirectives.Count > 0)
            {
                GenerateDirectives(e.StartDirectives);
            }

            WriteAutoGeneratedHeader();

            if (GetOption("MoveUsingsOutsideNamespace", false))
            {
                var importList = GetImportList(e);

                foreach (string import in importList.Keys)
                {
                    Output.Write("using ");
                    OutputIdentifier(import);
                    Output.WriteLine(";");
                }

                if (importList.Keys.Count > 0)
                {
                    Output.WriteLine("");
                }
            }





            if (e.AssemblyCustomAttributes.Count > 0)
            {
                GenerateAttributes(e.AssemblyCustomAttributes, "assembly: ");
                Output.WriteLine("");
            }
        }

        protected override void GenerateConditionStatement(CodeConditionStatement e)
        {
            Output.Write("if (");
            GenerateExpression(e.Condition);
            Output.Write(")");
            Output.WriteLine();
            Output.WriteLine("{");
            Indent++;
            GenerateStatements(e.TrueStatements);
            Indent--;
            CodeStatementCollection falseStatemetns = e.FalseStatements;
            if (falseStatemetns.Count > 0)
            {
                Output.Write("}");
                if (Options.ElseOnClosing)
                {
                    Output.Write(" ");
                }
                else
                {
                    Output.WriteLine("");
                }

                Output.Write("else");
                Output.WriteLine();
                Output.WriteLine("{");
                Indent++;
                GenerateStatements(e.FalseStatements);
                Indent--;
            }

            Output.WriteLine("}");
        }

        protected override void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c)
        {
            Output.WriteLine();
            OutputAccessibilityAndModifiers(e.Attributes);
            Output.Write(e.Name);
            Output.Write("(");
            OutputParameters(e.Parameters);
            Output.WriteLine(')');




            CodeExpressionCollection baseArgs = e.BaseConstructorArgs;
            CodeExpressionCollection thisArgs = e.ChainedConstructorArgs;

            if (baseArgs.Count > 0)
            {
                Output.Write(" : ");
                Indent++;
                Indent++;
                Output.Write("base(");
                GenerateExpressionList(baseArgs, false);
                Output.Write(")");
                Indent--;
                Indent--;
            }

            if (thisArgs.Count > 0)
            {
                Output.WriteLine(" : ");
                Indent++;
                Indent++;
                Output.Write("this(");
                GenerateExpressionList(thisArgs, false);
                Output.Write(")");
                Indent--;
                Indent--;
            }

            Indent++;
            Output.WriteLine();
            OpenBlock();
            GenerateStatements(e.Statements);
            CloseBlock();
        }

        protected override void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e)
        {
            Output.Write("new ");
            OutputType(e.DelegateType);
            Output.Write("(");
            GenerateExpression(e.TargetObject);
            Output.Write(".");
            OutputIdentifier(e.MethodName);
            Output.Write(")");
        }

        protected override void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
            }

            Output.Write("(");
            OutputExpressionList(e.Parameters);
            Output.Write(")");
        }

        protected override void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c)
        {
            if (e.CustomAttributes.Count > 0)
            {
                GenerateAttributes(e.CustomAttributes);
            }

            Output.Write("public static ");
            OutputType(e.ReturnType);
            Output.Write(" Main()");
            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.Statements);
            Indent--;
            Output.WriteLine("}");
        }

        protected override void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c)
        {
            if (IsCurrentDelegate || IsCurrentEnum)
            {
                return;
            }

            if (e.CustomAttributes.Count > 0)
            {
                GenerateAttributes(e.CustomAttributes);
            }

            if (e.PrivateImplementationType == null)
            {
                OutputMemberAccessModifier(e.Attributes);
            }

            Output.Write("event ");
            string name = e.Name;
            if (e.PrivateImplementationType != null)
            {
                name = GetBaseTypeOutput(e.PrivateImplementationType) + "." + name;
            }

            OutputTypeNamePair(e.Type, name);
            Output.WriteLine(";");
        }

        protected override void GenerateEventReferenceExpression(CodeEventReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write(".");
            }

            OutputIdentifier(e.EventName);
        }

        protected override void GenerateExpressionStatement(CodeExpressionStatement e)
        {
            GenerateExpression(e.Expression);
            if (!generatingForLoop)
            {
                Output.WriteLine(";");
            }
        }

        protected override void GenerateField(CodeMemberField e)
        {
            if (IsCurrentDelegate || IsCurrentInterface)
            {
                return;
            }

            if (IsCurrentEnum)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    GenerateAttributes(e.CustomAttributes);
                }

                OutputIdentifier(e.Name);
                if (e.InitExpression != null)
                {
                    Output.Write(" = ");
                    GenerateExpression(e.InitExpression);
                }

                Output.WriteLine(",");
            }
            else
            {
                if (e.CustomAttributes.Count > 0)
                {
                    GenerateAttributes(e.CustomAttributes);
                }

                OutputMemberAccessModifier(e.Attributes);
                OutputVTableModifier(e.Attributes);
                OutputFieldScopeModifier(e.Attributes);
                OutputTypeNamePair(e.Type, e.Name);
                if (e.InitExpression != null)
                {
                    Output.Write(" = ");
                    GenerateExpression(e.InitExpression);
                }

                Output.WriteLine(";");
            }
        }

        protected override void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write(".");
            }

            OutputIdentifier(e.FieldName);
        }

        protected override void GenerateGotoStatement(CodeGotoStatement e)
        {
            Output.Write("goto ");
            Output.Write(e.Label);
            Output.WriteLine(";");
        }

        protected override void GenerateIndexerExpression(CodeIndexerExpression e)
        {
            GenerateExpression(e.TargetObject);
            Output.Write("[");
            bool first = true;
            foreach (CodeExpression exp in e.Indices)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Output.Write(", ");
                }

                GenerateExpression(exp);
            }

            Output.Write("]");
        }

        protected override void GenerateIterationStatement(CodeIterationStatement e)
        {
            generatingForLoop = true;
            Output.Write("for (");
            GenerateStatement(e.InitStatement);
            Output.Write("; ");
            GenerateExpression(e.TestExpression);
            Output.Write("; ");
            GenerateStatement(e.IncrementStatement);
            Output.Write(")");
            Output.WriteLine("{");
            generatingForLoop = false;
            Indent++;
            GenerateStatements(e.Statements);
            CloseBlock();
        }

        protected override void GenerateLabeledStatement(CodeLabeledStatement e)
        {
            Indent--;
            Output.Write(e.Label);
            Output.WriteLine(":");
            Indent++;
            if (e.Statement != null)
            {
                GenerateStatement(e.Statement);
            }
        }

        protected override void GenerateLinePragmaEnd(CodeLinePragma e)
        {
            Output.WriteLine();
            Output.WriteLine("#line default");
            Output.WriteLine("#line hidden");
        }

        protected override void GenerateLinePragmaStart(CodeLinePragma e)
        {
            Output.WriteLine("");
            Output.Write("#line ");
            Output.Write(e.LineNumber);
            Output.Write(" \"");
            Output.Write(e.FileName);
            Output.Write("\"");
            Output.WriteLine("");
        }

        protected override void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c)
        {
            if (!(IsCurrentClass || IsCurrentStruct || IsCurrentInterface))
            {
                return;
            }

            if (e.CustomAttributes.Count > 0)
            {
                GenerateAttributes(e.CustomAttributes);
            }

            if (e.ReturnTypeCustomAttributes.Count > 0)
            {
                GenerateAttributes(e.ReturnTypeCustomAttributes, "return: ");
            }

            if (!IsCurrentInterface)
            {
                if (e.PrivateImplementationType == null)
                {
                    OutputMemberAccessModifier(e.Attributes);
                    OutputVTableModifier(e.Attributes);
                    OutputMemberScopeModifier(e.Attributes);
                }
            }
            else
            {
                OutputVTableModifier(e.Attributes);
            }

            OutputType(e.ReturnType);
            Output.Write(" ");
            if (e.PrivateImplementationType != null)
            {
                Output.Write(GetBaseTypeOutput(e.PrivateImplementationType));
                Output.Write(".");
            }

            OutputIdentifier(e.Name);

            OutputTypeParameters(e.TypeParameters);
            Output.Write("(");
            OutputParameters(e.Parameters);
            Output.Write(")");
            OutputTypeParameterConstraints(e.TypeParameters);
            if (!IsCurrentInterface && (e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract)
            {
                OutputStartingBrace();
                Indent++;
                GenerateStatements(e.Statements);
                CloseBlock();
            }
            else
            {
                Output.WriteLine(";");
            }
        }

        protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            GenerateMethodReferenceExpression(e.Method);
            Output.Write("(");
            OutputExpressionList(e.Parameters);
            Output.Write(")");
        }

        protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                if (e.TargetObject is CodeBinaryOperatorExpression)
                {
                    Output.Write("(");
                    GenerateExpression(e.TargetObject);
                    Output.Write(")");
                }
                else
                {
                    GenerateExpression(e.TargetObject);
                }

                Output.Write(".");
            }

            OutputIdentifier(e.MethodName);
            if (e.TypeArguments.Count > 0)
            {
                Output.Write(GetTypeArgumentsOutput(e.TypeArguments));
            }
        }

        protected override void GenerateMethodReturnStatement(CodeMethodReturnStatement e)
        {
            Output.Write("return");
            if (e.Expression != null)
            {
                Output.Write(" ");
                GenerateExpression(e.Expression);
            }

            Output.WriteLine(";");
        }

        protected override void GenerateNamespace(CodeNamespace e)
        {
            GenerateCommentStatements(e.Comments);
            GenerateNamespaceStart(e);
            if (!GetOption("MoveUsingsOutsideNamespace", false))
            {
                GenerateNamespaceImports(e);
            }

            Output.WriteLine("");
            GenerateTypes(e);
            GenerateNamespaceEnd(e);
        }

        protected override void GenerateNamespaceEnd(CodeNamespace e)
        {
            if (!string.IsNullOrEmpty(e.Name))
            {
                CloseBlock();
            }
        }

        protected override void GenerateNamespaceImport(CodeNamespaceImport e)
        {
            Output.Write("using ");
            OutputIdentifier(e.Namespace);
            Output.WriteLine(";");
        }

        protected override void GenerateNamespaceStart(CodeNamespace e)
        {
            if (!string.IsNullOrEmpty(e.Name))
            {
                Output.Write("namespace ");
                string[] names = e.Name.Split('.');
                Debug.Assert(names.Length > 0);
                OutputIdentifier(names[0]);
                for (int i = 1; i < names.Length; i++)
                {
                    Output.Write(".");
                    OutputIdentifier(names[i]);
                }

                OutputStartingBrace();
                Indent++;
            }
        }

        protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            Output.Write("new ");
            OutputType(e.CreateType);
            Output.Write("(");
            OutputExpressionList(e.Parameters);
            Output.Write(")");
        }

        protected override void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c)
        {
            if (GetOption("BlankLinesBetweenMembers", false))
            {
                Output.WriteLine();
            }

            if (!IsCurrentInterface)
            {
                OutputAccessibilityAndModifiers(e.Attributes);
            }
            OutputType(e.Type);
            Output.Write(" ");
            Output.Write(e.Name);

            if (e.GetStatements.Count == 0)
            {
                if (e.HasGet && !e.HasSet)
                {
                    Output.WriteLine(" { get; }");
                }
                else
                {
                    if (e.GetStatements.Count == 0 && e.SetStatements.Count == 0)
                    {
                        Output.WriteLine(" { get; set; }");
                    }
                }
            }
            else
            {
                Output.WriteLine();
                OpenBlock();
                Output.WriteLine("get");
                OpenBlock();
                GenerateStatements(e.GetStatements);
                CloseBlock();
                CloseBlock();
            }
        }

        protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write(".");
            }

            OutputIdentifier(e.PropertyName);
        }

        protected override void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e)
        {
            Output.Write("value");
        }

        protected override void GenerateRemoveEventStatement(CodeRemoveEventStatement e)
        {
            GenerateEventReferenceExpression(e.Event);
            Output.Write(" -= ");
            GenerateExpression(e.Listener);
            Output.WriteLine(";");
        }

        protected override void GenerateSnippetExpression(CodeSnippetExpression e)
        {
            Output.Write(e.Value);
        }

        protected override void GenerateSnippetMember(CodeSnippetTypeMember e)
        {
            Output.Write(e.Text);
        }

        protected override void GenerateThisReferenceExpression(CodeThisReferenceExpression e)
        {
            Output.Write("this");
        }

        protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e)
        {
            Output.Write("throw");
            if (e.ToThrow != null)
            {
                Output.Write(" ");
                GenerateExpression(e.ToThrow);
            }

            Output.WriteLine(";");
        }

        protected override void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e)
        {
            Output.Write("try");
            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.TryStatements);
            Indent--;
            CodeCatchClauseCollection catches = e.CatchClauses;
            if (catches.Count > 0)
            {
                IEnumerator en = catches.GetEnumerator();
                while (en.MoveNext())
                {
                    Output.Write("}");
                    if (Options.ElseOnClosing)
                    {
                        Output.Write(" ");
                    }
                    else
                    {
                        Output.WriteLine("");
                    }

                    CodeCatchClause current = (CodeCatchClause)en.Current;
                    Output.Write("catch (");
                    OutputType(current.CatchExceptionType);
                    Output.Write(" ");
                    OutputIdentifier(current.LocalName);
                    Output.Write(")");
                    OutputStartingBrace();
                    Indent++;
                    GenerateStatements(current.Statements);
                    Indent--;
                }
            }

            CodeStatementCollection finallyStatements = e.FinallyStatements;
            if (finallyStatements.Count > 0)
            {
                Output.Write("}");
                if (Options.ElseOnClosing)
                {
                    Output.Write(" ");
                }
                else
                {
                    Output.WriteLine("");
                }

                Output.Write("finally");
                OutputStartingBrace();
                Indent++;
                GenerateStatements(finallyStatements);
                Indent--;
            }

            Output.WriteLine("}");
        }

        protected override void GenerateTypeConstructor(CodeTypeConstructor e)
        {
            if (!(IsCurrentClass || IsCurrentStruct))
            {
                return;
            }

            if (e.CustomAttributes.Count > 0)
            {
                GenerateAttributes(e.CustomAttributes);
            }

            Output.Write("static ");
            Output.Write(CurrentTypeName);
            Output.Write("()");
            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.Statements);
            Indent--;
            Output.WriteLine("}");
        }

        protected override void GenerateTypeEnd(CodeTypeDeclaration e)
        {
            if (!IsCurrentDelegate)
            {
                CloseBlock();
            }
        }

        protected override void GenerateTypeStart(CodeTypeDeclaration e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                GenerateAttributes(e.CustomAttributes);
            }

            if (IsCurrentDelegate)
            {
                switch (e.TypeAttributes & TypeAttributes.VisibilityMask)
                {
                    case TypeAttributes.Public:
                        Output.Write("public ");
                        break;
                    case TypeAttributes.NotPublic:
                        Output.Write("internal ");
                        break;
                }

                CodeTypeDelegate del = (CodeTypeDelegate)e;
                Output.Write("delegate ");
                OutputType(del.ReturnType);
                Output.Write(" ");
                OutputIdentifier(e.Name);
                Output.Write("(");
                OutputParameters(del.Parameters);
                Output.WriteLine(");");
            }
            else
            {
                OutputTypeAttributes(e);
                OutputIdentifier(e.Name);
                OutputTypeParameters(e.TypeParameters);
                bool first = true;
                foreach (CodeTypeReference typeRef in e.BaseTypes)
                {
                    if (first)
                    {
                        Output.Write(" : ");
                        first = false;
                    }
                    else
                    {
                        Output.Write(", ");
                    }

                    OutputType(typeRef);
                }

                OutputTypeParameterConstraints(e.TypeParameters);
                OutputStartingBrace();
                Indent++;
            }
        }

        protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            OutputTypeNamePair(e.Type, e.Name);
            if (e.InitExpression != null)
            {
                Output.Write(" = ");
                GenerateExpression(e.InitExpression);
            }

            if (!generatingForLoop)
            {
                Output.WriteLine(";");
            }
        }

        protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e)
        {
            OutputIdentifier(e.VariableName);
        }

        protected override string GetTypeOutput(CodeTypeReference value)
        {
            string s = string.Empty;
            CodeTypeReference baseTypeRef = value;
            while (baseTypeRef.ArrayElementType != null)
            {
                baseTypeRef = baseTypeRef.ArrayElementType;
            }

            s += GetBaseTypeOutput(baseTypeRef);
            while (value != null && value.ArrayRank > 0)
            {
                char[] results = new char[value.ArrayRank + 1];
                results[0] = '[';
                results[value.ArrayRank] = ']';
                for (int i = 1; i < value.ArrayRank; i++)
                {
                    results[i] = ',';
                }

                s += new string(results);
                value = value.ArrayElementType;
            }

            return s;
        }

        protected override bool IsValidIdentifier(string value)
        {
            if (IsKeyword(value))
            {
                return false;
            }

            return Regex.IsMatch(value, @"[A-Za-z_][A-Za-z0-9_]*");
        }

        protected void OpenBlock()
        {
            Output.WriteLine("{");
            Indent++;
        }

        protected override void OutputAttributeDeclarations(CodeAttributeDeclarationCollection attributes)
        {
            GenerateAttributes(attributes);
        }

        protected override void OutputFieldScopeModifier(MemberAttributes attributes)
        {
            OutputVTableModifier(attributes);

            switch (attributes & MemberAttributes.ScopeMask)
            {
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Const:
                    Output.Write("const ");
                    break;
            }
        }

        protected override void OutputIdentifier(string ident)
        {
            Output.Write(CreateEscapedIdentifier(ident));
        }

        protected override void OutputMemberAccessModifier(MemberAttributes attributes)
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
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
            }
        }

        protected override void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            if ((attributes & MemberAttributes.VTableMask) == MemberAttributes.New)
            {
                Output.Write("new ");
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
                        case MemberAttributes.Assembly:
                            Output.Write("virtual ");
                            return;
                        default:
                            return;
                    }
            }
        }

        protected override void OutputType(CodeTypeReference typeRef)
        {
            Output.Write(GetTypeOutput(typeRef));
        }

        protected virtual void OutputTypeParameterConstraints(CodeTypeParameterCollection typeParameters)
        {
            if (typeParameters.Count == 0)
            {
                return;
            }

            for (int i = 0; i < typeParameters.Count; i++)
            {
                Output.WriteLine();
                Indent++;
                bool first = true;
                if (typeParameters[i].Constraints.Count > 0)
                {
                    foreach (CodeTypeReference typeRef in typeParameters[i].Constraints)
                    {
                        if (first)
                        {
                            Output.Write("where ");
                            Output.Write(typeParameters[i].Name);
                            Output.Write(" : ");
                            first = false;
                        }
                        else
                        {
                            Output.Write(", ");
                        }

                        OutputType(typeRef);
                    }
                }

                if (typeParameters[i].HasConstructorConstraint)
                {
                    if (first)
                    {
                        Output.Write("where ");
                        Output.Write(typeParameters[i].Name);
                        Output.Write(" : new()");
                    }
                    else
                    {
                        Output.Write(", new ()");
                    }
                }

                Indent--;
            }
        }

        protected virtual void OutputTypeParameters(CodeTypeParameterCollection typeParameters)
        {
            if (typeParameters.Count == 0)
            {
                return;
            }

            Output.Write('<');
            bool first = true;
            for (int i = 0; i < typeParameters.Count; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Output.Write(", ");
                }

                if (typeParameters[i].CustomAttributes.Count > 0)
                {
                    GenerateAttributes(typeParameters[i].CustomAttributes, null, true);
                    Output.Write(' ');
                }

                Output.Write(typeParameters[i].Name);
            }

            Output.Write('>');
        }

        protected override string QuoteSnippetString(string value)
        {
            return '"' + value + '"';
        }

        protected override bool Supports(GeneratorSupport support)
        {
            return true;
        }

        protected virtual void WriteAutoGeneratedHeader()
        {
            Output.WriteLine("//------------------------------------------------------------------------------");
            Output.WriteLine("// <auto-generated>");
            Output.WriteLine("//     This code was automatically generated by a tool. Do not make changes to");
            Output.WriteLine("//     the code in this file. All chages are eventually overwritten.");
            Output.WriteLine("// </auto-generated>");
            Output.WriteLine("//------------------------------------------------------------------------------");
            Output.WriteLine();
        }

        private static SortedList GetImportList(CodeCompileUnit e)
        {
            SortedList importList = new SortedList(StringComparer.Ordinal);
            foreach (CodeNamespace nspace in e.Namespaces)
            {
                if (!string.IsNullOrEmpty(nspace.Name))
                {
                    nspace.UserData["GenerateImports"] = false;

                    foreach (CodeNamespaceImport import in nspace.Imports)
                    {
                        if (!importList.Contains(import.Namespace))
                        {
                            importList.Add(import.Namespace, import.Namespace);
                        }
                    }
                }
            }

            return importList;
        }

        private static bool IsKeyword(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            switch (value)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "var":
                case "virtual":
                case "void":
                case "volatile":
                case "while":
                    return true;
                default: return false;
            }
        }

        private static bool IsPrefixTwoUnderscore(string value)
        {
            if (value.Length < 3 || value[0] != '_' || value[1] != '_')
            {
                return false;
            }

            return value[2] != '_';
        }

        private static bool IsStatic(CodeTypeDeclaration e)
        {
            var result = e.UserData["IsStatic"];
            if (result != null && result is bool boolean)
            {
                return boolean;
            }

            return false;
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix = null, bool inLine = false)
        {
            if (attributes.Count == 0)
            {
                return;
            }

            IEnumerator en = attributes.GetEnumerator();
            bool paramArray = false;
            while (en.MoveNext())
            {
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

        private void GenerateDelegate(CodeTypeDelegate e)
        {
            switch (e.TypeAttributes & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.Public:
                    Output.Write("public ");
                    break;
                case TypeAttributes.NotPublic:
                    Output.Write("internal ");
                    break;
            }

            CodeTypeDelegate del = e;
            Output.Write("delegate ");
            OutputType(del.ReturnType);
            Output.Write(" ");
            OutputIdentifier(e.Name);
            Output.Write("(");
            OutputParameters(del.Parameters);
            Output.WriteLine(");");
        }

        private void GenerateExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems)
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
                        Output.WriteLine(',');
                    }
                    else
                    {
                        Output.Write(", ");
                    }
                }
                GenerateExpression((CodeExpression)en.Current);
            }

            Indent--;
        }

        private string GetBaseTypeOutput(CodeTypeReference typeRef)
        {
            string s = typeRef.BaseType;
            if (s.Length == 0)
            {
                s = "void";
                return s;
            }

            string lowerCaseString = s.ToLower(CultureInfo.InvariantCulture).Trim();
            switch (lowerCaseString)
            {
                case "system.int16":
                    s = "short";
                    break;
                case "system.int32":
                    s = "int";
                    break;
                case "system.int64":
                    s = "long";
                    break;
                case "system.string":
                    s = "string";
                    break;
                case "system.object":
                    s = "object";
                    break;
                case "system.boolean":
                    s = "bool";
                    break;
                case "system.void":
                    s = "void";
                    break;
                case "system.char":
                    s = "char";
                    break;
                case "system.byte":
                    s = "byte";
                    break;
                case "system.uint16":
                    s = "ushort";
                    break;
                case "system.uint32":
                    s = "uint";
                    break;
                case "system.uint64":
                    s = "ulong";
                    break;
                case "system.sbyte":
                    s = "sbyte";
                    break;
                case "system.single":
                    s = "float";
                    break;
                case "system.double":
                    s = "double";
                    break;
                case "system.decimal":
                    s = "decimal";
                    break;
                default:

                    StringBuilder sb = new StringBuilder(s.Length + 10);
                    if ((typeRef.Options & CodeTypeReferenceOptions.GlobalReference) != 0)
                    {
                        sb.Append("global::");
                    }

                    string baseType = typeRef.BaseType;
                    int lastIndex = 0;
                    int currentTypeArgStart = 0;
                    for (int i = 0; i < baseType.Length; i++)
                    {
                        switch (baseType[i])
                        {
                            case '+':
                            case '.':
                                sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                                sb.Append('.');
                                i++;
                                lastIndex = i;
                                break;
                            case '`':
                                sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                                i++;
                                int numTypeArgs = 0;
                                while (i < baseType.Length && baseType[i] >= '0' && baseType[i] <= '9')
                                {
                                    numTypeArgs = numTypeArgs * 10 + (baseType[i] - '0');
                                    i++;
                                }

                                GetTypeArgumentsOutput(typeRef.TypeArguments, currentTypeArgStart, numTypeArgs, sb);
                                currentTypeArgStart += numTypeArgs;

                                if (i < baseType.Length && (baseType[i] == '+' || baseType[i] == '.'))
                                {
                                    sb.Append('.');
                                    i++;
                                }

                                lastIndex = i;
                                break;
                        }
                    }

                    if (lastIndex < baseType.Length)
                    {
                        sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex)));
                    }

                    return sb.ToString();
            }

            return s;
        }

        private T GetOption<T>(string name, T defaultValue)
        {
            object o = Options[name];
            if (o != null && o is T option)
            {
                return option;
            }


            if (CSharpOptions != null && o is T oo)
            {
                return oo;
            }

            return defaultValue;
        }

        private string GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments)
        {
            StringBuilder sb = new StringBuilder(128);
            GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
        }

        private void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
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

                if (i < typeArguments.Count)
                {
                    sb.Append(GetTypeOutput(typeArguments[i]));
                }
            }

            sb.Append('>');
        }

        private void OutputAccessibilityAndModifiers(MemberAttributes attributes)
        {
            OutputMemberAccessModifier(attributes);
            OutputVTableModifier(attributes);
            OutputMemberScopeModifier(attributes);
        }

        private void OutputStartingBrace()
        {
            if (Options.BracingStyle == "C")
            {
                Output.WriteLine("");
                Output.WriteLine("{");
            }
            else
            {
                Output.WriteLine(" {");
            }
        }

        private void OutputTabs()
        {
            for (int index = 0; index < Indent; ++index)
            {
                Output.Write(Options.IndentString);
            }
        }

        private void OutputTypeAttributes(CodeTypeDeclaration e)
        {
            if ((e.Attributes & MemberAttributes.New) != 0)
            {
                Output.Write("new ");
            }

            TypeAttributes attributes = e.TypeAttributes;
            switch (attributes & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    Output.Write("public ");
                    break;
                case TypeAttributes.NestedPrivate:
                    Output.Write("private ");
                    break;
                case TypeAttributes.NestedFamily:
                    Output.Write("protected ");
                    break;
                case TypeAttributes.NotPublic:
                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                    Output.Write("internal ");
                    break;
                case TypeAttributes.NestedFamORAssem:
                    Output.Write("protected internal ");
                    break;
            }

            if (e.IsStruct)
            {
                if (e.IsPartial)
                {
                    Output.Write("partial ");
                }

                Output.Write("struct ");
            }
            else if (e.IsEnum)
            {
                Output.Write("enum ");
            }
            else if (e.IsInterface)
            {
                Output.Write("interface ");
            }
            else
            {
                switch (attributes & TypeAttributes.ClassSemanticsMask)
                {
                    case TypeAttributes.Class:

                        if (IsStatic(e))
                        {
                            Output.Write("static ");
                        }
                        else
                        {
                            if ((attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                            {
                                Output.Write("sealed ");
                            }

                            if ((attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
                            {
                                Output.Write("abstract ");
                            }
                        }



                        if (e.IsPartial)
                        {
                            Output.Write("partial ");
                        }

                        Output.Write("class ");
                        break;
                    case TypeAttributes.Interface:
                        if (e.IsPartial)
                        {
                            Output.Write("partial ");
                        }

                        Output.Write("interface ");
                        break;
                }
            }
        }

        private void OutputVTableModifier(MemberAttributes attributes)
        {
            switch (attributes & MemberAttributes.VTableMask)
            {
                case MemberAttributes.New:
                    Output.Write("new ");
                    break;
            }
        }

        private void SetOptions(CSharpCodeGeneratorOptions generatorOptions)
        {
            CSharpOptions = generatorOptions;

        }
    }
}
