// -----------------------------------------------------------------------
// <copyright file="CodeDefaultReturnSwitchSectionStatement.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeDefaultReturnSwitchSectionStatement" />
    /// </summary>
    public class CodeDefaultReturnSwitchSectionStatement : CodeSwitchSectionStatement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeDefaultReturnSwitchSectionStatement"/> class.
        /// </summary>
        public CodeDefaultReturnSwitchSectionStatement()
        {
            Label = new CodeSwitchSectionLabelExpression(new CodeVariableReferenceExpression("default"));
            BodyStatements = new CodeStatementCollection();
            ReturnStatement = new CodeMethodReturnStatement();
        }

        /// <summary>
        /// Gets or sets the BodyStatements
        /// </summary>
        public CodeStatementCollection BodyStatements { get; set; }

        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        public override CodeSwitchSectionLabelExpression Label { get; set; }

        /// <summary>
        /// Gets or sets the ReturnStatement
        /// </summary>
        public CodeMethodReturnStatement ReturnStatement { get; set; }
    }
}
