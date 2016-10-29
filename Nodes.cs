namespace VisualiazdorLogica
{
    // Interfaz de un nodo. Puede tener varios o ningun hijos. 
    public interface INode
    {
        int ChildNumber { get; }
        INode[] Children { get; set; }

        bool Evaluate();
        string Prettify();
    }

    public interface ISimplifiable
    {
        INode Simplify();
    }

    // Cualquier variable, como 'p', 'q', 's', etc
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

    // Literal T(rue) o F(alse)
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

    // Nodo binario con un operador
    public abstract class BinaryNode : INode
    {
        public abstract char OperationCharacter { get; }

        public BinaryNode(INode left, INode right)
        {
            Children = new INode[2];
            Children[0] = left;
            Children[1] = right;
        }

        //public BinaryNode() : this(null, null) { }

        public int ChildNumber => 2;
        public INode[] Children { get; set; }

        public abstract bool Evaluate();

        public string Prettify()
        {
            return string.Format("({0}{2}{1})", Children[0].Prettify(), Children[1].Prettify(), OperationCharacter);
        }
    }

    #region BinaryNodes

    public class AndNode : BinaryNode
    {
        public AndNode(INode left, INode right) : base(left, right)
        { }

        public override char OperationCharacter => Characters.And;

        public override bool Evaluate()
        {
            return Children[0].Evaluate() && Children[1].Evaluate();
        }
    }

    public class OrNode : BinaryNode
    {
        public OrNode(INode left, INode right) : base(left, right)
        { }

        public override char OperationCharacter => Characters.Or;

        public override bool Evaluate()
        {
            return Children[0].Evaluate() || Children[1].Evaluate();
        }
    }

    public class ConditionalNode : BinaryNode, ISimplifiable
    {
        public ConditionalNode(INode left, INode right) : base(left, right)
        { }

        public override char OperationCharacter => Characters.Conditional;

        public override bool Evaluate()
        {
            return !Children[0].Evaluate() || Children[1].Evaluate();
        }

        public INode Simplify()
        {
            return new OrNode(new NotNode(Children[0]), Children[1]);
        }
    }

    public class BiconditionalNode : BinaryNode, ISimplifiable
    {
        public BiconditionalNode(INode left, INode right) : base(left, right)
        { }

        public override char OperationCharacter => Characters.Biconditional;

        public override bool Evaluate()
        {
            return !(Children[0].Evaluate() ^ Children[1].Evaluate());
        }

        public INode Simplify()
        {
            return new OrNode(
                new AndNode(Children[0], Children[1]),
                new AndNode(new NotNode(Children[0]), new NotNode(Children[1]))
                );
        }
    }

    #endregion

    // Nodo NOT
    public class NotNode : INode, ISimplifiable
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
            return Characters.Not + Children[0].Prettify();
        }

        public INode Simplify()
        {
            if (Children[0] is ISimplifiable) Children[0] = ((ISimplifiable) Children[0]).Simplify();

            if (Children[0] is BinaryNode)
            {
                var bin = Children[0] as BinaryNode;

                if (Children[0] is AndNode)
                    return new OrNode(new NotNode(bin.Children[0]), new NotNode(bin.Children[1]));
                if (Children[0] is OrNode)
                    return new AndNode(new NotNode(bin.Children[0]), new NotNode(bin.Children[1]));
            }

            if (Children[0] is NotNode)
            {
                return Children[0].Children[0];
            }

            return this;
        }
    }
}