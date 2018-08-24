using System.Collections;
using System.Text;

namespace EarleCode.Compiling.Earle.AST
{
    internal static class StringBuilderExtensions
    {
        public static void AppendLines(this StringBuilder sb, IEnumerable lines)
        {
            if (lines == null)
                return;

            foreach (var line in lines)
                sb.AppendLine(line.ToString());
        }

        public static void AppendList(this StringBuilder sb, IEnumerable values)
        {
            if (values == null)
                return;

            var first = true;
            foreach (var value in values)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");
                
                sb.Append(value);
            }
        }
    }
}