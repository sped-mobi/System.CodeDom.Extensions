// -----------------------------------------------------------------------
// <copyright file="CodeSwitchStatement.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeSwitchStatement" />
    /// </summary>
    public class CodeSwitchStatement : CodeStatement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSwitchStatement"/> class.
        /// </summary>
        public CodeSwitchStatement()
        {
            CheckExpression = new CodeExpression();
            Sections = new CodeSwitchSectionStatementCollection();
        }

        /// <summary>
        /// Gets or sets the CheckExpression
        /// </summary>
        public CodeExpression CheckExpression { get; set; }

        /// <summary>
        /// Gets the Sections
        /// </summary>
        public CodeSwitchSectionStatementCollection Sections { get; }
    }
}
