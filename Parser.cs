using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace VisualiazdorLogica
{
    public interface ILogicParser
    {
        INode Parse(string input);
    }

    public class DotNetParser : ILogicParser
    {
        public INode Parse(string input)
        {
            if (input.Length == 0)
                throw new ArgumentException("El nodo debe tener uno o más caracteres");

            // Prepare the input
            input = Util.Sanitize(input);

            bool hadParenthesis = input.RemoveOuterParenthesis(out input);

            // The string only contains special characters
            if (input.Strip(Characters.Special).Length == 0)
                throw new ArgumentException("El nodo solo contiene operadores");

            // No special characters at all, must be a literal?
            if (input.All(c => Array.IndexOf(Characters.Special, c) == -1))
            {
                if (hadParenthesis) throw new Exception("Variables o literales no pueden llevar paréntesis");

                // It's a literal true/false
                if (input == "T" || input == "F")
                {
                   return new LiteralNode(input == "T");
                }
                else
                {
                    // Then it's a variable node, like 'p', 'q', etc
                    return new VariableNode(input);
                }
            }

            // Assume it's a binary/Not node

            // .NET doesnt support recursive regex :(

            // Input: (q ^ (r V p)) <> (q ^ ~r)
            // Output: left: (q ^ (r V p)), right: (q ^ ~r), op: <>
            
            // Input: q ^ r -> r
            // Output: left: q ^ r, right: r, op: ->

            int level = 0;
            int levelStart = 0;
            List<string> simplifiedInput = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == Characters.LeftParent)
                {
                    if (level == 0) levelStart = i;
                    level++;
                }
                else if (input[i] == Characters.RightParent)
                {
                    level--;

                    if (level == 0)
                    {
                        simplifiedInput.Add(input.Substring(levelStart, i - levelStart));
                        levelStart = i;
                    }
                    else if (level == -1)
                    {
                        throw new FormatException("Extra ')' en posición " + i);
                    }
                }
                else if (Array.IndexOf(Characters.Operators, input[i]) != -1 && level == 0)
                {
                    var left = input.Substring(levelStart, i - levelStart);
                    if (!string.IsNullOrEmpty(left))
                        simplifiedInput.Add(left);
                    simplifiedInput.Add(input[i].ToString());
                    levelStart = i + 1;
                }
            }

            simplifiedInput.Add(input.Substring(levelStart, input.Length - levelStart));

            if (level != 0)
            {
                throw new FormatException("Falta paréntesis derecho");
            }

            // Ordenar entradas por el convenio de precedencia.
            for (int i = 0; i < Characters.Operators.Length; i++)
            {
                int position;
                if ((position = simplifiedInput.IndexOf(Characters.Operators[i].ToString())) != -1)
                {
                    if (simplifiedInput[position][0] == Characters.Not)
                    {
                        // El operador NOT solo puede estar delante del otro operador NOT.
                        if (position > 0)
                            throw new Exception("Error sintáctico del operador negación");

                        return new NotNode(Parse(string.Join("",
                                        simplifiedInput.GetRange(position + 1, simplifiedInput.Count - position - 1))));
                    }

                    if (simplifiedInput.Count(txt => txt.Length == 1 &&
                        Characters.GetLevel(txt[0]) == Characters.GetLevel(simplifiedInput[position][0])) > 1)
                    {
                        throw new FormatException("Ambiguo operador: " + simplifiedInput[position]);
                    }

                    IBinaryOperator binop = null;

                    // Determine the operator
                    switch (simplifiedInput[position][0])
                    {
                        case Characters.Biconditional:
                            binop = new BiconditionalOperation();
                            break;
                        case Characters.Conditional:
                            binop = new ConditionalOperation();
                            break;
                        case Characters.And:
                            binop = new AndOperation();
                            break;
                        case Characters.Or:
                            binop = new OrOperation();
                            break;
                    }

                    // return the operator node with everything to it's left as the first child
                    // and everything to it's right as the second child
                    return new BinaryNode(binop,
                                Parse(string.Join("", simplifiedInput.GetRange(0, position))),
                                Parse(string.Join("", simplifiedInput.GetRange(position + 1, simplifiedInput.Count - position - 1))));
                }
            }

            throw new Exception("Operator no encondrado"); // this should never happen
        }
    }
}