using System;

namespace TerrainGenCore
{

	/**
	 * K.jpg's OpenSimplex 2, faster variant
	 *
	 * - 2D is standard simplex implemented using a lookup table.
	 * - 3D is "Re-oriented 4-point BCC noise" which constructs a
	 * congruent BCC lattice in a much different way than usual.
	 * - 4D constructs the lattice as a union of five copies of its
	 * reciprocal. It successively finds the closest point on each.
	 *
	 * Multiple versions of each function are provided. See the
	 * documentation above each, for more info.
	 */

	public class OpenSimplex2F
	{
		private static readonly int Psize = 2048;
		private static readonly int Pmask = 2047;

		private readonly short[] _perm;
		private readonly Grad2[] _permGrad2;
		private readonly Grad3[] _permGrad3;
		private readonly Grad4[] _permGrad4;

		public OpenSimplex2F(long seed)
		{
			_perm = new short[Psize];
			_permGrad2 = new Grad2[Psize];
			_permGrad3 = new Grad3[Psize];
			_permGrad4 = new Grad4[Psize];
			var source = new short[Psize];
			for (short i = 0; i < Psize; i++)
				source[i] = i;
			for (var i = Psize - 1; i >= 0; i--)
			{
				seed = seed * 6364136223846793005L + 1442695040888963407L;
				var r = (int)((seed + 31) % (i + 1));
				if (r < 0)
					r += (i + 1);
				_perm[i] = source[r];
				_permGrad2[i] = Gradients2D[_perm[i]];
				_permGrad3[i] = Gradients3D[_perm[i]];
				_permGrad4[i] = Gradients4D[_perm[i]];
				source[r] = source[i];
			}
		}

		/*
		 * Noise Evaluators
		 */

		/**
		 * 2D Simplex noise, standard lattice orientation.
		 */
		public double Noise2(double x, double y)
		{

			// Get points for A2* lattice
			var s = 0.366025403784439 * (x + y);
			double xs = x + s, ys = y + s;

			return noise2_Base(xs, ys);
		}

		/**
		 * 2D Simplex noise, with Y pointing down the main diagonal.
		 * Might be better for a 2D sandbox style game, where Y is vertical.
		 * Probably slightly less optimal for heightmaps or continent maps.
		 */
		public double noise2_XBeforeY(double x, double y)
		{

			// Skew transform and rotation baked into one.
			var xx = x * 0.7071067811865476;
			var yy = y * 1.224744871380249;

			return noise2_Base(yy + xx, yy - xx);
		}

		/**
		 * 2D Simplex noise base.
		 * Lookup table implementation inspired by DigitalShadow.
		 */
		private double noise2_Base(double xs, double ys)
		{
			double value = 0;

			// Get base points and offsets
			int xsb = FastFloor(xs), ysb = FastFloor(ys);
			double xsi = xs - xsb, ysi = ys - ysb;

			// Index to point list
			var index = (int)((ysi - xsi) / 2 + 1);

			var ssi = (xsi + ysi) * -0.211324865405187;
			double xi = xsi + ssi, yi = ysi + ssi;

			// Point contributions
			for (var i = 0; i < 3; i++)
			{
				var c = Lookup2D[index + i];

				double dx = xi + c.Dx, dy = yi + c.Dy;
				var attn = 0.5 - dx * dx - dy * dy;
				if (attn <= 0)
					continue;

				int pxm = (xsb + c.Xsv) & Pmask, pym = (ysb + c.Ysv) & Pmask;
				var grad = _permGrad2[_perm[pxm] ^ pym];
				var extrapolation = grad.Dx * dx + grad.Dy * dy;

				attn *= attn;
				value += attn * attn * extrapolation;
			}

			return value;
		}

		/**
		 * 3D Re-oriented 4-point BCC noise, classic orientation.
		 * Proper substitute for 3D Simplex in light of Forbidden Formulae.
		 * Use noise3_XYBeforeZ or noise3_XZBeforeY instead, wherever appropriate.
		 */
		public double noise3_Classic(double x, double y, double z)
		{

			// Re-orient the cubic lattices via rotation, to produce the expected look on cardinal planar slices.
			// If texturing objects that don't tend to have cardinal plane faces, you could even remove this.
			// Orthonormal rotation. Not a skew transform.
			var r = (2.0 / 3.0) * (x + y + z);
			double xr = r - x, yr = r - y, zr = r - z;

			// Evaluate both lattices to form a BCC lattice.
			return noise3_BCC(xr, yr, zr);
		}

		/**
		 * 3D Re-oriented 4-point BCC noise, with better visual isotropy in (X, Y).
		 * Recommended for 3D terrain and time-varied animations.
		 * The Z coordinate should always be the "different" coordinate in your use case.
		 * If Y is vertical in world coordinates, call noise3_XYBeforeZ(x, z, Y) or use noise3_XZBeforeY.
		 * If Z is vertical in world coordinates, call noise3_XYBeforeZ(x, y, Z).
		 * For a time varied animation, call noise3_XYBeforeZ(x, y, T).
		 */
		public double noise3_XYBeforeZ(double x, double y, double z)
		{

			// Re-orient the cubic lattices without skewing, to make X and Y triangular like 2D.
			// Orthonormal rotation. Not a skew transform.
			var xy = x + y;
			var s2 = xy * -0.211324865405187;
			var zz = z * 0.577350269189626;
			double xr = x + s2 - zz, yr = y + s2 - zz;
			var zr = xy * 0.577350269189626 + zz;

			// Evaluate both lattices to form a BCC lattice.
			return noise3_BCC(xr, yr, zr);
		}

