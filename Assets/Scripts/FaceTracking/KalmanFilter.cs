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
        private float k;
        private float p = DefaultP;

        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private float q;
        private float r;
        private T x;

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
            q = aQ;
            r = aR;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public T Update(T measurement, float? newQ = null, float? newR = null)
        {
            // update values if supplied.
            if (newQ != null && q != newQ) q = (float) newQ;
            if (newR != null && r != newR) r = (float) newR;

            // update measurement.
            {
                k = (p + q) / (p + q + r);
                p = r * (p + q) / (r + p + q);
            }

            // filter result back into calculation.
            dynamic dynamicMeasurement = measurement;
            dynamic result = x + (dynamicMeasurement - x) * k;
            x = result;
            return result;
        }

        public T Update(List<T> measurements, bool areMeasurementsNewestFirst = false, float? newQ = null,
            float? newR = null)
        {
            var result = default(T);
            int i = areMeasurementsNewestFirst ? measurements.Count - 1 : 0;

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
            p = 1;
            x = default;
            k = 0;
        }
    }
}