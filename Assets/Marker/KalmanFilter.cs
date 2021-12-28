using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KalmanFilter
{


    float paramQ; //0.001
    float paramR; //0.0015
    public KalmanFilter(float q, float r)
    {
        paramQ = q;
        paramR = r;
    }



    public class Measurement<T>
    {
        public T x; //optimal value
        public T predictX;
        public T z;
        public float P;
        public float K;

        public void SetObservedValue(T val)
        {
            z = val;
        }
    }

    public void KalmanUpdate(Measurement<float> measurement)
    {
        MeasurementUpdate(measurement);
        measurement.x = measurement.predictX + (measurement.z - measurement.predictX) * measurement.K;
        measurement.predictX = measurement.x;
    }

    void MeasurementUpdate(Measurement<float> measurement)
    {
        measurement.K = (measurement.P + paramQ) / (measurement.P + paramQ + paramR);
        measurement.P = paramR * (measurement.P + paramQ) / (paramR + measurement.P + paramQ);
    }

    public void KalmanUpdate(Measurement<Vector3> measurement)
    {
        MeasurementUpdate(measurement);
        measurement.x = measurement.predictX + (measurement.z - measurement.predictX) * measurement.K;
        measurement.predictX = measurement.x;
    }

    void MeasurementUpdate(Measurement<Vector3> measurement)
    {
        measurement.K = (measurement.P + paramQ) / (measurement.P + paramQ + paramR);
        measurement.P = paramR * (measurement.P + paramQ) / (paramR + measurement.P + paramQ);
    }


    public void KalmanUpdate(Measurement<Vector2> measurement)
    {
        MeasurementUpdate(measurement);
        measurement.x = measurement.predictX + (measurement.z - measurement.predictX) * measurement.K;
        measurement.predictX = measurement.x;
    }

    void MeasurementUpdate(Measurement<Vector2> measurement)
    {
        measurement.K = (measurement.P + paramQ) / (measurement.P + paramQ + paramR);
        measurement.P = paramR * (measurement.P + paramQ) / (paramR + measurement.P + paramQ);
    }


}
