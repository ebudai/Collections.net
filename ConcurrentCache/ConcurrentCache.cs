// Copyright © 2022 Eric Budai
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Budaisoft.Collections.Generic
{
    /// <summary>
    ///     Wrapper for <see cref="ConcurrentDictionary{TKey, TValue}"/> using <see cref="Lazy{T, TMetadata}"/> instantiation for <typeparamref name="TValue"/>
    /// </summary>
    /// <remarks>
    ///     The purpose of this class is so that we can have what behaves like a normal <see cref="ConcurrentDictionary{TKey, TValue}"/>,
    ///     but the default generation function will only be executed once regardless of how many concurrent calls are made.
    /// </remarks>
    public class ConcurrentCache<TKey, TValue>
    {

        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _backingDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>(); // wrapped dictionary
        private readonly Func<TKey, TValue> _valueFactory = _ => default; // function to return default values

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentCache{TKey, TValue}"/> with <see cref="EqualityComparer{TKey}"/>
        ///     for the equality comparer and a value factory which supplies the default value of <typeparamref name="TValue"/>
        /// </summary>
        public ConcurrentCache() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentCache{TKey, TValue}"/> with a value factory which supplies the default
        ///     value of <typeparamref name="TValue"/>, and the specified equality comparer
        /// </summary>
        /// <param name="comparer">equality comparer for <typeparamref name="TKey"/></param>
        public ConcurrentCache(IEqualityComparer<TKey> comparer) => _backingDictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>(comparer);

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentCache{TKey, TValue}"/> with <see cref="EqualityComparer{TKey}"/>
        ///     for the equality comparer and the specified value factory
        /// </summary>
        /// <param name="defaultValueFactory">callable used to produce values of type <typeparamref name="TValue"/> given a <typeparamref name="TKey"/></param>
        public ConcurrentCache(Expression<Func<TKey, TValue>> defaultValueFactory) => _valueFactory = defaultValueFactory.Compile();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentCache{TKey, TValue}"/> with specified equality comparer and value factory
        /// </summary>
        /// <param name="comparer">equality comparer for <typeparamref name="TKey"/></param>
        /// <param name="defaultValueFactory">callable used to produce values of type <typeparamref name="TValue"/> given a <typeparamref name="TKey"/></param>
        public ConcurrentCache(IEqualityComparer<TKey> comparer, Expression<Func<TKey, TValue>> defaultValueFactory) : this(comparer) => _valueFactory = defaultValueFactory.Compile();

        /// <summary>
        ///     Gets a collection containing the keys in the <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// </summary>
        /// <value>
        ///     An <see cref="ICollection{TKey}"/> containing the keys in the <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// </value>
        public ICollection<TKey> Keys => _backingDictionary.Keys;

        /// <summary>
        ///     Gets a collection containing the values in the <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// </summary>
        /// <value>
        ///     An <see cref="ICollection{TValue}"/> containing the values in the <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// </value>
        public ICollection<Lazy<TValue>> Values => _backingDictionary.Values;

        /// <summary>
        ///     Attempts to associate <paramref name="key"/> with <paramref name="value"/> in the <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="key">the key of the element to add</param>
        /// <param name="value">the value of the element to add</param>
        /// <returns>true if the key/value pair was successfully added to the <see cref="ConcurrentDictionary{TKey, TValue}"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        /// <exception cref="OverflowException">the underlying <see cref="ConcurrentDictionary{TKey, TValue}"/> is full</exception>
        public bool TryAdd(TKey key, TValue value) => _backingDictionary.TryAdd(key, new Lazy<TValue>(() => value, isThreadSafe: true));

        /// <summary>
        ///     Attempts to remove and return the the value with the specified <paramref name="key"/>
        /// </summary>
        /// <param name="key">the key of the element to remove and return</param>
        /// <param name="value">the returned element</param>
        /// <returns>true if the key/value pair was successfully removed from the <see cref="ConcurrentDictionary{TKey, TValue}"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="key"/> is null</exception>
        public bool TryRemove(TKey key, out TValue value)
        {
            var removed = _backingDictionary.TryRemove(key, out var lazy);
            value = removed ? lazy.Value : default;
            return removed;
        }

        /// <summary>
        ///     Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey, TValue}"/> if <paramref name="key"/> does not exist
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for <paramref name="key"/></param>
        /// <returns>
        ///     The value for <paramref name="key"/>.  This will either be the existing value for <paramref name="key"/> if it already exist,
        ///     or a new value for <paramref name="key"/> given by <paramref name="valueFactory"/> if it does not.
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="key"/> or <paramref name="valueFactory"/> is null</exception>
        /// <exception cref="OverflowException">if the <see cref="ConcurrentDictionary{TKey, TValue}"/> is null</exception>
        public TValue GetOrAdd(TKey key, Expression<Func<TKey, TValue>> valueFactory) => _backingDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory.Compile()(k))).Value;

        /// <summary>
        ///     Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey, TValue}"/> if <paramref name="key"/> does not exist
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for <paramref name="key"/></param>
        /// <returns>
        ///     The value for <paramref name="key"/>.  This will either be the existing value for <paramref name="key"/> if it already exist,
        ///     or a new value for <paramref name="key"/> given by <paramref name="valueFactory"/> if it does not.
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="key"/> is null</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="valueFactory"/> is null</exception>
        /// <exception cref="OverflowException">if the <see cref="ConcurrentDictionary{TKey, TValue}"/> is null</exception>
        public TValue GetOrAdd(TKey key, Expression<Func<TValue>> valueFactory) => _backingDictionary.GetOrAdd(key, _ => new Lazy<TValue>(valueFactory.Compile())).Value;

        /// <summary>
        ///     Gets or sets the value associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <remarks>the getter for this method will automatically construct a value for <paramref name="key"/> using the value factory function</remarks>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key</returns>
        public TValue this[TKey key]
        {
            get => _backingDictionary.GetOrAdd(key, k => new Lazy<TValue>(() => _valueFactory(k))).Value;
            set => _backingDictionary[key] = new Lazy<TValue>(() => value, isThreadSafe: true);
        }

        /// <summary>
        ///     Determines whether the <see cref="ConcurrentDictionary{TKey, TValue}"/> contains <paramref name="key"/>
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="ConcurrentDictionary{TKey, TValue}"/></param>
        /// <returns>true if the <see cref="ConcurrentDictionary{TKey, TValue}"/> contains an element with <paramref name="key"/>.</returns>
        public bool ContainsKey(TKey key) => _backingDictionary.ContainsKey(key);
    }
}
