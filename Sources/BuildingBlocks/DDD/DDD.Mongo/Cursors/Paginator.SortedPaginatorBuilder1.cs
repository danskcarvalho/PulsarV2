using System.Linq.Expressions;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public partial class Paginator
{
    public class SortedPaginatorBuilder1<TElement, TProjection> where TElement : class where TProjection : class
    {
        internal SortedPaginatorBuilder1() { }

        public SortedPaginatorBuilder2<TElement, TProjection, TValue, object> SortBy<TValue>(Expression<Func<TElement, TValue>> expression, Expression<Func<TProjection, TValue>>? projection = null)
        {
            if (projection == null && typeof(TElement) != typeof(TProjection))
                throw new InvalidOperationException("projection must be specified");

            List<string> propertyNames = new List<string>();
            TraversePropertyChain(expression, propertyNames);
            if (propertyNames.Count == 0)
                throw new InvalidOperationException("invalid expression");

            var result = new SortedPaginatorBuilder2<TElement, TProjection, TValue, object>
            {
                _sort1 = string.Join('.', propertyNames),
                _sort1Fn = expression.Compile()
            };

            if (projection != null)
            {
                List<string> propertyNames2 = new List<string>();
                TraversePropertyChain(projection, propertyNames2);
                result._proj1Fn = projection.Compile();

                if (propertyNames2.Count == 0)
                    throw new InvalidOperationException("invalid expression");
            }

            return result;
        }

        private void TraversePropertyChain<TU, TV>(Expression<Func<TU, TV>> expression, List<string> propertyNames)
        {
            if (expression.Body is MemberExpression memberExpr)
            {
                MemberExpression? me = memberExpr;
                // Traverse the expression tree to get property names
                while (me != null)
                {
                    propertyNames.Insert(0, me.Member.Name);
                    me = me.Expression as MemberExpression;
                }
            }
        }
    }
}
