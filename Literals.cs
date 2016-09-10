using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VisualiazdorLogica
{
    public static class LiteralVariables
    {
        public static Dictionary<string, bool> Map = new Dictionary<string, bool>();

        public static void SetValues(Dictionary<string, bool> values)
        {
            foreach (var pair in values)
            {
                Set(pair.Key, pair.Value);
            }
        }

        public static void Add(string key, bool value)
        {
            if (!Map.ContainsKey(key)) Map.Add(key, value);
        }

        public static void Set(string key, bool value)
        {
            if (Map.ContainsKey(key))
                Map[key] = value;
            else Map.Add(key, value);
        }

        public static bool Get(string key)
        {
            //if (_map.ContainsKey(key))
            // Force throw exception

            return Map[key];
        }
    }

    public static class Util
    {
        public static string Prettify(string node)
        {
            node = Sanitize(node);
            RemoveOuterParenthesis(node, out node);

            return node;
        }

        public static string Sanitize(string input, bool removeWhitespace = true)
        {
            string output = input;

            if (removeWhitespace)
                output = Regex.Replace(input, @"\s", ""); // No whitespace

            output = Regex.Replace(output, "<>", "↔"); // Bicondicional
            output = Regex.Replace(output, ">", "→"); // Condicional
            output = Regex.Replace(output, @"\^", "∧"); // Conjunción
            output = Regex.Replace(output, @"V", "⋁"); // Disyunción
            output = Regex.Replace(output, @"~", "¬"); // Negación

            return output;
        }

        public static string Strip(this string value, char[] characters)
        {
            string output = value;
            for (int i = 0; i < characters.Length; i++)
            {
                output = output.Replace(characters[i]+"", "");
            }
            return output;
        }

        // Returns whether it had semantic parenthesis.
        public static bool RemoveOuterParenthesis(this string value, out string output)
        {
            output = value;

            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value[0] != Characters.LeftParent || value[value.Length - 1] != Characters.RightParent) return false;

            int level = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == Characters.LeftParent)
                {
                    level++;
                }
                else if (value[i] == Characters.RightParent)
                {
                    level--;

                    // The outer parenthesis belong to a group inside
                    // e.g. (p^q) V (r^s)
                    if (level == 0 && i != value.Length - 1)
                        return false;
                }
            }

            output = value.Substring(1, value.Length - 2);
            return true;
        }
    }

    public static class Characters
    {
        public const char Biconditional = '↔';
        public const char Conditional = '→';
        public const char And = '∧';
        public const char Or = '⋁';
        public const char Not = '¬';
        public const char RightParent = ')';
        public const char LeftParent = '(';

        public const char True = 'T';
        public const char False = 'F';

        // Ordenado por el Convenio de Precedencia
        public static readonly char[] Operators = new[]
        {
            Biconditional,
            Conditional,
            And,
            Or,
            Not,
        };

        // Ordenado por el Convenio de Precedencia
        public static readonly char[] Special = new []
        {
            Biconditional,
            Conditional,
            And,
            Or,
            Not,
            RightParent,
            LeftParent
        };
    }

}