# Collections

This is a collection of dotnet Collections.

It currenlty only contains ConcurrentCache.

## Concurrent Cache

This container is the same as a ConcurrentDictionary<TKey, TValue>, with the additional guarantee
that the value will only be computed once.  ConcurrentDictionary will compute it as many times as it
is concurrently added, and then throw away all but one of the values.  
It accomplishes this by wrapping TValue in a Lazy<T>.