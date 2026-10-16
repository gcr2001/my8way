using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sintetizador chiptune simples. Todos os efeitos sonoros e a trilha sonora do
/// jogo sao gerados por codigo (ondas quadrada/triangular/serra/ruido), o que
/// mantem o build leve e evita assets de terceiros.
/// </summary>
public static class AudioSynth
{
    public const int SampleRate = 44100;

    public enum Wave
    {
        Sine,
        Square,
        Triangle,
        Saw,
        Noise
    }

    /// <summary>Uma nota dentro de uma sequencia.</summary>
    public struct Note
    {
        public float Start;
        public float Duration;
        public float Frequency;
        public float EndFrequency;
        public float Volume;
        public Wave Shape;

        public Note(float start, float duration, float frequency, float volume, Wave shape)
        {
            Start = start;
            Duration = duration;
            Frequency = frequency;
            EndFrequency = frequency;
            Volume = volume;
            Shape = shape;
        }

        public Note(float start, float duration, float frequency, float endFrequency, float volume, Wave shape)
        {
            Start = start;
            Duration = duration;
            Frequency = frequency;
            EndFrequency = endFrequency;
            Volume = volume;
            Shape = shape;
        }
    }

    private static uint noiseState = 0x9E3779B9u;

    /// <summary>Converte uma nota MIDI em frequencia (A4 = 69 = 440 Hz).</summary>
    public static float Midi(int midi)
    {
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }

    public static AudioClip FromSamples(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>Renderiza uma lista de notas em um clip de duracao fixa.</summary>
    public static AudioClip Sequence(string name, float length, IList<Note> notes)
    {
        int total = Mathf.Max(1, Mathf.CeilToInt(length * SampleRate));
        float[] buffer = new float[total];
        for (int i = 0; i < notes.Count; i++)
        {
            Render(buffer, notes[i]);
        }

        Normalize(buffer, 0.92f);
        return FromSamples(name, buffer);
    }

    /// <summary>Escreve uma nota (com envelope percussivo) dentro do buffer.</summary>
    public static void Render(float[] buffer, Note note)
    {
        int start = Mathf.Clamp(Mathf.RoundToInt(note.Start * SampleRate), 0, buffer.Length);
        int count = Mathf.RoundToInt(note.Duration * SampleRate);
        if (count <= 0)
        {
            return;
        }

        int attack = Mathf.Max(1, Mathf.RoundToInt(0.004f * SampleRate));
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            int index = start + i;
            if (index >= buffer.Length)
            {
                break;
            }

            float t = i / (float)count;
            float frequency = Mathf.Lerp(note.Frequency, note.EndFrequency, t);
            phase += frequency / SampleRate;
            if (phase >= 1f)
            {
                phase -= Mathf.Floor(phase);
            }

            float envelope = Mathf.Min(1f, i / (float)attack) * Mathf.Pow(1f - t, 1.6f);
            buffer[index] += Oscillator(note.Shape, phase) * envelope * note.Volume;
        }
    }

    public static float Oscillator(Wave shape, float phase)
    {
        switch (shape)
        {
            case Wave.Sine:
                return Mathf.Sin(phase * Mathf.PI * 2f);
            case Wave.Square:
                return phase < 0.5f ? 1f : -1f;
            case Wave.Triangle:
                return 1f - (4f * Mathf.Abs(Mathf.Repeat(phase + 0.25f, 1f) - 0.5f));
            case Wave.Saw:
                return (phase * 2f) - 1f;
            case Wave.Noise:
                noiseState ^= noiseState << 13;
                noiseState ^= noiseState >> 17;
                noiseState ^= noiseState << 5;
                return ((noiseState & 0xFFFFu) / 32768f) - 1f;
            default:
                return 0f;
        }
    }

