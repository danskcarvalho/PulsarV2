namespace Pulsar.BuildingBlocks.DataFactory;

public delegate TModel BuildFunction<TModel>();
public delegate TModel BuildFunction<TModel, TArg1>(TArg1 arg1);
public delegate TModel BuildFunction<TModel, TArg1, TArg2>(TArg1 arg1, TArg2 arg2);
public delegate TModel BuildFunction<TModel, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3);