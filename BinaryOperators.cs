namespace VisualiazdorLogica
{
    public interface IBinaryOperator
    {
        bool Evaluate(bool left, bool right);
        string ToString();
    }

    public class AndOperation : IBinaryOperator
    {
        public bool Evaluate(bool left, bool right)
        {
            return left && right;
        }

        public override string ToString()
        {
            return Characters.And.ToString();
        }
    }

    public class OrOperation : IBinaryOperator
    {
        public bool Evaluate(bool left, bool right)
        {
            return left || right;
        }

        public override string ToString()
        {
            return Characters.Or.ToString();
        }
    }

    public class ConditionalOperation : IBinaryOperator
    {
        public bool Evaluate(bool left, bool right)
        {
            if (left && !right) return false;
            return true;
        }

        public override string ToString()
        {
            return Characters.Conditional.ToString();
        }
    }

    public class BiconditionalOperation : IBinaryOperator
    {
        public bool Evaluate(bool left, bool right)
        {
            return !(left ^ right);
        }

        public override string ToString()
        {
            return Characters.Biconditional.ToString();
        }
    }
}