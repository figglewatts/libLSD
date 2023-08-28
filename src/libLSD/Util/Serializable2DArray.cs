using System;
using System.Collections;
using System.Collections.Generic;

namespace libLSD.Util
{
    [Serializable]
    public class Serializable2DArray<T> : IEnumerable<T>
    {
        public T[] Underlying;
        public int Width;
        public int Height;

        public T this[int x, int y]
        {
            get => Underlying[x * Height + y];
            set => Underlying[x * Height + y] = value;
        }

        public Serializable2DArray(int width, int height)
        {
            Width = width;
            Height = height;
            Underlying = new T[width * height];
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var t in Underlying)
            {
                yield return t;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() => Underlying.GetEnumerator();

        public int Length => Underlying.Length;
    }
}
