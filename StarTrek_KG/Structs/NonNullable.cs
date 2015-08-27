using System;

namespace StarTrek_KG.Structs
{
    public struct NonNullable<T> where T : class
    {
        private readonly T value;

        private NonNullable(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            this.value = value;
        }

        private T Value
        {
            get
            {
                if (value == null)
                {
                    throw new NullReferenceException();
                }
                return value;
            }
        }

        public static implicit operator NonNullable<T>(T value)
        {
            return new NonNullable<T>(value);
        }

        public static implicit operator T(NonNullable<T> wrapper)
        {
            return wrapper.Value;
        }
    }
}
