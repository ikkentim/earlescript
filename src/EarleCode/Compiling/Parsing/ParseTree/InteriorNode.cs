// EarleCode
// Copyright 2017 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Compiling.Parsing.ParseTree
{
    /// <summary>
    ///     Reprents an interior parse tree node.
    /// </summary>
    public class InteriorNode : IInteriorNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InteriorNode" /> class.
        /// </summary>
        /// <param name="rule">The production rule symbol the node represents.</param>
        /// <param name="children">The children of the node.</param>
        public InteriorNode(string rule, IReadOnlyList<INode> children)
        {
            Rule = rule;
            Children = children;
        }

        #region Implementation of IAbstractSyntaxTreeInteriorNode

        /// <summary>
        ///     Gets the production rule symbol this node represents.
        /// </summary>
        public string Rule { get; }

        /// <summary>
        ///     Gets a collection of child nodes of this node.
        /// </summary>
        public IReadOnlyList<INode> Children { get; }

        #endregion
		
	    #region Implementation of INode

	    /// <summary>
	    /// Returns a string representing this node as a tree.
	    /// </summary>
	    /// <returns>A string representing this node as a tree.</returns>
	    public string ToTreeString()
	    {
		    var children = Children.Select(c => c.ToTreeString().Split('\n').ToList()).ToArray();

		    // Need at least 1 child to use as filler
		    if (children.Length == 0)
			    children = new[] {new List<string>(new[] {" "})};

		    var height = children.Max(s => s.Count);
		    var width = 0;
		    var result = new List<string>();

			// Construct lines of children
		    foreach (var child in children)
		    {
			    var childWidth = child.Max(c => c.Length);
			    var addLines = height - child.Count;
			    child.AddRange(Enumerable.Repeat(string.Concat(Enumerable.Repeat(" ", childWidth)), addLines));
		    }

			// Join rows of children into result
		    for (var i = 0; i < height; i++)
		    {
			    var row = string.Join(" | ", children.Select(c => c[i]));

			    if (row.Length > width)
				    width = row.Length;

			    result.Add(row);
		    }

		    var title = Rule;
		    var space = width - title.Length;

		    if (space < 0)
		    {
			    var addition = string.Concat(Enumerable.Repeat(" ", -space));
			    width += -space;
			    space = 0;

			    for (var i = 0; i < result.Count; i++)
				    result[i] += addition;
		    }

		    var halfSpace = space / 2;
		    var halfSpaceRemainder = space - halfSpace * 2;

		    var head = string.Concat(Enumerable.Repeat(" ", halfSpace));
			var tail = head + string.Concat(Enumerable.Repeat(" ", halfSpaceRemainder));
		    title = head + title + tail;
		    var line = string.Concat(Enumerable.Repeat("-", width));

		    return title + "\n" + line + "\n" + string.Join("\n", result);
	    }

	    #endregion

        public override string ToString()
        {
            return string.Join(" ", Children);
        }
    }
}