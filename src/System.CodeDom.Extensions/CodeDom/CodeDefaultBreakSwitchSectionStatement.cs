// -----------------------------------------------------------------------
// <copyright file="CodeDefaultBreakSwitchSectionStatement.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeDefaultBreakSwitchSectionStatement" />
    /// </summary>
    public class CodeDefaultBreakSwitchSectionStatement : CodeSwitchSectionStatement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDefaultBreakSwitchSectionStatement"/> class.
        /// </summary>
        public CodeDefaultBreakSwitchSectionStatement()
        {
            Label = new CodeSwitchSectionLabelExpression(new CodeVariableReferenceExpression("default"));
            BodyStatements = new CodeStatementCollection();
        }

        /// <summary>
        /// Gets or sets the BodyStatements
        /// </summary>
        public CodeStatementCollection BodyStatements { get; set; }

        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        public override CodeSwitchSectionLabelExpression Label { get; set; }
    }
}