		/**
		 * 3D Re-oriented 4-point BCC noise, with better visual isotropy in (X, Z).
		 * Recommended for 3D terrain and time-varied animations.
		 * The Y coordinate should always be the "different" coordinate in your use case.
		 * If Y is vertical in world coordinates, call noise3_XZBeforeY(x, Y, z).
		 * If Z is vertical in world coordinates, call noise3_XZBeforeY(x, Z, y) or use noise3_XYBeforeZ.
		 * For a time varied animation, call noise3_XZBeforeY(x, T, y) or use noise3_XYBeforeZ.
		 */
		public double noise3_XZBeforeY(double x, double y, double z)
		{

			// Re-orient the cubic lattices without skewing, to make X and Z triangular like 2D.
			// Orthonormal rotation. Not a skew transform.
			var xz = x + z;
			var s2 = xz * -0.211324865405187;
			var yy = y * 0.577350269189626;
			var xr = x + s2 - yy;
			var zr = z + s2 - yy;
			var yr = xz * 0.577350269189626 + yy;

			// Evaluate both lattices to form a BCC lattice.
			return noise3_BCC(xr, yr, zr);
		}

		/**
		 * Generate overlapping cubic lattices for 3D Re-oriented BCC noise.
		 * Lookup table implementation inspired by DigitalShadow.
		 * It was actually faster to narrow down the points in the loop itself,
		 * than to build up the index with enough info to isolate 4 points.
		 */
		private double noise3_BCC(double xr, double yr, double zr)
		{

			// Get base and offsets inside cube of first lattice.
			int xrb = FastFloor(xr), yrb = FastFloor(yr), zrb = FastFloor(zr);
			double xri = xr - xrb, yri = yr - yrb, zri = zr - zrb;

			// Identify which octant of the cube we're in. This determines which cell
			// in the other cubic lattice we're in, and also narrows down one point on each.
			int xht = (int)(xri + 0.5), yht = (int)(yri + 0.5), zht = (int)(zri + 0.5);
			var index = (xht << 0) | (yht << 1) | (zht << 2);

			// Point contributions
			double value = 0;
			var c = Lookup3D[index];
			while (c != null)
			{
				double dxr = xri + c.Dxr, dyr = yri + c.Dyr, dzr = zri + c.Dzr;
				var attn = 0.5 - dxr * dxr - dyr * dyr - dzr * dzr;
				if (attn < 0)
				{
					c = c.NextOnFailure;
				}
				else
				{
					int pxm = (xrb + c.Xrv) & Pmask, pym = (yrb + c.Yrv) & Pmask, pzm = (zrb + c.Zrv) & Pmask;
					var grad = _permGrad3[_perm[_perm[pxm] ^ pym] ^ pzm];
					var extrapolation = grad.Dx * dxr + grad.Dy * dyr + grad.Dz * dzr;

					attn *= attn;
					value += attn * attn * extrapolation;
					c = c.NextOnSuccess;
				}
			}

			return value;
		}

		/**
		 * 4D OpenSimplex2F noise, classic lattice orientation.
		 */
		public double noise4_Classic(double x, double y, double z, double w)
		{

			// Get points for A4 lattice
			var s = -0.138196601125011 * (x + y + z + w);
			double xs = x + s, ys = y + s, zs = z + s, ws = w + s;

			return noise4_Base(xs, ys, zs, ws);
		}

		/**
		 * 4D OpenSimplex2F noise, with XY and ZW forming orthogonal triangular-based planes.
		 * Recommended for 3D terrain, where X and Y (or Z and W) are horizontal.
		 * Recommended for noise(x, y, sin(time), cos(time)) trick.
		 */
		public double noise4_XYBeforeZW(double x, double y, double z, double w)
		{

			var s2 = (x + y) * -0.178275657951399372 + (z + w) * 0.215623393288842828;
			var t2 = (z + w) * -0.403949762580207112 + (x + y) * -0.375199083010075342;
			double xs = x + s2, ys = y + s2, zs = z + t2, ws = w + t2;

			return noise4_Base(xs, ys, zs, ws);
		}

		/**
		 * 4D OpenSimplex2F noise, with XZ and YW forming orthogonal triangular-based planes.
		 * Recommended for 3D terrain, where X and Z (or Y and W) are horizontal.
		 */
		public double noise4_XZBeforeYW(double x, double y, double z, double w)
		{

			var s2 = (x + z) * -0.178275657951399372 + (y + w) * 0.215623393288842828;
			var t2 = (y + w) * -0.403949762580207112 + (x + z) * -0.375199083010075342;
			double xs = x + s2, ys = y + t2, zs = z + s2, ws = w + t2;

			return noise4_Base(xs, ys, zs, ws);
		}

		/**
		 * 4D OpenSimplex2F noise, with XYZ oriented like noise3_Classic,
		 * and W for an extra degree of freedom. W repeats eventually.
		 * Recommended for time-varied animations which texture a 3D object (W=time)
		 */
		public double noise4_XYZBeforeW(double x, double y, double z, double w)
		{

			var xyz = x + y + z;
			var ww = w * 0.2236067977499788;
			var s2 = xyz * -0.16666666666666666 + ww;
			double xs = x + s2, ys = y + s2, zs = z + s2, ws = -0.5 * xyz + ww;

			return noise4_Base(xs, ys, zs, ws);
		}

