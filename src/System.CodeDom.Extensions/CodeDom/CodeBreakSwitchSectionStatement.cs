// -----------------------------------------------------------------------
// <copyright file="CodeBreakSwitchSectionStatement.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeBreakSwitchSectionStatement" />
    /// </summary>
    public class CodeBreakSwitchSectionStatement : CodeSwitchSectionStatement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBreakSwitchSectionStatement"/> class.
        /// </summary>
        public CodeBreakSwitchSectionStatement()
        {
            Label = new CodeSwitchSectionLabelExpression();
            BodyStatements = new CodeStatementCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBreakSwitchSectionStatement"/> class.
        /// </summary>
        /// <param name="bodyStatements">The bodyStatements<see cref="CodeStatementCollection"/></param>
        /// <param name="label">The label<see cref="CodeSwitchSectionLabelExpression"/></param>
        public CodeBreakSwitchSectionStatement(CodeStatementCollection bodyStatements, CodeSwitchSectionLabelExpression label)
        {
            BodyStatements = bodyStatements;
            Label = label;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeBreakSwitchSectionStatement"/> class.
        /// </summary>
        /// <param name="label">The label<see cref="CodeSwitchSectionLabelExpression"/></param>
        /// <param name="bodyStatements">The bodyStatements<see cref="CodeStatement[]"/></param>
        public CodeBreakSwitchSectionStatement(CodeSwitchSectionLabelExpression label, params CodeStatement[] bodyStatements)
        {
            Label = label;
            BodyStatements = bodyStatements == null ? new CodeStatementCollection() : new CodeStatementCollection(bodyStatements);
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
