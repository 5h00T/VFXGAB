using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("sh00t/AudioWave")]
public class AudioBinder : VFXBinderBase
{
    [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
    ExposedProperty outputWaveMapProperty = "OutputWaveMap";

    [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
    ExposedProperty outputSpectrumMapProperty = "OutputSpectrumMap";

    [VFXPropertyBinding("System.UInt32"), SerializeField]
    ExposedProperty waveLengthProperty = "WaveLength";

    [VFXPropertyBinding("System.UInt32"), SerializeField]
    ExposedProperty spectrumLengthProperty = "SpectrumLength";

    [SerializeField]
    private AudioSource audioSource;
    private Color[] outputColors;
    private Color[] outputSpectrumColors;
    private Texture2D outputWaveMap;
    private Texture2D outputSpectrumMap;
    private float[] outputWave;
    private float[] outputSpectrum;
    public int waveLength = 512;
    public int spectrumLength = 512;
    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;

    public override bool IsValid(VisualEffect component)
    {
        return audioSource != null &&
            component.HasTexture(outputWaveMapProperty) &&
            component.HasTexture(outputSpectrumMapProperty) &&
            component.HasUInt(waveLengthProperty) &&
            component.HasUInt(spectrumLengthProperty);
    }

    private void GetWave()
    {
        if (outputWave == null || waveLength != outputWave.Length)
        {
            outputWave = new float[waveLength];
            outputColors = new Color[waveLength];

            outputWaveMap = new Texture2D(waveLength, 1, TextureFormat.RFloat, false);
            outputWaveMap.filterMode = FilterMode.Bilinear;
            outputWaveMap.wrapMode = TextureWrapMode.Clamp;
        }

        if (outputSpectrum == null || spectrumLength != outputSpectrum.Length)
        {
            outputSpectrum = new float[spectrumLength];
            outputSpectrumColors = new Color[spectrumLength];

            outputSpectrumMap = new Texture2D(spectrumLength, 1, TextureFormat.RFloat, false);
            outputSpectrumMap.filterMode = FilterMode.Bilinear;
            outputSpectrumMap.wrapMode = TextureWrapMode.Clamp;
        }

        audioSource.GetOutputData(outputWave, 1);

        for (var i = 0; i < waveLength; i++)
        {
            outputColors[i].r = outputWave[i];
        }
        outputWaveMap.SetPixels(outputColors);
        outputWaveMap.Apply();

        audioSource.GetSpectrumData(outputSpectrum, 1, fftWindow);
        for (var i = 0; i < spectrumLength; i++)
        {
            outputSpectrumColors[i].r = outputSpectrum[i];
        }
        outputSpectrumMap.SetPixels(outputSpectrumColors);
        outputSpectrumMap.Apply();
    }

    public override void UpdateBinding(VisualEffect component)
    {
        GetWave();

        component.SetTexture(outputWaveMapProperty, outputWaveMap);
        component.SetTexture(outputSpectrumMapProperty, outputSpectrumMap);
        component.SetUInt(waveLengthProperty, (uint)waveLength);
        component.SetUInt(spectrumLengthProperty, (uint)spectrumLength);
    }
}
