// -----------------------------------------------------------------------
// <copyright file="CSharpCodeGeneratorOptions.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.CodeDom.Compiler;

namespace System.CodeDom.CSharp
{
    public class CSharpCodeGeneratorOptions : CodeGeneratorOptions
    {
        public new string IndentString { get; set; }


        public bool MultilineDocComments
        {
            get
            {
                return (bool) base["MultilineDocComments"];
            }
            set
            {
                base["MultilineDocComments"] = value;
            }
        }

        public bool MoveUsingsOutsideNamespace
        {
            get
            {
                return (bool)base["MoveUsingsOutsideNamespace"];
            }
            set
            {
                base["MoveUsingsOutsideNamespace"] = value;
            }
        }
    }
}
