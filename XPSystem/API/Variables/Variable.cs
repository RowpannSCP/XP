namespace XPSystem.API.Variables
{
    using System;

    /// <summary>
    /// Represents a variable with expire-able data.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Gets or sets value of the variable.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the expiry time of the variable.
        /// Can be null if the variable does not expire.
        /// </summary>
        public DateTime? ExpiryTime { get; set; }

        /// <summary>
        /// Gets whether or not the variable has expired.
        /// </summary>
        public bool IsExpired => ExpiryTime != null && ExpiryTime < DateTime.Now;

        /// <summary>
        /// Creates a new, uninitialized, variable.
        /// </summary>
        public Variable()
        {
        }

        /// <summary>
        /// Creates a new variable with the specified value.
        /// </summary>
        /// <param name="value">The value of the variable.</param>
        public Variable(object value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new variable with the specified value and expiry data.
        /// </summary>
        /// <param name="value">The value of the variable.</param>
        /// <param name="expiryTime">The expiry time of the variable.</param>
        public Variable(object value, DateTime? expiryTime)
        {
            Value = value;
            ExpiryTime = expiryTime;
        }

        /// <summary>
        /// Returns the value of the variable as the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to.</typeparam>
        /// <returns>The value of the variable as the specified type.</returns>
        /// <exception cref="InvalidCastException">Thrown if the value cannot be cast to the specified type.</exception>
        public T As<T>()
        {
            if (Value is T value)
                return value;

            throw new InvalidCastException($"Cannot cast {Value.GetType()} to {typeof(T)}");
        }

        /// <summary>
        /// Creates a new variable with the specified string as value.
        /// </summary>
        public static implicit operator Variable(string value) => new(value);

        /// <summary>
        /// Creates a new variable with the specified int as value.
        /// </summary>
        public static implicit operator Variable(int value) => new(value);

        /// <summary>
        /// Creates a new variable with the specified float as value.
        /// </summary>
        public static implicit operator Variable(bool value) => new(value);

        /// <summary>
        /// Attempts to cast the variable to a string.
        /// </summary>
        public static implicit operator bool(Variable variable) => variable.As<bool>();
    }
}