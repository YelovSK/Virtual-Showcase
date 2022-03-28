using System.Collections.Generic;

// source https://gist.github.com/davidfoster/48acce6c13e5f7f247dc5d5909dce349

namespace VirtualVitrine.FaceTracking
{
    public class KalmanFilter<T>
    {
        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        public const float DefaultQ = 0.000001f;
        public const float DefaultR = 0.01f;

        public const float DefaultP = 1f;
        private float _k;
        private float _p = DefaultP;

        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private float _q;
        private float _r;
        private T _x;

        //-----------------------------------------------------------------------------------------
        // Constructors:
        //-----------------------------------------------------------------------------------------

        // N.B. passing in DEFAULT_Q is necessary, even though we have the same value (as an optional parameter), because this
        // defines a parameterless constructor, allowing us to be new()'d in generics contexts.
        public KalmanFilter() : this(DefaultQ)
        {
        }

        public KalmanFilter(float aQ = DefaultQ, float aR = DefaultR)
        {
            _q = aQ;
            _r = aR;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public T Update(T measurement, float? newQ = null, float? newR = null)
        {
            // update values if supplied.
            if (newQ != null && _q != newQ) _q = (float) newQ;
            if (newR != null && _r != newR) _r = (float) newR;

            // update measurement.
            {
                _k = (_p + _q) / (_p + _q + _r);
                _p = _r * (_p + _q) / (_r + _p + _q);
            }

            // filter result back into calculation.
            dynamic dynamicMeasurement = measurement;
            var result = _x + (dynamicMeasurement - _x) * _k;
            _x = result;
            return result;
        }

        public T Update(List<T> measurements, bool areMeasurementsNewestFirst = false, float? newQ = null,
            float? newR = null)
        {
            var result = default(T);
            var i = areMeasurementsNewestFirst ? measurements.Count - 1 : 0;

            while (i < measurements.Count && i >= 0)
            {
                // decrement or increment the counter.
                if (areMeasurementsNewestFirst)
                    --i;
                else
                    ++i;

                result = Update(measurements[i], newQ, newR);
            }

            return result;
        }

        public void Reset()
        {
            _p = 1;
            _x = default;
            _k = 0;
        }
    }
}