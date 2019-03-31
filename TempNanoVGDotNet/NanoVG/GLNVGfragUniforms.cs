using System;
using System.Runtime.InteropServices;

namespace NanoVGDotNet.NanoVG
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GlNvgFragUniforms
    {
        // matrices are actually 3 vec4s

        // float[12]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public float[] scissorMat;
        // float[12]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public float[] paintMat;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public NvgColor innerCol;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public NvgColor outerCol;
        // float[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public float[] scissorExt;
        // float[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public float[] scissorScale;
        // float[2]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public float[] extent;

        public float radius;
        public float feather;
        public float strokeMult;
        public float strokeThr;

        private float texType_;

        public int TexType
        {
            get { return (int)texType_; }
            set { texType_ = value; }
        }

        private float type_;

        public int Type
        {
            get { return (int)type_; }
            set { type_ = value; }
        }

        public GlNvgFragUniforms()
        {
            scissorMat = new float[12];
            paintMat = new float[12];
            innerCol = new NvgColor();
            outerCol = new NvgColor();
            scissorExt = new float[2];
            scissorScale = new float[2];
            extent = new float[2];
        }

        public float[] GetFloats
        {
            get
            {
                var size = (int)GetSize;
                var felements = (int)Math.Ceiling((float)(size / sizeof(float)));
                var farr = new float[felements];

                var ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, farr, 0, felements);
                Marshal.FreeHGlobal(ptr);
                return farr;
            }
        }

        /// <summary>
        /// Gets the size of the <see cref="GlNvgFragUniforms"/> in bytes.
        /// </summary>
        /// <value>The size of the GLNVGfragUniforms struct.</value>
        public static uint GetSize
        {
            get
            {
                // 176 bytes
                //return (uint)(12 + 12 + 4 + 4 + 2 + 2 + 2 + 6) * 4;
                return (uint)Marshal.SizeOf(typeof(GlNvgFragUniforms));
            }
        }
    }
}