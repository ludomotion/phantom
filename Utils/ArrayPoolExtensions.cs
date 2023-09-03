﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom.Utils
{
    /// <summary>
    /// Helpers for working with the <see cref="ArrayPool{T}"/> type.
    /// </summary>
    public static class ArrayPoolExtensions
    {
        /// <summary>
        /// Changes the number of elements of a rented one-dimensional array to the specified new size.
        /// </summary>
        /// <typeparam name="T">The type of items into the target array to resize.</typeparam>
        /// <param name="pool">The target <see cref="ArrayPool{T}"/> instance to use to resize the array.</param>
        /// <param name="array">The rented <typeparamref name="T"/> array to resize, or <see langword="null"/> to create a new array.</param>
        /// <param name="newSize">The size of the new array.</param>
        /// <param name="clearArray">Indicates whether the contents of the array should be cleared before reuse.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newSize"/> is less than 0.</exception>
        /// <remarks>When this method returns, the caller must not use any references to the old array anymore.</remarks>
        public static void Resize<T>(this ArrayPool<T> pool, ref T[] array, int newSize, bool clearArray = false)
        {
            // If the new size is the same as the current size, do nothing
            if (array.Length == newSize)
            {
                return;
            }

            // Rent a new array with the specified size, and copy as many items from the current array
            // as possible to the new array. This mirrors the behavior of the Array.Resize API from
            // the BCL: if the new size is greater than the length of the current array, copy all the
            // items from the original array into the new one. Otherwise, copy as many items as possible,
            // until the new array is completely filled, and ignore the remaining items in the first array.
            T[] newArray = pool.Rent(newSize);
            int itemsToCopy = Math.Min(array.Length, newSize);
            Array.Copy(array, 0, newArray, 0, itemsToCopy);
            pool.Return(array, clearArray);
            array = newArray;
        }
    }
}
