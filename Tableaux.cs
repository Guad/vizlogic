using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VisualiazdorLogica
{
    public static class Tableaux
    {
        public static TableauxNode Generate(INode root, TableauxNode father)
        {
            if (root is NotNode && root.Children[0] is VariableNode)
            {
                return new TableauxNode(root.Prettify(), father)
                {
                    State = false,
                    Representative = root,
                    Closed = InverseRecursiveMatch(father, node => 
                        node.Children.Any(c => c.Representative is VariableNode &&
                                              ((VariableNode) c.Representative).Key == ((VariableNode) root.Children[0]).Key)),
                };
            }

            if (root is LiteralNode || root is VariableNode)
            {
                return new TableauxNode(root.Prettify(), father)
                {
                    State = true,
                    Representative = root,
                    Closed = InverseRecursiveMatch(father, node =>
                        node.Children.Any(c => c.Representative is NotNode && c.Representative.Children[0] is VariableNode &&
                                              ((VariableNode)c.Representative.Children[0]).Key == ((VariableNode)root.Children[0]).Key)),
                };
            }

            if (root is AndNode)
            {
                TableauxNode node = new TableauxNode(root.Prettify(), father);
                node.Children.Add(Generate(root.Children[0], node));
                node.Children.Add(Generate(root.Children[1], node));
                node.Disyuntive = false;
                return node;
            }

            if (root is OrNode)
            {
                TableauxNode node = new TableauxNode(root.Prettify(), father);
                node.Children.Add(Generate(root.Children[0], node));
                node.Children.Add(Generate(root.Children[1], node));
                node.Disyuntive = true;
                return node;
            }

            return null;
        }

        private static bool InverseRecursiveMatch(TableauxNode root, Func<TableauxNode, bool> predicate)
        {
            if (predicate(root)) return true;

            return InverseRecursiveMatch(root.Father, predicate);
        }
    }

    public class TableauxNode
    {
        public TableauxNode(string text, TableauxNode father)
        {
            Children = new List<TableauxNode>();
            Father = father;
            Text = text;
        }

        public bool Disyuntive { get; set; } // Disyuntiva o no?
        public List<TableauxNode> Children { get; set; }

        public TableauxNode Father { get; set; }

        public INode Representative { get; set; }

        public string Text { get; set; }

        public bool? Closed { get; set; }
        public bool? State { get; set; }
    }

}
