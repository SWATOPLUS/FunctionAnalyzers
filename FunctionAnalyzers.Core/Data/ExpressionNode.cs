namespace FunctionAnalyzers.Core.Data
{
    public class ExpressionNode
    {
        public ExpressionNode(string variableName)
        {
            VariableName = variableName;
        }

        public ExpressionNode(string operationName, params ExpressionNode[] operands)
        {
            OperationName = operationName;
            Operands = operands;
        }


        public string? VariableName { get; }

        public string? OperationName { get; }

        public ExpressionNode[]? Operands { get; }

        public bool IsVariable => VariableName != null;

        public bool IsOperation => OperationName != null;

    }
}
