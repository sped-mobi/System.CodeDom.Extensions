// -----------------------------------------------------------------------
// <copyright file="CodeFallThroughSwitchSectionStatement.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeFallThroughSwitchSectionStatement" />
    /// </summary>
    public class CodeFallThroughSwitchSectionStatement : CodeSwitchSectionStatement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFallThroughSwitchSectionStatement"/> class.
        /// </summary>
        public CodeFallThroughSwitchSectionStatement()
        {
            Label = new CodeSwitchSectionLabelExpression();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFallThroughSwitchSectionStatement"/> class.
        /// </summary>
        /// <param name="label">The label<see cref="CodeSwitchSectionLabelExpression"/></param>
        public CodeFallThroughSwitchSectionStatement(CodeSwitchSectionLabelExpression label)
        {
            Label = label;
        }

        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        public override CodeSwitchSectionLabelExpression Label { get; set; }
    }
}
