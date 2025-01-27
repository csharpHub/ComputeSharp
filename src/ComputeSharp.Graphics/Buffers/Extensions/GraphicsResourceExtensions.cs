﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using ComputeSharp.Graphics.Buffers.Abstract;

namespace ComputeSharp.Graphics.Buffers.Extensions
{
    /// <summary>
    /// A <see langword="class"/> with extension methods for the <see cref="GraphicsDevice"/> type to allocate generic resources
    /// </summary>
    internal static class GraphicsResourceExtensions
    {
        /// <summary>
        /// Allocates a new constant buffer with the specified generic values
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> instance to use to allocate the buffer</param>
        /// <param name="data">The input <see cref="object"/> sequence to copy to the allocated buffer</param>
        /// <returns>A constant <see cref="ConstantBuffer{T}"/> instance (as a <see cref="GraphicsResource"/>) with the input data</returns>
        [Pure]
        public static GraphicsResource AllocateConstantBufferFromReflectedValues(this GraphicsDevice device, IReadOnlyList<object> data)
        {
            Span<Vector4> temporarySpan = stackalloc Vector4[data.Count];
            ref byte r0 = ref Unsafe.As<Vector4, byte>(ref temporarySpan.GetPinnableReference());
            int offset = 0;

            // Local method to write a value in the temporary buffer
            void WriteValue<T>(T value, ref byte target) where T : unmanaged
            {
                int size = Unsafe.SizeOf<T>();
                if (offset % 16 > 16 - size) offset += 16 - offset % 16;
                Unsafe.As<byte, T>(ref Unsafe.Add(ref target, offset)) = value;
                offset += size;
            }

            // Iterate on the input values and write them with the proper padding
            for (int j = 0; j < data.Count; j++)
            {
                switch (data[j])
                {
                    case bool b:
                        uint ub = default;
                        Unsafe.As<uint, bool>(ref ub) = b;
                        WriteValue(ub, ref r0);
                        break;
                    case Bool2 b2: WriteValue(b2, ref r0); break;
                    case Bool3 b3: WriteValue(b3, ref r0); break;
                    case Bool4 b4: WriteValue(b4, ref r0); break;
                    case int i: WriteValue(i, ref r0); break;
                    case Int2 i2: WriteValue(i2, ref r0); break;
                    case Int3 i3: WriteValue(i3, ref r0); break;
                    case Int4 i4: WriteValue(i4, ref r0); break;
                    case uint u: WriteValue(u, ref r0); break;
                    case UInt2 ui2: WriteValue(ui2, ref r0); break;
                    case UInt3 ui3: WriteValue(ui3, ref r0); break;
                    case UInt4 ui4: WriteValue(ui4, ref r0); break;
                    case float f: WriteValue(f, ref r0); break;
                    case Float2 f2: WriteValue(f2, ref r0); break;
                    case Float3 f3: WriteValue(f3, ref r0); break;
                    case Float4 f4: WriteValue(f4, ref r0); break;
                    case Vector2 v2: WriteValue(v2, ref r0); break;
                    case Vector3 v3: WriteValue(v3, ref r0); break;
                    case Vector4 v4: WriteValue(v4, ref r0); break;
                    case double d: WriteValue(d, ref r0); break;
                    case Double2 d2: WriteValue(d2, ref r0); break;
                    case Double3 d3: WriteValue(d3, ref r0); break;
                    case Double4 d4: WriteValue(d4, ref r0); break;
                    default: throw new InvalidOperationException($"Invalid item of type {data[j].GetType()}");
                }
            }

            // Create the array to return
            ConstantBuffer<Vector4> buffer = new ConstantBuffer<Vector4>(device, data.Count);
            buffer.SetData(temporarySpan);

            return buffer;
        }
    }
}
