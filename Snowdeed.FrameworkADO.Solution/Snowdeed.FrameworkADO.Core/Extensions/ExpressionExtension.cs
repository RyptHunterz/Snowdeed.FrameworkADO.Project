using System.Linq.Expressions;
using System.Reflection;

namespace Snowdeed.FrameworkADO.Core.Extensions;

public static class ExpressionExtension
{
    public static string ConvertToSql<T, TResult>(this Expression<Func<T, TResult>> expression)
    {
        return ProcessExpression(expression.Body);
    }

    private static string ProcessExpression(Expression expression)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.Equal:
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
            case ExpressionType.NotEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.LessThan:

                var binaryExpression = (BinaryExpression)expression;

                string? left = ProcessExpression(binaryExpression.Left);
                string? right = ProcessExpression(binaryExpression.Right);
                string opr = GetSqlOperator(binaryExpression.NodeType);

                return $"({left} {opr} {right})";

            case ExpressionType.MemberAccess:

                var memberExpression = (MemberExpression)expression;

                if (memberExpression.Expression is ConstantExpression)
                {
                    var value = GetValueFromExpression(memberExpression);
                    return FormatValue(value);
                }

                return memberExpression.Member.Name;

            case ExpressionType.Constant:
                var constantExpression = (ConstantExpression)expression;
                return FormatValue(constantExpression.Value);

            default:
                break;
        }
        throw new NotSupportedException($"Type d'expression {expression.GetType().Name} non supporté");
    }

    private static object? GetValueFromExpression(MemberExpression memberExpression)
    {
        if (memberExpression.Expression is ConstantExpression constantExpression)
        {
            var fieldInfo = memberExpression.Member as FieldInfo;

            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(constantExpression.Value);
            }
        }

        throw new InvalidOperationException("Impossible d'extraire la valeur de l'expression membre.");
    }

    private static string FormatValue(object? value)
    {
        return value is string ? $"'{value}'" : $"{value}";
    }

    private static string GetSqlOperator(ExpressionType expressionType)
    {
        return expressionType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            _ => throw new NotSupportedException($"Opérateur {expressionType} non supporté"),
        };
    }
}