    /// <summary>Evita clipping sem matar a dinamica do clip.</summary>
    public static void Normalize(float[] buffer, float peak)
    {
        float max = 0f;
        for (int i = 0; i < buffer.Length; i++)
        {
            float abs = Mathf.Abs(buffer[i]);
            if (abs > max)
            {
                max = abs;
            }
        }

        if (max <= 0.0001f || max <= peak)
        {
            return;
        }

        float scale = peak / max;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] *= scale;
        }
    }

    // ------------------------------------------------------------- efeitos ---

    public static AudioClip Coin()
    {
        return Sequence("sfx_coin", 0.3f, new List<Note>
        {
            new Note(0f, 0.06f, Midi(84), 0.5f, Wave.Square),
            new Note(0.055f, 0.2f, Midi(91), 0.5f, Wave.Square),
        });
    }

    public static AudioClip Hit()
    {
        return Sequence("sfx_hit", 0.42f, new List<Note>
        {
            new Note(0f, 0.18f, 420f, 90f, 0.75f, Wave.Square),
            new Note(0f, 0.3f, 1200f, 200f, 0.35f, Wave.Noise),
            new Note(0.05f, 0.25f, 110f, 60f, 0.5f, Wave.Triangle),
        });
    }

    public static AudioClip Dash()
    {
        return Sequence("sfx_dash", 0.24f, new List<Note>
        {
            new Note(0f, 0.2f, 240f, 900f, 0.32f, Wave.Saw),
            new Note(0f, 0.14f, 1600f, 400f, 0.22f, Wave.Noise),
        });
    }

    public static AudioClip WaveClear()
    {
        List<Note> notes = new List<Note>();
        int[] steps = { 72, 76, 79, 84, 88 };
        for (int i = 0; i < steps.Length; i++)
        {
            notes.Add(new Note(i * 0.085f, 0.22f, Midi(steps[i]), 0.42f, Wave.Square));
            notes.Add(new Note(i * 0.085f, 0.22f, Midi(steps[i] - 12), 0.22f, Wave.Triangle));
        }

        return Sequence("sfx_wave", 0.8f, notes);
    }

    public static AudioClip GameOver()
    {
        List<Note> notes = new List<Note>();
        int[] steps = { 69, 65, 62, 57 };
        for (int i = 0; i < steps.Length; i++)
        {
            notes.Add(new Note(i * 0.22f, 0.4f, Midi(steps[i]), 0.5f, Wave.Square));
            notes.Add(new Note(i * 0.22f, 0.4f, Midi(steps[i] - 12), 0.3f, Wave.Triangle));
        }

        return Sequence("sfx_gameover", 1.3f, notes);
    }

    public static AudioClip Victory()
    {
        List<Note> notes = new List<Note>();
        int[] steps = { 72, 76, 79, 84, 79, 84, 88 };
        float[] times = { 0f, 0.12f, 0.24f, 0.36f, 0.6f, 0.72f, 0.84f };
        for (int i = 0; i < steps.Length; i++)
        {
            notes.Add(new Note(times[i], 0.45f, Midi(steps[i]), 0.4f, Wave.Square));
            notes.Add(new Note(times[i], 0.45f, Midi(steps[i] - 24), 0.26f, Wave.Triangle));
        }

        return Sequence("sfx_victory", 1.8f, notes);
    }

    public static AudioClip Click()
    {
        return Sequence("sfx_click", 0.09f, new List<Note>
        {
            new Note(0f, 0.07f, Midi(88), 0.3f, Wave.Square),
        });
    }

    public static AudioClip Tick()
    {
        return Sequence("sfx_tick", 0.12f, new List<Note>
        {
            new Note(0f, 0.09f, Midi(81), 0.26f, Wave.Triangle),
        });
    }

    public static AudioClip Spawn()
    {
        return Sequence("sfx_spawn", 0.4f, new List<Note>
        {
            new Note(0f, 0.3f, 120f, 520f, 0.3f, Wave.Square),
        });
    }

    // -------------------------------------------------------------- trilha ---

    /// <summary>
    /// Trilha sonora em loop: 8 compassos sobre a progressao Am - F - C - G,
    /// com baixo triangular, arpejo em ondas quadradas e hi-hat de ruido.
    /// </summary>
    public static AudioClip Music()
    {
        const float bpm = 124f;
        const int bars = 8;
        float sixteenth = 60f / bpm / 4f;
        float length = bars * 16 * sixteenth;

        int total = Mathf.CeilToInt(length * SampleRate);
        float[] buffer = new float[total];

        // Acordes (notas MIDI da triade) e a fundamental do baixo.
        int[][] chords =
        {
            new[] { 57, 60, 64 }, // Am
            new[] { 53, 57, 60 }, // F
            new[] { 48, 52, 55 }, // C
            new[] { 55, 59, 62 }, // G
        };

        // Motivo pentatonico usado na segunda metade do loop.
        int[] lead = { 72, 76, 79, 76, 81, 79, 76, 72 };

        for (int bar = 0; bar < bars; bar++)
        {
            int[] chord = chords[bar % chords.Length];
            float barStart = bar * 16 * sixteenth;
            bool secondHalf = bar >= bars / 2;

            // Baixo: fundamental nos tempos, com oitava alternada.
            for (int beat = 0; beat < 4; beat++)
            {
                int note = chord[0] - 12 + (beat == 2 ? 12 : 0);
                Render(buffer, new Note(barStart + (beat * 4 * sixteenth), sixteenth * 3.2f,
                    Midi(note), 0.34f, Wave.Triangle));
            }

            // Arpejo em semicolcheias.
            for (int step = 0; step < 16; step++)
            {
                int degree = step % 4;
                int index = degree == 3 ? 1 : degree;
                int note = chord[index] + 12;
                if (step % 8 == 4)
                {
                    note += 12;
                }

                Render(buffer, new Note(barStart + (step * sixteenth), sixteenth * 1.6f,
                    Midi(note), 0.1f, Wave.Square));
            }

            // Hi-hat de ruido nas colcheias (acento nos contratempos).
            for (int step = 0; step < 16; step += 2)
            {
                float volume = (step % 4 == 2) ? 0.1f : 0.05f;
                Render(buffer, new Note(barStart + (step * sixteenth), sixteenth * 0.5f,
                    7000f, 3000f, volume, Wave.Noise));
            }

            // Melodia entra na segunda metade para o loop nao ficar monotono.
            if (secondHalf)
            {
                for (int step = 0; step < 8; step++)
                {
                    int note = lead[(step + (bar * 3)) % lead.Length];
                    Render(buffer, new Note(barStart + (step * 2 * sixteenth), sixteenth * 1.9f,
                        Midi(note), 0.14f, Wave.Square));
                }
            }
        }

        Normalize(buffer, 0.82f);
        return FromSamples("music_neon_circuit", buffer);
    }
}
