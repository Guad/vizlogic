using System.Windows.Documents;

namespace VisualiazdorLogica
{
    // Interfaces
    public interface INode
    {
        int ChildNumber { get; }
        INode[] Children { get; set; }

        bool Evaluate();
        string Prettify();
    }

    public class VariableNode : INode
    {
        public string Key;

        public VariableNode(string key)
        {
            Key = key;
            LiteralVariables.Add(key, false);
        }

        public VariableNode() { }

        public int ChildNumber => 0;
        public INode[] Children { get; set; }

        public bool Evaluate()
        {
            return LiteralVariables.Get(Key);
        }

        public string Prettify()
        {
            return Key;
        }
    }

    public class LiteralNode : INode
    {
        public bool Value;

        public LiteralNode(bool value)
        {
            Value = value;
        }

        public LiteralNode() { }

        public int ChildNumber => 0;
        public INode[] Children { get; set; }

        public bool Evaluate()
        {
            return Value;
        }

        public string Prettify()
        {
            return (Value ? Characters.True : Characters.False).ToString();
        }
    }

    public class BinaryNode : INode
    {
        public IBinaryOperator Operator { get; set; }

        public BinaryNode(IBinaryOperator op, INode left, INode right)
        {
            Operator = op;
            Children = new INode[2];
            Children[0] = left;
            Children[1] = right;
        }

        public BinaryNode() : this(null, null, null)
        { }

        public int ChildNumber => 2;
        public INode[] Children { get; set; }

        public bool Evaluate()
        {
            return Operator.Evaluate(Children[0].Evaluate(), Children[1].Evaluate());
        }

        public string Prettify()
        {
            return string.Format("({0}{2}{1})", Children[0].Prettify(), Children[1].Prettify(), Operator.ToString());
        }
    }

    public class NotNode : INode
    {
        public NotNode(INode child)
        {
            Children = new INode[1];
            Children[0] = child;
        }

        public NotNode() : this(null) {}

        public bool Evaluate()
        {
            return !Children[0].Evaluate();
        }

        public int ChildNumber => 1;
        public INode[] Children { get; set; }

        public string Prettify()
        {
            bool parenthesis = Children[0] is BinaryNode;

            if (parenthesis) return string.Format("{0}({1})", Characters.Not, Children[0].Prettify());
            return Characters.Not + Children[0].Prettify();
        }
    }
}