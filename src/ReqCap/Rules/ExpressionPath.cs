using System.Linq.Expressions;

namespace ReqCap.Rules;

internal static class ExpressionPath {
    public static string GetPropertyPath(Expression expression) {
        var members = new Stack<string>();
        var current = expression;
        while (current is MemberExpression memberExpression) {
            members.Push(memberExpression.Member.Name);
            if (memberExpression.Expression is null)
                break;
            current = memberExpression.Expression;
        }
        if (current is not ParameterExpression) {
            throw new ArgumentException("Expression must be a member access path, for example x => x.Property or x => x.Child.Property.");
        }
        return string.Join('.', members);
    }
}