		/**
		 * 4D OpenSimplex2F noise base.
		 * Current implementation not fully optimized by lookup tables.
		 * But still comes out slightly ahead of Gustavson's Simplex in tests.
		 */
		private double noise4_Base(double xs, double ys, double zs, double ws)
		{
			double value = 0;

			// Get base points and offsets
			int xsb = FastFloor(xs), ysb = FastFloor(ys), zsb = FastFloor(zs), wsb = FastFloor(ws);
			double xsi = xs - xsb, ysi = ys - ysb, zsi = zs - zsb, wsi = ws - wsb;

			// If we're in the lower half, flip so we can repeat the code for the upper half. We'll flip back later.
			var siSum = xsi + ysi + zsi + wsi;
			var ssi = siSum * 0.309016994374947; // Prep for vertex contributions.
			var inLowerHalf = (siSum < 2);
			if (inLowerHalf)
			{
				xsi = 1 - xsi;
				ysi = 1 - ysi;
				zsi = 1 - zsi;
				wsi = 1 - wsi;
				siSum = 4 - siSum;
			}

			// Consider opposing vertex pairs of the octahedron formed by the central cross-section of the stretched tesseract
			double aabb = xsi + ysi - zsi - wsi, abab = xsi - ysi + zsi - wsi, abba = xsi - ysi - zsi + wsi;
			double aabbScore = Math.Abs(aabb), ababScore = Math.Abs(abab), abbaScore = Math.Abs(abba);

			// Find the closest point on the stretched tesseract as if it were the upper half
			int vertexIndex, via, vib;
			double asi, bsi;
			if (aabbScore > ababScore && aabbScore > abbaScore)
			{
				if (aabb > 0)
				{
					asi = zsi;
					bsi = wsi;
					vertexIndex = 0b0011;
					via = 0b0111;
					vib = 0b1011;
				}
				else
				{
					asi = xsi;
					bsi = ysi;
					vertexIndex = 0b1100;
					via = 0b1101;
					vib = 0b1110;
				}
			}
			else if (ababScore > abbaScore)
			{
				if (abab > 0)
				{
					asi = ysi;
					bsi = wsi;
					vertexIndex = 0b0101;
					via = 0b0111;
					vib = 0b1101;
				}
				else
				{
					asi = xsi;
					bsi = zsi;
					vertexIndex = 0b1010;
					via = 0b1011;
					vib = 0b1110;
				}
			}
			else
			{
				if (abba > 0)
				{
					asi = ysi;
					bsi = zsi;
					vertexIndex = 0b1001;
					via = 0b1011;
					vib = 0b1101;
				}
				else
				{
					asi = xsi;
					bsi = wsi;
					vertexIndex = 0b0110;
					via = 0b0111;
					vib = 0b1110;
				}
			}

			if (bsi > asi)
			{
				via = vib;
				var temp = bsi;
				bsi = asi;
				asi = temp;
			}

			if (siSum + asi > 3)
			{
				vertexIndex = via;
				if (siSum + bsi > 4)
				{
					vertexIndex = 0b1111;
				}
			}

			// Now flip back if we're actually in the lower half.
			if (inLowerHalf)
			{
				xsi = 1 - xsi;
				ysi = 1 - ysi;
				zsi = 1 - zsi;
				wsi = 1 - wsi;
				vertexIndex ^= 0b1111;
			}

			// Five points to add, total, from five copies of the A4 lattice.
			for (var i = 0; i < 5; i++)
			{

				// Update xsb/etc. and add the lattice point's contribution.
				var c = Vertices4D[vertexIndex];
				xsb += c.Xsv;
				ysb += c.Ysv;
				zsb += c.Zsv;
				wsb += c.Wsv;
				double xi = xsi + ssi, yi = ysi + ssi, zi = zsi + ssi, wi = wsi + ssi;
				double dx = xi + c.Dx, dy = yi + c.Dy, dz = zi + c.Dz, dw = wi + c.Dw;
				var attn = 0.5 - dx * dx - dy * dy - dz * dz - dw * dw;
				if (attn > 0)
				{
					int pxm = xsb & Pmask, pym = ysb & Pmask, pzm = zsb & Pmask, pwm = wsb & Pmask;
					var grad = _permGrad4[_perm[_perm[_perm[pxm] ^ pym] ^ pzm] ^ pwm];
					var ramped = grad.Dx * dx + grad.Dy * dy + grad.Dz * dz + grad.Dw * dw;

					attn *= attn;
					value += attn * attn * ramped;
				}

				// Maybe this helps the compiler/JVM/LLVM/etc. know we can end the loop here. Maybe not.
				if (i == 4)
					break;

				// Update the relative skewed coordinates to reference the vertex we just added.
				// Rather, reference its counterpart on the lattice copy that is shifted down by
				// the vector <-0.2, -0.2, -0.2, -0.2>
				xsi += c.Xsi;
				ysi += c.Ysi;
				zsi += c.Zsi;
				wsi += c.Wsi;
				ssi += c.SsiDelta;

				// Next point is the closest vertex on the 4-simplex whose base vertex is the aforementioned vertex.
				var score0 =
					1.0 + ssi * (-1.0 / 0.309016994374947); // Seems slightly faster than 1.0-xsi-ysi-zsi-wsi
				vertexIndex = 0b0000;
				if (xsi >= ysi && xsi >= zsi && xsi >= wsi && xsi >= score0)
				{
					vertexIndex = 0b0001;
				}
				else if (ysi > xsi && ysi >= zsi && ysi >= wsi && ysi >= score0)
				{
					vertexIndex = 0b0010;
				}
				else if (zsi > xsi && zsi > ysi && zsi >= wsi && zsi >= score0)
				{
					vertexIndex = 0b0100;
				}
				else if (wsi > xsi && wsi > ysi && wsi > zsi && wsi >= score0)
				{
					vertexIndex = 0b1000;
				}
			}

			return value;
		}

		/*
		 * Utility
		 */

		private static int FastFloor(double x)
		{
			var xi = (int)x;
			return x < xi ? xi - 1 : xi;
		}

		/*
		 * Definitions
		 */

		private static readonly LatticePoint2D[] Lookup2D;
		private static readonly LatticePoint3D[] Lookup3D;
		private static readonly LatticePoint4D[] Vertices4D;

