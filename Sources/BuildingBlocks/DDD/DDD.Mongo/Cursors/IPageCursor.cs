using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Cursors
{
    public interface IPageCursor<TSelf, TElement> 
        where TElement : class 
        where TSelf : class, IPageCursor<TSelf, TElement>
    {
        abstract static TSelf FromFilter(dynamic filter);
        abstract static bool HasSortColumn2 { get; }
        (string Name, object? Value) SortColumn1 { get; }
        (string Name, object? Value)? SortColumn2 { get; }

        TSelf Next(TElement last);
    }
}
