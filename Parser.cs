using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualiazdorLogica
{
    // Interfaz para un parser.
    // Tenia pensado tener varios, como
    // por ejemplo regex, pero regex en
    // .NET no tiene soporte para recursion.
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

            // Preparamos la entrada
            input = Util.Sanitize(input);

            // Guardamos si tenía parentesis. Es importante si el nodo es un literal, ya que no puede llevar
            // parentesis.
            bool hadParenthesis = input.RemoveOuterParenthesis(out input);

            // La entrada solo contiene caracteres especiales
            if (input.Strip(Characters.Special).Length == 0)
                throw new ArgumentException("El nodo solo contiene operadores");

            // No hay caracteres especiales, tiene que ser un literal?
            if (input.All(c => Array.IndexOf(Characters.Special, c) == -1))
            {
                if (hadParenthesis) throw new Exception("Variables o literales no pueden llevar paréntesis");

                // Es un literal verdadero/falso
                if (input == "T" || input == "F")
                {
                   return new LiteralNode(input == "T");
                }
                else
                {
                    // En otro case es un nodo de variable, como 'p', 'q', etc.
                    return new VariableNode(input);
                }
            }

            // Asumimos es un nodo binario o un nodo NOT
            
            // Simplificamos la entrada e ignoramos los niveles inferiores.
            // Solo tenemos en cuenta nuesto nivel.
            int level = 0;
            int levelStart = 0;
            List<string> simplifiedInput = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                // Entramos en un nivel.
                if (input[i] == Characters.LeftParent)
                {
                    if (level == 0) levelStart = i;
                    level++;
                }
                else if (input[i] == Characters.RightParent) // Salimos de un nivel.
                {
                    level--;

                    if (level == 0)
                    {
                        // hemos vuelto a nuestro nivel, dividimos la entrada
                        // para parsearlo recursivamente despues.

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
                    // Estamos en un caracter operador, Y estamos en nuestro nivel.
                    // Dividimos la entrada para luego parsear a su derecha e izquierda.
                    var left = input.Substring(levelStart, i - levelStart);
                    if (!string.IsNullOrEmpty(left))
                        simplifiedInput.Add(left);
                    simplifiedInput.Add(input[i].ToString());
                    levelStart = i + 1;
                }
            }

            // Añadimos lo que ha quedado.
            simplifiedInput.Add(input.Substring(levelStart, input.Length - levelStart));

            // No volvimos a nuestro nivel - faltan parentesis.
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

                        // Parsear todo a su derecha.
                        return new NotNode(Parse(string.Join("",
                                        simplifiedInput.GetRange(position + 1, simplifiedInput.Count - position - 1))));
                    }

                    // Existen mas de un operador del mismo nivel de precedencia
                    if (simplifiedInput.Count(txt => txt.Length == 1 &&
                        Characters.GetLevel(txt[0]) == Characters.GetLevel(simplifiedInput[position][0])) > 1)
                    {
                        throw new FormatException("Ambiguo operador: " + simplifiedInput[position]);
                    }

                    INode left = Parse(string.Join("", simplifiedInput.GetRange(0, position)));
                    INode right =
                        Parse(string.Join("",
                            simplifiedInput.GetRange(position + 1, simplifiedInput.Count - position - 1)));

                    // Determinamos el operador
                    // devolvemos el nodo operador con todo a su derecha como el primer hijo
                    // y todo a su izquierda como su segundo hijo.
                    switch (simplifiedInput[position][0])
                    {
                        case Characters.Biconditional:
                            return new BiconditionalNode(left, right);
                        case Characters.Conditional:
                            return new ConditionalNode(left, right);
                        case Characters.And:
                            return new AndNode(left, right);
                        case Characters.Or:
                            return new OrNode(left, right);
                    }
                }
            }
            // Esto puede pasar cuando en nuestro nivel no hay ni literales ni operadores
            throw new Exception("Operator no encondrado o doble parentesis.");
        }
    }
}