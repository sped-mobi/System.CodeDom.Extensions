// -----------------------------------------------------------------------
// <copyright file="CodeSwitchSectionLabelExpression.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeSwitchSectionLabelExpression" />
    /// </summary>
    public class CodeSwitchSectionLabelExpression : CodeExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSwitchSectionLabelExpression"/> class.
        /// </summary>
        public CodeSwitchSectionLabelExpression()
        {
            Expression = new CodeExpression();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSwitchSectionLabelExpression"/> class.
        /// </summary>
        /// <param name="expression">The expression<see cref="CodeExpression"/></param>
        public CodeSwitchSectionLabelExpression(CodeExpression expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Gets or sets the Expression
        /// </summary>
        public CodeExpression Expression { get; set; }
    }
}
