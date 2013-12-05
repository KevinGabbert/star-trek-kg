using System;

namespace StarTrek_KG
{
    public struct NonNullable<T> where T : class
    {
        private readonly T value;

        public NonNullable(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.value = value;
        }

        public T Value
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