		static OpenSimplex2F()
		{
			Lookup2D = new LatticePoint2D[4];
			Lookup3D = new LatticePoint3D[8];
			Vertices4D = new LatticePoint4D[16];

			Lookup2D[0] = new LatticePoint2D(1, 0);
			Lookup2D[1] = new LatticePoint2D(0, 0);
			Lookup2D[2] = new LatticePoint2D(1, 1);
			Lookup2D[3] = new LatticePoint2D(0, 1);

			for (var i = 0; i < 8; i++)
			{
				var i1 = (i >> 0) & 1;
				var j1 = (i >> 1) & 1;
				var k1 = (i >> 2) & 1;
				var i2 = i1 ^ 1;
				var j2 = j1 ^ 1;
				var k2 = k1 ^ 1;

				// The two points within this octant, one from each of the two cubic half-lattices.
				var c0 = new LatticePoint3D(i1, j1, k1, 0);
				var c1 = new LatticePoint3D(i1 + i2, j1 + j2, k1 + k2, 1);

				// Each single step away on the first half-lattice.
				var c2 = new LatticePoint3D(i1 ^ 1, j1, k1, 0);
				var c3 = new LatticePoint3D(i1, j1 ^ 1, k1, 0);
				var c4 = new LatticePoint3D(i1, j1, k1 ^ 1, 0);

				// Each single step away on the second half-lattice.
				var c5 = new LatticePoint3D(i1 + (i2 ^ 1), j1 + j2, k1 + k2, 1);
				var c6 = new LatticePoint3D(i1 + i2, j1 + (j2 ^ 1), k1 + k2, 1);
				var c7 = new LatticePoint3D(i1 + i2, j1 + j2, k1 + (k2 ^ 1), 1);

				// First two are guaranteed.
				c0.NextOnFailure = c0.NextOnSuccess = c1;
				c1.NextOnFailure = c1.NextOnSuccess = c2;

				// Once we find one on the first half-lattice, the rest are out.
				// In addition, knowing c2 rules out c5.
				c2.NextOnFailure = c3;
				c2.NextOnSuccess = c6;
				c3.NextOnFailure = c4;
				c3.NextOnSuccess = c5;
				c4.NextOnFailure = c4.NextOnSuccess = c5;

				// Once we find one on the second half-lattice, the rest are out.
				c5.NextOnFailure = c6;
				c5.NextOnSuccess = null;
				c6.NextOnFailure = c7;
				c6.NextOnSuccess = null;
				c7.NextOnFailure = c7.NextOnSuccess = null;

				Lookup3D[i] = c0;
			}

			for (var i = 0; i < 16; i++)
			{
				Vertices4D[i] = new LatticePoint4D((i >> 0) & 1, (i >> 1) & 1, (i >> 2) & 1, (i >> 3) & 1);
			}

			Gradients2D = new Grad2[Psize];
			Grad2[] grad2 =
			{
				new Grad2(0.130526192220052, 0.99144486137381),
				new Grad2(0.38268343236509, 0.923879532511287),
				new Grad2(0.608761429008721, 0.793353340291235),
				new Grad2(0.793353340291235, 0.608761429008721),
				new Grad2(0.923879532511287, 0.38268343236509),
				new Grad2(0.99144486137381, 0.130526192220051),
				new Grad2(0.99144486137381, -0.130526192220051),
				new Grad2(0.923879532511287, -0.38268343236509),
				new Grad2(0.793353340291235, -0.60876142900872),
				new Grad2(0.608761429008721, -0.793353340291235),
				new Grad2(0.38268343236509, -0.923879532511287),
				new Grad2(0.130526192220052, -0.99144486137381),
				new Grad2(-0.130526192220052, -0.99144486137381),
				new Grad2(-0.38268343236509, -0.923879532511287),
				new Grad2(-0.608761429008721, -0.793353340291235),
				new Grad2(-0.793353340291235, -0.608761429008721),
				new Grad2(-0.923879532511287, -0.38268343236509),
				new Grad2(-0.99144486137381, -0.130526192220052),
				new Grad2(-0.99144486137381, 0.130526192220051),
				new Grad2(-0.923879532511287, 0.38268343236509),
				new Grad2(-0.793353340291235, 0.608761429008721),
				new Grad2(-0.608761429008721, 0.793353340291235),
				new Grad2(-0.38268343236509, 0.923879532511287),
				new Grad2(-0.130526192220052, 0.99144486137381)
			};

			foreach (var t in grad2)
			{
				t.Dx /= N2;
				t.Dy /= N2;
			}

			for (var i = 0; i < Psize; i++)
			{
				Gradients2D[i] = grad2[i % grad2.Length];
			}

			Gradients3D = new Grad3[Psize];
			Grad3[] grad3 =
			{
				new Grad3(-2.22474487139, -2.22474487139, -1.0),
				new Grad3(-2.22474487139, -2.22474487139, 1.0),
				new Grad3(-3.0862664687972017, -1.1721513422464978, 0.0),
				new Grad3(-1.1721513422464978, -3.0862664687972017, 0.0),
				new Grad3(-2.22474487139, -1.0, -2.22474487139),
				new Grad3(-2.22474487139, 1.0, -2.22474487139),
				new Grad3(-1.1721513422464978, 0.0, -3.0862664687972017),
				new Grad3(-3.0862664687972017, 0.0, -1.1721513422464978),
				new Grad3(-2.22474487139, -1.0, 2.22474487139),
				new Grad3(-2.22474487139, 1.0, 2.22474487139),
				new Grad3(-3.0862664687972017, 0.0, 1.1721513422464978),
				new Grad3(-1.1721513422464978, 0.0, 3.0862664687972017),
				new Grad3(-2.22474487139, 2.22474487139, -1.0),
				new Grad3(-2.22474487139, 2.22474487139, 1.0),
				new Grad3(-1.1721513422464978, 3.0862664687972017, 0.0),
				new Grad3(-3.0862664687972017, 1.1721513422464978, 0.0),
				new Grad3(-1.0, -2.22474487139, -2.22474487139),
				new Grad3(1.0, -2.22474487139, -2.22474487139),
				new Grad3(0.0, -3.0862664687972017, -1.1721513422464978),
				new Grad3(0.0, -1.1721513422464978, -3.0862664687972017),
				new Grad3(-1.0, -2.22474487139, 2.22474487139),
				new Grad3(1.0, -2.22474487139, 2.22474487139),
				new Grad3(0.0, -1.1721513422464978, 3.0862664687972017),
				new Grad3(0.0, -3.0862664687972017, 1.1721513422464978),
				new Grad3(-1.0, 2.22474487139, -2.22474487139),
				new Grad3(1.0, 2.22474487139, -2.22474487139),
				new Grad3(0.0, 1.1721513422464978, -3.0862664687972017),
				new Grad3(0.0, 3.0862664687972017, -1.1721513422464978),
				new Grad3(-1.0, 2.22474487139, 2.22474487139),
				new Grad3(1.0, 2.22474487139, 2.22474487139),
				new Grad3(0.0, 3.0862664687972017, 1.1721513422464978),
				new Grad3(0.0, 1.1721513422464978, 3.0862664687972017),
				new Grad3(2.22474487139, -2.22474487139, -1.0),
				new Grad3(2.22474487139, -2.22474487139, 1.0),
				new Grad3(1.1721513422464978, -3.0862664687972017, 0.0),
				new Grad3(3.0862664687972017, -1.1721513422464978, 0.0),
				new Grad3(2.22474487139, -1.0, -2.22474487139),
				new Grad3(2.22474487139, 1.0, -2.22474487139),
				new Grad3(3.0862664687972017, 0.0, -1.1721513422464978),
				new Grad3(1.1721513422464978, 0.0, -3.0862664687972017),
				new Grad3(2.22474487139, -1.0, 2.22474487139),
				new Grad3(2.22474487139, 1.0, 2.22474487139),
				new Grad3(1.1721513422464978, 0.0, 3.0862664687972017),
				new Grad3(3.0862664687972017, 0.0, 1.1721513422464978),
				new Grad3(2.22474487139, 2.22474487139, -1.0),
				new Grad3(2.22474487139, 2.22474487139, 1.0),
				new Grad3(3.0862664687972017, 1.1721513422464978, 0.0),
				new Grad3(1.1721513422464978, 3.0862664687972017, 0.0)
			};

			foreach (var t in grad3)
			{
				t.Dx /= N3;
				t.Dy /= N3;
				t.Dz /= N3;
			}

			for (var i = 0; i < Psize; i++)
			{
				Gradients3D[i] = grad3[i % grad3.Length];
			}

			Gradients4D = new Grad4[Psize];
			Grad4[] grad4 =
			{
				new Grad4(-0.753341017856078, -0.37968289875261624, -0.37968289875261624, -0.37968289875261624),
				new Grad4(-0.7821684431180708, -0.4321472685365301, -0.4321472685365301, 0.12128480194602098),
				new Grad4(-0.7821684431180708, -0.4321472685365301, 0.12128480194602098, -0.4321472685365301),
				new Grad4(-0.7821684431180708, 0.12128480194602098, -0.4321472685365301, -0.4321472685365301),
				new Grad4(-0.8586508742123365, -0.508629699630796, 0.044802370851755174, 0.044802370851755174),
				new Grad4(-0.8586508742123365, 0.044802370851755174, -0.508629699630796, 0.044802370851755174),
				new Grad4(-0.8586508742123365, 0.044802370851755174, 0.044802370851755174, -0.508629699630796),
				new Grad4(-0.9982828964265062, -0.03381941603233842, -0.03381941603233842, -0.03381941603233842),
				new Grad4(-0.37968289875261624, -0.753341017856078, -0.37968289875261624, -0.37968289875261624),
				new Grad4(-0.4321472685365301, -0.7821684431180708, -0.4321472685365301, 0.12128480194602098),
				new Grad4(-0.4321472685365301, -0.7821684431180708, 0.12128480194602098, -0.4321472685365301),
				new Grad4(0.12128480194602098, -0.7821684431180708, -0.4321472685365301, -0.4321472685365301),
				new Grad4(-0.508629699630796, -0.8586508742123365, 0.044802370851755174, 0.044802370851755174),
				new Grad4(0.044802370851755174, -0.8586508742123365, -0.508629699630796, 0.044802370851755174),
				new Grad4(0.044802370851755174, -0.8586508742123365, 0.044802370851755174, -0.508629699630796),
				new Grad4(-0.03381941603233842, -0.9982828964265062, -0.03381941603233842, -0.03381941603233842),
				new Grad4(-0.37968289875261624, -0.37968289875261624, -0.753341017856078, -0.37968289875261624),
				new Grad4(-0.4321472685365301, -0.4321472685365301, -0.7821684431180708, 0.12128480194602098),
				new Grad4(-0.4321472685365301, 0.12128480194602098, -0.7821684431180708, -0.4321472685365301),
				new Grad4(0.12128480194602098, -0.4321472685365301, -0.7821684431180708, -0.4321472685365301),
				new Grad4(-0.508629699630796, 0.044802370851755174, -0.8586508742123365, 0.044802370851755174),
				new Grad4(0.044802370851755174, -0.508629699630796, -0.8586508742123365, 0.044802370851755174),
				new Grad4(0.044802370851755174, 0.044802370851755174, -0.8586508742123365, -0.508629699630796),
				new Grad4(-0.03381941603233842, -0.03381941603233842, -0.9982828964265062, -0.03381941603233842),
				new Grad4(-0.37968289875261624, -0.37968289875261624, -0.37968289875261624, -0.753341017856078),
				new Grad4(-0.4321472685365301, -0.4321472685365301, 0.12128480194602098, -0.7821684431180708),
				new Grad4(-0.4321472685365301, 0.12128480194602098, -0.4321472685365301, -0.7821684431180708),
				new Grad4(0.12128480194602098, -0.4321472685365301, -0.4321472685365301, -0.7821684431180708),
				new Grad4(-0.508629699630796, 0.044802370851755174, 0.044802370851755174, -0.8586508742123365),
				new Grad4(0.044802370851755174, -0.508629699630796, 0.044802370851755174, -0.8586508742123365),
				new Grad4(0.044802370851755174, 0.044802370851755174, -0.508629699630796, -0.8586508742123365),
				new Grad4(-0.03381941603233842, -0.03381941603233842, -0.03381941603233842, -0.9982828964265062),
				new Grad4(-0.6740059517812944, -0.3239847771997537, -0.3239847771997537, 0.5794684678643381),
				new Grad4(-0.7504883828755602, -0.4004672082940195, 0.15296486218853164, 0.5029860367700724),
				new Grad4(-0.7504883828755602, 0.15296486218853164, -0.4004672082940195, 0.5029860367700724),
				new Grad4(-0.8828161875373585, 0.08164729285680945, 0.08164729285680945, 0.4553054119602712),
				new Grad4(-0.4553054119602712, -0.08164729285680945, -0.08164729285680945, 0.8828161875373585),
				new Grad4(-0.5029860367700724, -0.15296486218853164, 0.4004672082940195, 0.7504883828755602),
				new Grad4(-0.5029860367700724, 0.4004672082940195, -0.15296486218853164, 0.7504883828755602),
				new Grad4(-0.5794684678643381, 0.3239847771997537, 0.3239847771997537, 0.6740059517812944),
				new Grad4(-0.3239847771997537, -0.6740059517812944, -0.3239847771997537, 0.5794684678643381),
				new Grad4(-0.4004672082940195, -0.7504883828755602, 0.15296486218853164, 0.5029860367700724),
				new Grad4(0.15296486218853164, -0.7504883828755602, -0.4004672082940195, 0.5029860367700724),
				new Grad4(0.08164729285680945, -0.8828161875373585, 0.08164729285680945, 0.4553054119602712),
				new Grad4(-0.08164729285680945, -0.4553054119602712, -0.08164729285680945, 0.8828161875373585),
				new Grad4(-0.15296486218853164, -0.5029860367700724, 0.4004672082940195, 0.7504883828755602),
				new Grad4(0.4004672082940195, -0.5029860367700724, -0.15296486218853164, 0.7504883828755602),
				new Grad4(0.3239847771997537, -0.5794684678643381, 0.3239847771997537, 0.6740059517812944),
				new Grad4(-0.3239847771997537, -0.3239847771997537, -0.6740059517812944, 0.5794684678643381),
				new Grad4(-0.4004672082940195, 0.15296486218853164, -0.7504883828755602, 0.5029860367700724),
				new Grad4(0.15296486218853164, -0.4004672082940195, -0.7504883828755602, 0.5029860367700724),
				new Grad4(0.08164729285680945, 0.08164729285680945, -0.8828161875373585, 0.4553054119602712),
				new Grad4(-0.08164729285680945, -0.08164729285680945, -0.4553054119602712, 0.8828161875373585),
				new Grad4(-0.15296486218853164, 0.4004672082940195, -0.5029860367700724, 0.7504883828755602),
				new Grad4(0.4004672082940195, -0.15296486218853164, -0.5029860367700724, 0.7504883828755602),
				new Grad4(0.3239847771997537, 0.3239847771997537, -0.5794684678643381, 0.6740059517812944),
				new Grad4(-0.6740059517812944, -0.3239847771997537, 0.5794684678643381, -0.3239847771997537),
				new Grad4(-0.7504883828755602, -0.4004672082940195, 0.5029860367700724, 0.15296486218853164),
				new Grad4(-0.7504883828755602, 0.15296486218853164, 0.5029860367700724, -0.4004672082940195),
				new Grad4(-0.8828161875373585, 0.08164729285680945, 0.4553054119602712, 0.08164729285680945),
				new Grad4(-0.4553054119602712, -0.08164729285680945, 0.8828161875373585, -0.08164729285680945),
				new Grad4(-0.5029860367700724, -0.15296486218853164, 0.7504883828755602, 0.4004672082940195),
				new Grad4(-0.5029860367700724, 0.4004672082940195, 0.7504883828755602, -0.15296486218853164),
				new Grad4(-0.5794684678643381, 0.3239847771997537, 0.6740059517812944, 0.3239847771997537),
				new Grad4(-0.3239847771997537, -0.6740059517812944, 0.5794684678643381, -0.3239847771997537),
				new Grad4(-0.4004672082940195, -0.7504883828755602, 0.5029860367700724, 0.15296486218853164),
				new Grad4(0.15296486218853164, -0.7504883828755602, 0.5029860367700724, -0.4004672082940195),
				new Grad4(0.08164729285680945, -0.8828161875373585, 0.4553054119602712, 0.08164729285680945),
				new Grad4(-0.08164729285680945, -0.4553054119602712, 0.8828161875373585, -0.08164729285680945),
				new Grad4(-0.15296486218853164, -0.5029860367700724, 0.7504883828755602, 0.4004672082940195),
				new Grad4(0.4004672082940195, -0.5029860367700724, 0.7504883828755602, -0.15296486218853164),
				new Grad4(0.3239847771997537, -0.5794684678643381, 0.6740059517812944, 0.3239847771997537),
				new Grad4(-0.3239847771997537, -0.3239847771997537, 0.5794684678643381, -0.6740059517812944),
				new Grad4(-0.4004672082940195, 0.15296486218853164, 0.5029860367700724, -0.7504883828755602),
				new Grad4(0.15296486218853164, -0.4004672082940195, 0.5029860367700724, -0.7504883828755602),
				new Grad4(0.08164729285680945, 0.08164729285680945, 0.4553054119602712, -0.8828161875373585),
				new Grad4(-0.08164729285680945, -0.08164729285680945, 0.8828161875373585, -0.4553054119602712),
				new Grad4(-0.15296486218853164, 0.4004672082940195, 0.7504883828755602, -0.5029860367700724),
				new Grad4(0.4004672082940195, -0.15296486218853164, 0.7504883828755602, -0.5029860367700724),
				new Grad4(0.3239847771997537, 0.3239847771997537, 0.6740059517812944, -0.5794684678643381),
				new Grad4(-0.6740059517812944, 0.5794684678643381, -0.3239847771997537, -0.3239847771997537),
				new Grad4(-0.7504883828755602, 0.5029860367700724, -0.4004672082940195, 0.15296486218853164),
				new Grad4(-0.7504883828755602, 0.5029860367700724, 0.15296486218853164, -0.4004672082940195),
				new Grad4(-0.8828161875373585, 0.4553054119602712, 0.08164729285680945, 0.08164729285680945),
				new Grad4(-0.4553054119602712, 0.8828161875373585, -0.08164729285680945, -0.08164729285680945),
				new Grad4(-0.5029860367700724, 0.7504883828755602, -0.15296486218853164, 0.4004672082940195),
				new Grad4(-0.5029860367700724, 0.7504883828755602, 0.4004672082940195, -0.15296486218853164),
				new Grad4(-0.5794684678643381, 0.6740059517812944, 0.3239847771997537, 0.3239847771997537),
				new Grad4(-0.3239847771997537, 0.5794684678643381, -0.6740059517812944, -0.3239847771997537),
				new Grad4(-0.4004672082940195, 0.5029860367700724, -0.7504883828755602, 0.15296486218853164),
				new Grad4(0.15296486218853164, 0.5029860367700724, -0.7504883828755602, -0.4004672082940195),
				new Grad4(0.08164729285680945, 0.4553054119602712, -0.8828161875373585, 0.08164729285680945),
				new Grad4(-0.08164729285680945, 0.8828161875373585, -0.4553054119602712, -0.08164729285680945),
				new Grad4(-0.15296486218853164, 0.7504883828755602, -0.5029860367700724, 0.4004672082940195),
				new Grad4(0.4004672082940195, 0.7504883828755602, -0.5029860367700724, -0.15296486218853164),
				new Grad4(0.3239847771997537, 0.6740059517812944, -0.5794684678643381, 0.3239847771997537),
				new Grad4(-0.3239847771997537, 0.5794684678643381, -0.3239847771997537, -0.6740059517812944),
				new Grad4(-0.4004672082940195, 0.5029860367700724, 0.15296486218853164, -0.7504883828755602),
				new Grad4(0.15296486218853164, 0.5029860367700724, -0.4004672082940195, -0.7504883828755602),
				new Grad4(0.08164729285680945, 0.4553054119602712, 0.08164729285680945, -0.8828161875373585),
				new Grad4(-0.08164729285680945, 0.8828161875373585, -0.08164729285680945, -0.4553054119602712),
				new Grad4(-0.15296486218853164, 0.7504883828755602, 0.4004672082940195, -0.5029860367700724),
				new Grad4(0.4004672082940195, 0.7504883828755602, -0.15296486218853164, -0.5029860367700724),
				new Grad4(0.3239847771997537, 0.6740059517812944, 0.3239847771997537, -0.5794684678643381),
				new Grad4(0.5794684678643381, -0.6740059517812944, -0.3239847771997537, -0.3239847771997537),
				new Grad4(0.5029860367700724, -0.7504883828755602, -0.4004672082940195, 0.15296486218853164),
				new Grad4(0.5029860367700724, -0.7504883828755602, 0.15296486218853164, -0.4004672082940195),
				new Grad4(0.4553054119602712, -0.8828161875373585, 0.08164729285680945, 0.08164729285680945),
				new Grad4(0.8828161875373585, -0.4553054119602712, -0.08164729285680945, -0.08164729285680945),
				new Grad4(0.7504883828755602, -0.5029860367700724, -0.15296486218853164, 0.4004672082940195),
				new Grad4(0.7504883828755602, -0.5029860367700724, 0.4004672082940195, -0.15296486218853164),
				new Grad4(0.6740059517812944, -0.5794684678643381, 0.3239847771997537, 0.3239847771997537),
				new Grad4(0.5794684678643381, -0.3239847771997537, -0.6740059517812944, -0.3239847771997537),
				new Grad4(0.5029860367700724, -0.4004672082940195, -0.7504883828755602, 0.15296486218853164),
				new Grad4(0.5029860367700724, 0.15296486218853164, -0.7504883828755602, -0.4004672082940195),
				new Grad4(0.4553054119602712, 0.08164729285680945, -0.8828161875373585, 0.08164729285680945),
				new Grad4(0.8828161875373585, -0.08164729285680945, -0.4553054119602712, -0.08164729285680945),
				new Grad4(0.7504883828755602, -0.15296486218853164, -0.5029860367700724, 0.4004672082940195),
				new Grad4(0.7504883828755602, 0.4004672082940195, -0.5029860367700724, -0.15296486218853164),
				new Grad4(0.6740059517812944, 0.3239847771997537, -0.5794684678643381, 0.3239847771997537),
				new Grad4(0.5794684678643381, -0.3239847771997537, -0.3239847771997537, -0.6740059517812944),
				new Grad4(0.5029860367700724, -0.4004672082940195, 0.15296486218853164, -0.7504883828755602),
				new Grad4(0.5029860367700724, 0.15296486218853164, -0.4004672082940195, -0.7504883828755602),
				new Grad4(0.4553054119602712, 0.08164729285680945, 0.08164729285680945, -0.8828161875373585),
				new Grad4(0.8828161875373585, -0.08164729285680945, -0.08164729285680945, -0.4553054119602712),
				new Grad4(0.7504883828755602, -0.15296486218853164, 0.4004672082940195, -0.5029860367700724),
				new Grad4(0.7504883828755602, 0.4004672082940195, -0.15296486218853164, -0.5029860367700724),
				new Grad4(0.6740059517812944, 0.3239847771997537, 0.3239847771997537, -0.5794684678643381),
				new Grad4(0.03381941603233842, 0.03381941603233842, 0.03381941603233842, 0.9982828964265062),
				new Grad4(-0.044802370851755174, -0.044802370851755174, 0.508629699630796, 0.8586508742123365),
				new Grad4(-0.044802370851755174, 0.508629699630796, -0.044802370851755174, 0.8586508742123365),
				new Grad4(-0.12128480194602098, 0.4321472685365301, 0.4321472685365301, 0.7821684431180708),
				new Grad4(0.508629699630796, -0.044802370851755174, -0.044802370851755174, 0.8586508742123365),
				new Grad4(0.4321472685365301, -0.12128480194602098, 0.4321472685365301, 0.7821684431180708),
				new Grad4(0.4321472685365301, 0.4321472685365301, -0.12128480194602098, 0.7821684431180708),
				new Grad4(0.37968289875261624, 0.37968289875261624, 0.37968289875261624, 0.753341017856078),
				new Grad4(0.03381941603233842, 0.03381941603233842, 0.9982828964265062, 0.03381941603233842),
				new Grad4(-0.044802370851755174, 0.044802370851755174, 0.8586508742123365, 0.508629699630796),
				new Grad4(-0.044802370851755174, 0.508629699630796, 0.8586508742123365, -0.044802370851755174),
				new Grad4(-0.12128480194602098, 0.4321472685365301, 0.7821684431180708, 0.4321472685365301),
				new Grad4(0.508629699630796, -0.044802370851755174, 0.8586508742123365, -0.044802370851755174),
				new Grad4(0.4321472685365301, -0.12128480194602098, 0.7821684431180708, 0.4321472685365301),
				new Grad4(0.4321472685365301, 0.4321472685365301, 0.7821684431180708, -0.12128480194602098),
				new Grad4(0.37968289875261624, 0.37968289875261624, 0.753341017856078, 0.37968289875261624),
				new Grad4(0.03381941603233842, 0.9982828964265062, 0.03381941603233842, 0.03381941603233842),
				new Grad4(-0.044802370851755174, 0.8586508742123365, -0.044802370851755174, 0.508629699630796),
				new Grad4(-0.044802370851755174, 0.8586508742123365, 0.508629699630796, -0.044802370851755174),
				new Grad4(-0.12128480194602098, 0.7821684431180708, 0.4321472685365301, 0.4321472685365301),
				new Grad4(0.508629699630796, 0.8586508742123365, -0.044802370851755174, -0.044802370851755174),
				new Grad4(0.4321472685365301, 0.7821684431180708, -0.12128480194602098, 0.4321472685365301),
				new Grad4(0.4321472685365301, 0.7821684431180708, 0.4321472685365301, -0.12128480194602098),
				new Grad4(0.37968289875261624, 0.753341017856078, 0.37968289875261624, 0.37968289875261624),
				new Grad4(0.9982828964265062, 0.03381941603233842, 0.03381941603233842, 0.03381941603233842),
				new Grad4(0.8586508742123365, -0.044802370851755174, -0.044802370851755174, 0.508629699630796),
				new Grad4(0.8586508742123365, -0.044802370851755174, 0.508629699630796, -0.044802370851755174),
				new Grad4(0.7821684431180708, -0.12128480194602098, 0.4321472685365301, 0.4321472685365301),
				new Grad4(0.8586508742123365, 0.508629699630796, -0.044802370851755174, -0.044802370851755174),
				new Grad4(0.7821684431180708, 0.4321472685365301, -0.12128480194602098, 0.4321472685365301),
				new Grad4(0.7821684431180708, 0.4321472685365301, 0.4321472685365301, -0.12128480194602098),
				new Grad4(0.753341017856078, 0.37968289875261624, 0.37968289875261624, 0.37968289875261624)
			};

			foreach (var t in grad4)
			{
				t.Dx /= N4;
				t.Dy /= N4;
				t.Dz /= N4;
				t.Dw /= N4;
			}

			for (var i = 0; i < Psize; i++)
			{
				Gradients4D[i] = grad4[i % grad4.Length];
			}
		}

