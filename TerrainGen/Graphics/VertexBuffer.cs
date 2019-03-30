using System;
using OpenTK.Graphics.OpenGL;
using TerrainGen.Util;

namespace TerrainGen.Graphics
{
    public class VertexBuffer
    {
        public int ColorBufferId = -1;
        public int ElementBufferId = -1;
        public int NormalBufferId = -1;
        public int VertexBufferId = -1;
        public int VaoId = -1;

        public bool Initialized { get; private set; }

        public int NumElements;

        public void InitializeVbo(ChunkBuffer buffer)
        {
            lock (buffer.LockHandle)
            {
                try
                {
                    if (VaoId == -1)
                        GL.GenVertexArrays(1, out VaoId);
                    GL.BindVertexArray(VaoId);

                    // Color Buffer
                    {
                        // Generate Array Buffer Id
                        if (ColorBufferId == -1)
                            GL.GenBuffers(1, out ColorBufferId);

                        // Bind current context to Array Buffer ID
                        GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferId);

                        // Send data to buffer
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Length * sizeof(int)),
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
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Length * SmallVertex.Size),
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
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(buffer.Length * Vertex.Size),
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
                        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(buffer.Length * sizeof(short)),
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

                    GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);
                    GL.EnableVertexAttribArray(0);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Short, false, 0, 0);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);
                    GL.EnableVertexAttribArray(1);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Byte, false, 0, 0);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferId);
                    GL.EnableVertexAttribArray(2);
                    GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, false, 0, 0);

                    GL.BindVertexArray(0);

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
            GL.BindVertexArray(VaoId);
            GL.DrawArrays(type, 0, NumElements);
            GL.BindVertexArray(0);
        }
    }
}