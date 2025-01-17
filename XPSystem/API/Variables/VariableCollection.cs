namespace XPSystem.API.Variables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection of variables.
    /// </summary>
    public class VariableCollection : IEnumerable<KeyValuePair<string, Variable>>
    {
        private Dictionary<string, Variable> _variables = new();

        /// <summary>
        /// Gets the number of variables in the collection.
        /// </summary>
        public int Count => _variables.Count;

        /// <summary>
        /// Adds a new variable to the collection.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        /// <param name="expiryTime">The expiry time of the variable.</param>
        public void Add(string key, object value, DateTime? expiryTime = null)
        {
            var variable = new Variable(value, expiryTime);

            if (!variable.IsExpired)
                _variables.Add(key, variable);
        }

        /// <summary>
        /// Adds a new variable to the collection.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <param name="variable">The variable to add.</param>
        public void Add(string key, Variable variable)
        {
            if (variable.IsExpired)
                return;

            _variables.Add(key, variable);
        }

        /// <summary>
        /// Sets the value of the variable with the specified key.
        /// Creates a new variable if one with the specified key does not exist.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        /// <param name="expiryTime">The expiry time of the variable.</param>
        public void Set(string key, object value, DateTime? expiryTime = null)
        {
            if (_variables.TryGetValue(key, out Variable variable))
            {
                variable.Value = value;
                variable.ExpiryTime = expiryTime;
            }
            else
            {
                Add(key, value, expiryTime);
            }
        }

        /// <summary>
        /// Attempts to get the variable with the specified key.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <param name="variable">The variable with the specified key, if found; otherwise, null.</param>
        /// <returns>Whether or not the variable was found.</returns>
        public bool TryGet(string key, out Variable variable)
        {
            if (_variables.TryGetValue(key, out variable))
            {
                if (!variable.IsExpired)
                    return true;

                Remove(key);
            }

            variable = null;
            return false;
        }

        /// <summary>
        /// Attempts to get the variable with the specified key and cast it to the specified type.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <param name="value">The variable with the specified key, if found; otherwise, null.</param>
        /// <typeparam name="T">The type to cast the variable to.</typeparam>
        /// <returns>Whether or not the variable was found and casted.</returns>
        /// <remarks>Will throw if the cast fails.</remarks>
        public bool TryGet<T>(string key, out T value)
        {
            if (TryGet(key, out Variable variable))
            {
                value = variable.As<T>();
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Removes the variable with the specified key.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        /// <returns>Whether or not the variable was removed.</returns>
        public bool Remove(string key) => _variables.Remove(key);

        /// <summary>
        /// Gets or sets the variable with the specified key.
        /// </summary>
        /// <param name="key">The key of the variable.</param>
        public Variable this[string key]
        {
            get => TryGet(key, out Variable variable)
                ? variable
                : null;
            set => _variables[key] = value;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, Variable>> GetEnumerator() => _variables.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}