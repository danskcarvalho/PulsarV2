using System.Linq.Expressions;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors;

public partial class Paginator
{
    public class SortedPaginatorBuilder2<TElement, TProjection, TSort1, TSort2> : Paginator<TElement> where TElement : class where TProjection : class
    {
        internal string _sort1;
        internal string? _sort2;
        internal Func<TElement, TSort1> _sort1Fn;
        internal Func<TElement, TSort2>? _sort2Fn;
        internal Func<TProjection, TSort1>? _proj1Fn;
        internal Func<TProjection, TSort2>? _proj2Fn;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal SortedPaginatorBuilder2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public SortedPaginatorBuilder2<TElement, TProjection, TSort1, TValue> AndBy<TValue>(Expression<Func<TElement, TValue>> expression, Expression<Func<TProjection, TValue>>? projection = null)
        {
            var result = new SortedPaginatorBuilder2<TElement, TProjection, TSort1, TValue>()
            {
                _sort1 = _sort1,
                _sort2 = _sort2,
                _sort1Fn = _sort1Fn,
                _proj1Fn = _proj1Fn,
            };

            if (this._sort2 is not null)
                throw new InvalidOperationException("cannot call this method twice");

            List<string> propertyNames = new List<string>();
            TraversePropertyChain(expression, propertyNames);
            if (propertyNames.Count == 0)
                throw new InvalidOperationException("invalid expression");

            result._sort2 = string.Join('.', propertyNames);
            result._sort2Fn = expression.Compile();

            if (projection != null)
            {
                List<string> propertyNames2 = new List<string>();
                TraversePropertyChain(projection, propertyNames2);
                result._proj2Fn = projection.Compile();

                if (propertyNames2.Count == 0)
                    throw new InvalidOperationException("invalid expression");
            }

            return result;
        }

        public override PageCursor<TElement, TProjection, object, TSort1, TSort2> ForLimit(int limit)
        {
            return new PageCursor<TElement, TProjection, object, TSort1, TSort2>
            {
                _limit = limit,
                _sort1 = _sort1,
                _sort1Fn = _sort1Fn,
                _sort2 = _sort2,
                _sort2Fn = _sort2Fn,
                _proj1Fn = _proj1Fn,
                _proj2Fn = _proj2Fn
            };
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
