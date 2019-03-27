using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TerrainGen.Buffer
{
    public class SimpleVertexBuffer
    {
        public int ColorBufferId = -1;
        public int ElementBufferId = -1;
        public int NormalBufferId = -1;
        public int VertexBufferId = -1;

        public bool Initialized { get; private set; }

        public int NumElements;

        public void InitializeVbo(ChunkBuffer buffer)
        {
            lock (buffer.LockHandle)
            {
                try
                {
                    // Color Buffer
                    {
                        // Generate Array Buffer Id
                        if (ColorBufferId == -1)
                            GL.GenBuffers(1, out ColorBufferId);

                        // Bind current context to Array Buffer ID
                        GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferId);

                        // Send data to buffer
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (buffer.Length * sizeof(int)),
                            buffer.ColorBuffer,
                            BufferUsageHint.StaticDraw);

                        // Validate that the buffer is the correct size
                        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
                            out int bufferSize);
                        if (buffer.Length * sizeof(int) != bufferSize)
                            throw new ApplicationException("Vertex color array not uploaded correctly");

                        // Clear the buffer Binding
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    }

                    // Normal Array Buffer
                    {
                        // Generate Array Buffer Id
                        if (NormalBufferId == -1)
                            GL.GenBuffers(1, out NormalBufferId);

                        // Bind current context to Array Buffer ID
                        GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);

                        // Send data to buffer
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (buffer.Length * SmallVertex.Size),
                            buffer.NormalBuffer, BufferUsageHint.StaticDraw);

                        // Validate that the buffer is the correct size
                        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
                            out int bufferSize);
                        if (buffer.Length * SmallVertex.Size != bufferSize)
                            throw new ApplicationException("Normal array not uploaded correctly");

                        // Clear the buffer Binding
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    }

                    // Vertex Array Buffer
                    {
                        // Generate Array Buffer Id
                        if (VertexBufferId == -1)
                            GL.GenBuffers(1, out VertexBufferId);

                        // Bind current context to Array Buffer ID
                        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);

                        // Send data to buffer
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (buffer.Length * Vertex.Size),
                            buffer.VertexBuffer,
                            BufferUsageHint.DynamicDraw);

                        // Validate that the buffer is the correct size
                        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
                            out int bufferSize);
                        if (buffer.Length * Vertex.Size != bufferSize)
                            throw new ApplicationException("Vertex array not uploaded correctly");

                        // Clear the buffer Binding
                        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    }

                    // Element Array Buffer
                    {
                        // Generate Array Buffer Id
                        if (ElementBufferId == -1)
                            GL.GenBuffers(1, out ElementBufferId);

                        // Bind current context to Array Buffer ID
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferId);

                        // Send data to buffer
                        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (buffer.Length * sizeof(short)),
                            buffer.IndexBuffer,
                            BufferUsageHint.StreamDraw);

                        // Validate that the buffer is the correct size
                        GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize,
                            out int bufferSize);
                        if (buffer.Length * sizeof(short) != bufferSize)
                            throw new ApplicationException("Element array not uploaded correctly");

                        // Clear the buffer Binding
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    }

                    Initialized = true;
                }
                catch (ApplicationException ex)
                {
                    Lumberjack.Error($"{ex.Message}. Try re-rendering.");
                }
                finally
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                }

                // Store the number of elements for the DrawElements call
                NumElements = buffer.Length;
            }
        }

        public void Render(PrimitiveType type = PrimitiveType.Quads)
        {
            // Push current Array Buffer state so we can restore it later
            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);

            // Normal Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);

                // Set the Pointer to the current bound array describing how the data is stored
                GL.NormalPointer(NormalPointerType.Byte, SmallVertex.Size, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(ArrayCap.NormalArray);
            }

            // Color Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferId);

                // Set the Pointer to the current bound array describing how the data is stored
                GL.ColorPointer(3, ColorPointerType.UnsignedByte, sizeof(int), IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(ArrayCap.ColorArray);
            }

            // Vertex Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);

                // Set the Pointer to the current bound array describing how the data is stored
                GL.VertexPointer(3, VertexPointerType.Short, Vertex.Size, IntPtr.Zero);

                // Enable the client state so it will use this array buffer pointer
                GL.EnableClientState(ArrayCap.VertexArray);
            }

            // Element Array Buffer
            {
                // Bind to the Array Buffer ID
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferId);

                // Draw the elements in the element array buffer
                // Draws up items in the Color, Vertex, TexCoordinate, and Normal Buffers using indices in the ElementArrayBuffer
                GL.DrawElements(type, NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

                // Could also call GL.DrawArrays which would ignore the ElementArrayBuffer and just use primitives
                // Of course we would have to reorder our data to be in the correct primitive order
            }

            // Restore the state
            GL.PopClientAttrib();
        }
    }
}