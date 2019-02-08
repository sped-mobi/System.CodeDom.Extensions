// -----------------------------------------------------------------------
// <copyright file="CodeReturnValueSwitchSectionStatement.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeReturnValueSwitchSectionStatement" />
    /// </summary>
    public class CodeReturnValueSwitchSectionStatement : CodeSwitchSectionStatement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeReturnValueSwitchSectionStatement"/> class.
        /// </summary>
        public CodeReturnValueSwitchSectionStatement()
        {
            Label = new CodeSwitchSectionLabelExpression();
            ReturnStatement = new CodeMethodReturnStatement();
            BodyStatements = new CodeStatementCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeReturnValueSwitchSectionStatement"/> class.
        /// </summary>
        /// <param name="label">The label<see cref="CodeSwitchSectionLabelExpression"/></param>
        /// <param name="returnStatement">The returnStatement<see cref="CodeMethodReturnStatement"/></param>
        /// <param name="bodyStatements">The bodyStatements<see cref="CodeStatement[]"/></param>
        public CodeReturnValueSwitchSectionStatement(CodeSwitchSectionLabelExpression label,
            CodeMethodReturnStatement returnStatement,
            params CodeStatement[] bodyStatements)
        {
            Label = label;
            BodyStatements = bodyStatements == null ? new CodeStatementCollection() : new CodeStatementCollection(bodyStatements);
            ReturnStatement = returnStatement;
        }

        /// <summary>
        /// Gets or sets the BodyStatements
        /// </summary>
        public CodeStatementCollection BodyStatements { get; set; }

        /// <summary>
        /// Gets a value indicating whether HasBody
        /// </summary>
        public bool HasBody => BodyStatements.Count > 0;

        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        public override CodeSwitchSectionLabelExpression Label { get; set; }

        /// <summary>
        /// Gets or sets the ReturnStatement
        /// </summary>
        public CodeMethodReturnStatement ReturnStatement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SingleLine
        /// </summary>
        public bool SingleLine { get; set; }
    }
}