		private class LatticePoint2D
		{
			public readonly int Xsv;
			public readonly int Ysv;
			public readonly double Dx;
			public readonly double Dy;

			public LatticePoint2D(int xsv, int ysv)
			{
				Xsv = xsv;
				Ysv = ysv;
				var ssv = (xsv + ysv) * -0.211324865405187;
				Dx = -xsv - ssv;
				Dy = -ysv - ssv;
			}
		}

		private class LatticePoint3D
		{
			public readonly double Dxr;
			public readonly double Dyr;
			public readonly double Dzr;
			public readonly int Xrv;
			public readonly int Yrv;
			public readonly int Zrv;
			public LatticePoint3D NextOnFailure, NextOnSuccess;

			public LatticePoint3D(int xrv, int yrv, int zrv, int lattice)
			{
				Dxr = -xrv + lattice * 0.5;
				Dyr = -yrv + lattice * 0.5;
				Dzr = -zrv + lattice * 0.5;
				Xrv = xrv + lattice * 1024;
				Yrv = yrv + lattice * 1024;
				Zrv = zrv + lattice * 1024;
			}
		}

		private class LatticePoint4D
		{
			public readonly int Xsv;
			public readonly int Ysv;
			public readonly int Zsv;
			public readonly int Wsv;
			public readonly double Dx;
			public readonly double Dy;
			public readonly double Dz;
			public readonly double Dw;
			public readonly double Xsi;
			public readonly double Ysi;
			public readonly double Zsi;
			public readonly double Wsi;
			public readonly double SsiDelta;

