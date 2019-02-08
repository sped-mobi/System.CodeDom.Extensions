// -----------------------------------------------------------------------
// <copyright file="CodeSwitchSectionStatementCollection.cs" company="Ollon, LLC">
//     Copyright (c) 2017 Ollon, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace System.CodeDom
{
    /// <summary>
    /// Defines the <see cref="CodeSwitchSectionStatementCollection" />
    /// </summary>
    public class CodeSwitchSectionStatementCollection : List<CodeSwitchSectionStatement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSwitchSectionStatementCollection"/> class.
        /// </summary>
        public CodeSwitchSectionStatementCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSwitchSectionStatementCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection<see cref="IEnumerable{CodeSwitchSectionStatement}"/></param>
        public CodeSwitchSectionStatementCollection(IEnumerable<CodeSwitchSectionStatement> collection) : base(collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeSwitchSectionStatementCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity<see cref="int"/></param>
        public CodeSwitchSectionStatementCollection(int capacity) : base(capacity)
        {
        }
    }
}
