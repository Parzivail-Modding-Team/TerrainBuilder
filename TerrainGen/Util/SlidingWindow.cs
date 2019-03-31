using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGen.Util
{
    public class SlidingWindow
    {
        private readonly int _length;
        private readonly double[] _readings;

        private int _readIndex;
        private double _total;
        private double _average;
        private double _oldAverage;

        public SlidingWindow(int length)
        {
            _length = length;
            _readings = new double[length];
            for (var i = 0; i < _readings.Length; i++)
                _readings[i] = 0;
        }

        public double GetAverage()
        {
            return _average;
        }

        public double GetOldAverage()
        {
            return _oldAverage;
        }

        public double Slide(double nextValue)
        {
            _total = _total - _readings[_readIndex];
            // read from the sensor:
            _readings[_readIndex] = nextValue;
            // add the reading to the total:
            _total = _total + _readings[_readIndex];
            // advance to the next position in the array:
            _readIndex = _readIndex + 1;

            // if we're at the end of the array...
            if (_readIndex >= _length)
            {
                // ...wrap around to the beginning:
                _readIndex = 0;
            }

            _oldAverage = _average;
            // calculate the average:
            _average = _total / _length;
            return _average;
        }
    }
}
