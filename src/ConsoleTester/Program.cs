using System;
using System.CodeDom;
using System.CodeDom.Compiler;
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
                                CustomAttributes =
                                {
                                    new CodeAttributeDeclaration
                                    {
                                        Name = "MyAttribute"
                                    }
                                },
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
                                    new CodeConstructor
                                    {
                                        Name = "MyClass",
                                        Comments =
                                        {
                                            new CodeCommentStatement("This is a doc comment on a constructor.", true)
                                        },
                                        Attributes = MemberAttributes.Public,
                                        BaseConstructorArgs =
                                        {
                                            new CodePrimitiveExpression(false)
                                        },
                                        Parameters =
                                        {
                                            new CodeParameterDeclarationExpression
                                            {
                                                
                                                Name = "name",
                                                Type = new CodeTypeReference(typeof(string)),
                                                
                                            }
                                        },
                                        CustomAttributes =
                                        {
                                            new CodeAttributeDeclaration
                                            {
                                                Name = "MyAttribute"
                                            }
                                        },
                                    },
                                    new CodeMemberMethod
                                    {
                                        Name = "MyMethod",
                                        ReturnType = new CodeTypeReference(typeof(void)),
                                        Parameters =
                                        {
                                            new CodeParameterDeclarationExpression
                                            {

                                                Name = "name",
                                                Type = new CodeTypeReference(typeof(string)),

                                            }
                                        },
                                        CustomAttributes =
                                        {
                                            new CodeAttributeDeclaration
                                            {
                                                Name = "MyAttribute"
                                            }
                                        },
                                        Comments =
                                        {
                                            new CodeCommentStatement("This is a doc comment on a method.", true)
                                        },
                                    },
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
                                        },
                                        CustomAttributes =
                                        {
                                            new CodeAttributeDeclaration
                                            {
                                                Name = "MyAttribute"
                                            }
                                        },
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
                generator.MoveUsingsOutsideNamespace = true;
                generator.MultiLineDocComments = true;

                CodeGeneratorOptions options = new CodeGeneratorOptions
                {
                    BracingStyle = "C",
                    BlankLinesBetweenMembers = false,
                    IndentString = "    "
                };

                generator.GenerateCodeFromCompileUnit(unit, writer, options);
            }

            Console.Write(builder);

            Console.ReadKey();
        }
    }
}
