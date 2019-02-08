using System;
using System.CodeDom;
using System.CodeDom.CSharp;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace ConsoleTester
{
    public static class Program
    {
        public static void Main()
        {
            CodeCompileUnit unit = new CodeCompileUnit
            {
                Namespaces =
                {
                    new CodeNamespace
                    {
                        Name = "MyNamespace.Name",
                        Imports =
                        {
                            new CodeNamespaceImport("System"),
                            new CodeNamespaceImport("System.Collections"),
                            new CodeNamespaceImport("System.Collections.Generic")
                        },
                        UserData = { ["GenerateImports"] = true },
                        Types =
                        {
                            new CodeTypeDeclaration
                            {
                                IsPartial = true,
                                IsClass = true,
                                IsEnum = false,
                                IsStruct = false,
                                IsInterface = false,
                                TypeAttributes = TypeAttributes.Public | TypeAttributes.Abstract,
                                Name = "MyClass",
                                UserData =
                                {
                                    ["IsStatic"] = true
                                },
                                Comments =
                                {
                                    new CodeCommentStatement("This is a doc comment on a class.", true)
                                },
                                Members =
                                {
                                    new CodeMemberProperty
                                    {
                                        Name = "Name",
                                        Attributes = MemberAttributes.Public,
                                        Type = new CodeTypeReference(typeof(string)),
                                        HasGet = true,
                                        HasSet = false,
                                        Comments =
                                        {
                                            new CodeCommentStatement("This is a doc comment on a property.", true)
                                        }
                                    },
                                    new CodeMemberProperty
                                    {
                                        Name = "Description",
                                        Attributes = MemberAttributes.Public,
                                        Type = new CodeTypeReference(typeof(string)),
                                        HasGet = true,
                                        HasSet = false,
                                        Comments =
                                        {
                                            new CodeCommentStatement("This is a doc comment on a property.", true)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            StringBuilder builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                CSharpCodeProvider provider = new CSharpCodeProvider();
                CSharpCodeGenerator generator = new CSharpCodeGenerator();

                CSharpCodeGeneratorOptions options = new CSharpCodeGeneratorOptions
                {
                    BracingStyle = "C",
                    BlankLinesBetweenMembers = false,
                    IndentString = "    ",
                    MultilineDocComments = true,
                    MoveUsingsOutsideNamespace = true
                };

                generator.GenerateCodeFromCompileUnit(unit, writer, options);
            }

            Console.Write(builder);

            Console.ReadKey();
        }
    }
}
