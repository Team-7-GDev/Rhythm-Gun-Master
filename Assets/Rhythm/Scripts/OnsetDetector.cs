using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

public class OnsetDetector : MonoBehaviour
{
    public int myIndex = 1;
    public AudioClip audioClip;
    private float[] spectrumData;
    private float[] previousSpectrumData;
    private float[] samples;
    public float thresholdMultiplier = 1.5f; // Multiplier for onset detection
    public int frameSize = 1024; // Frame size for FFT
    private List<float> spectralFlux = new List<float>();
    private List<float> onsetThresholds = new List<float>();
    private List<bool> onsets = new List<bool>();
    private Complex[] complexSamples;

    public GameObject bar;
    public float yMultiplier = 100.0f;
    private Transform[] bars;


    public static OnsetDetector Instance { get; private set; }

    public float Bass { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (audioClip == null)
        {
            Debug.LogError("Please assign an audio clip.");
            return;
        }

        spectrumData = new float[frameSize / 2];
        previousSpectrumData = new float[frameSize];
        samples = new float[frameSize];
        complexSamples = new Complex[frameSize];

        if (!barParent)
        {
            barParent = new GameObject("Bar Parent").transform;
            barParent.SetLocalPositionAndRotation(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
        }

        bars = new Transform[frameSize / 2];
        for (int i = 0; i < bars.Length; i++)
        {
            Transform t = Instantiate(bar, new UnityEngine.Vector3(2.0f * i, 0.0f, 0.0f), UnityEngine.Quaternion.identity).transform;
            t.SetParent(barParent);
            bars[i] = t;
        }

        startTime = Time.time;

        //StartCoroutine(SpawnBar());

        // Process the audio clip
        //ProcessAudioClip();

    }

    private float startTime;

    public int offsetSamples;
    [ReadOnly] public int currentSampleOffset;
    [ReadOnly] public int sampleOffsetDiff;
    [ReadOnly] public float elapsedTime;
    [ReadOnly] public float percent;

    private Transform barParent;

    private void Update()
    {
        int prevSampleOffset = currentSampleOffset;
        int totalSamples = audioClip.samples;

        elapsedTime = Time.time - startTime;
        percent = Mathf.Round((elapsedTime * 100.0f / audioClip.length) * 100.0f) * 0.01f;
        //percent = Mathf.Round((currentSampleOffset * 100.0f / totalSamples) * 100.0f) * 0.01f;

        currentSampleOffset = Mathf.FloorToInt(elapsedTime * audioClip.frequency);
        //currentSampleOffset += 100;

        sampleOffsetDiff = currentSampleOffset - prevSampleOffset;

        if (currentSampleOffset >= totalSamples)
        {
            print("Reached End of clip");
            gameObject.SetActive(false);
            return;
        }

        int offsetSamples = currentSampleOffset % totalSamples;

        audioClip.GetData(samples, offsetSamples);

        //for (int i = 0; i < bars.Length; i++)
        //{
        //    if (float.IsNaN(samples[i]))
        //        samples[i] = 0.0f;
        //    UpdateY(bars[i], samples[i] * yMultiplier);
        //}

        // Perform FFT to convert sample data to spectrum data
        PerformFFT(samples, spectrumData);

        // Use the spectrum data for visualization or analysis
        for (int i = 0; i < spectrumData.Length; i++)
        {
            if (float.IsNaN(spectrumData[i]))
                spectrumData[i] = 0.0f;
            UpdateY(bars[i], spectrumData[i] * yMultiplier);
        }

        Bass = GetAverageData(12, 47);
    }

    private float GetAverageData(int start, int end)
    {
        float sum = 0;
        int count = 0;

        for (int i = start; i <= end; i++)
        {
            sum += spectrumData[i];
            count++;
        }

        return (count == 0) ? 0.0f : sum / count;
    }

    private void UpdateY(Transform t, float y)
    {
        UnityEngine.Vector3 localScale = t.localScale;
        localScale.y = 1.0f + y;
        t.localScale = localScale;
    }

    void ProcessAudioClip()
    {
        int totalSamples = audioClip.samples;
        int totalFrames = totalSamples / frameSize;

        print($"Total Frames : {totalFrames}");

        for (int i = 0; i < totalFrames; i++)
        {
            audioClip.GetData(samples, i * frameSize);

            //if (i == myIndex)
            //    foreach (var sample in samples)
            //        print(sample);

            // Perform FFT on the samples to get spectrum data
            PerformFFT(samples, spectrumData);

            if (i == myIndex)
                foreach (var sample in spectrumData)
                    print(sample);

            if (i > 0)
            {
                // Compute spectral flux
                float flux = ComputeSpectralFlux(spectrumData, previousSpectrumData);
                spectralFlux.Add(flux);
            }
            else
            {
                spectralFlux.Add(0f);
            }

            // Save current spectrum data for the next frame
            spectrumData.CopyTo(previousSpectrumData, 0);
        }

        // Compute onset thresholds
        ComputeOnsetThresholds();

        // Detect onsets
        DetectOnsets();

        // Example: Log detected onsets
        for (int i = 0; i < onsets.Count; i++)
        {
            if (onsets[i])
            {
                //Debug.Log("Onset detected at frame: " + i);
            }
        }
    }

    float ComputeSpectralFlux(float[] currentSpectrum, float[] previousSpectrum)
    {
        float flux = 0f;
        for (int i = 0; i < currentSpectrum.Length; i++)
        {
            float value = currentSpectrum[i] - previousSpectrum[i];
            flux += value > 0 ? value : 0;
        }
        return flux;
    }

    void ComputeOnsetThresholds()
    {
        int windowSize = 10;
        for (int i = 0; i < spectralFlux.Count; i++)
        {
            int start = Mathf.Max(0, i - windowSize / 2);
            int end = Mathf.Min(spectralFlux.Count - 1, i + windowSize / 2);

            float mean = 0f;
            for (int j = start; j <= end; j++)
            {
                mean += spectralFlux[j];
            }
            mean /= (end - start + 1);

            onsetThresholds.Add(mean * thresholdMultiplier);
        }
    }

    void DetectOnsets()
    {
        for (int i = 0; i < spectralFlux.Count; i++)
        {
            if (i < onsetThresholds.Count && spectralFlux[i] > onsetThresholds[i])
            {
                onsets.Add(true);
            }
            else
            {
                onsets.Add(false);
            }
        }
    }

    void PerformFFT(float[] data, float[] output)
    {
        // Copy data to complex array and apply window function
        for (int i = 0; i < data.Length; i++)
        {
            complexSamples[i] = new Complex(data[i] * WindowFunction(i, data.Length, FFTWindow.Hanning), 0);
        }

        // Perform the FFT using Math.NET Numerics
        Fourier.Forward(complexSamples, FourierOptions.Matlab);

        float max = float.MinValue;

        // Copy the magnitude of the complex numbers to the output array
        for (int i = 0; i < output.Length; i++)
        {
            output[i] = (float)complexSamples[i].Magnitude;
            if (output[i] > max)
                max = output[i];
        }

        //for (int i = 0; i < output.Length; i++)
        //{
        //    output[i] /= max;
        //}
    }

    float WindowFunction(int index, int total, FFTWindow window)
    {
        switch (window)
        {
            case FFTWindow.Blackman:
                return 0.42f - 0.5f * Mathf.Cos(2.0f * Mathf.PI * index * total) + 0.08f * Mathf.Cos(4.0f * Mathf.PI * index * total);
            case FFTWindow.BlackmanHarris:
                return 0.35875f - 0.48829f * Mathf.Cos(2.0f * Mathf.PI * index / (total - 1)) + 0.14128f * Mathf.Cos(4.0f * Mathf.PI * index / (total - 1)) - 0.01168f * Mathf.Cos(6.0f * Mathf.PI * index / (total - 1));
            case FFTWindow.Hamming:
                return 0.54f - 0.46f * Mathf.Cos(2.0f * Mathf.PI * index / (total - 1));
            case FFTWindow.Hanning:
                return 0.5f * (1.0f - Mathf.Cos(2.0f * Mathf.PI * index / (total - 1)));
            default:
                return 1.0f;
        }
    }

    void FFT(float[] data, float[] output)
    {
        int N = data.Length;
        for (int k = 0; k < N; k++)
        {
            float real = 0f;
            float imag = 0f;
            for (int n = 0; n < N; n++)
            {
                float angle = 2f * Mathf.PI * k * n / N;
                real += data[n] * Mathf.Cos(angle);
                imag -= data[n] * Mathf.Sin(angle);
            }
            output[k] = Mathf.Sqrt(real * real + imag * imag);
        }
    }

    void Swap(ref float a, ref float b)
    {
        float temp = a;
        a = b;
        b = temp;
    }
}