			public LatticePoint4D(int xsv, int ysv, int zsv, int wsv)
			{
				Xsv = xsv + 409;
				Ysv = ysv + 409;
				Zsv = zsv + 409;
				Wsv = wsv + 409;
				var ssv = (xsv + ysv + zsv + wsv) * 0.309016994374947;
				Dx = -xsv - ssv;
				Dy = -ysv - ssv;
				Dz = -zsv - ssv;
				Dw = -wsv - ssv;
				Xsi = Xsi = 0.2 - xsv;
				Ysi = Ysi = 0.2 - ysv;
				Zsi = Zsi = 0.2 - zsv;
				Wsi = Wsi = 0.2 - wsv;
				SsiDelta = (0.8 - xsv - ysv - zsv - wsv) * 0.309016994374947;
			}
		}

		/*
		 * Gradients
		 */

		private class Grad2
		{
			public double Dx, Dy;

			public Grad2(double dx, double dy)
			{
				Dx = dx;
				Dy = dy;
			}
		}

		private class Grad3
		{
			public double Dx, Dy, Dz;

			public Grad3(double dx, double dy, double dz)
			{
				Dx = dx;
				Dy = dy;
				Dz = dz;
			}
		}

		private class Grad4
		{
			public double Dx, Dy, Dz, Dw;

			public Grad4(double dx, double dy, double dz, double dw)
			{
				Dx = dx;
				Dy = dy;
				Dz = dz;
				Dw = dw;
			}
		}

		private const double N2 = 0.01001634121365712;
		private const double N3 = 0.030485933181293584;
		private const double N4 = 0.009202377986303158;
		private static readonly Grad2[] Gradients2D;
		private static readonly Grad3[] Gradients3D;
		private static readonly Grad4[] Gradients4D;
	}
}