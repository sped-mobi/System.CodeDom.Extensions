// -----------------------------------------------------------------------
// <copyright file="CodeSwitchSectionStatement.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeSwitchSectionStatement" />
    /// </summary>
    public abstract class CodeSwitchSectionStatement : CodeStatement
    {
        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        public abstract CodeSwitchSectionLabelExpression Label { get; set; }
    }
}
