using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vigenere_Cypher_Cracker
{
    class Vigenere
    {
        public Language Language { get; set; }
        Caesar caesar { get; set; }
        ROT Rot { get; set; }

        public Vigenere(Language language, ROT rot = ROT.ROT0)
        {
            Language = language;
            caesar = new Caesar(language);
            Rot = rot;
        }

        static IEnumerable<FreqPair> GetFrequencyTable(string str)
            => str.GroupBy(t => t)
                .Select(t => new FreqPair
                {
                    Char = t.Key,
                    Freq = t.Count() / (double)str.Length
                });

        double IndexOfCoincidence(IEnumerable<FreqPair> table)
        {
            double acc = 0;
            foreach (var pair in table)
                if (Language.LettersFrequency.ContainsKey(pair.Char))
                    acc += pair.Freq * Language.LettersFrequency[pair.Char];

            return acc;
        }

        public static List<string> Divide(string str, int numGroups)
        {
            var result = new List<StringBuilder>();
            for (int i = 0; i < numGroups; i++)
                result.Add(new StringBuilder());

            for (int i = 0; i < str.Length; i++)
            {
                result[i % numGroups].Append(str[i]);
            }

            return result.Select(t => t.ToString()).ToList();
        }

        (int, double) BestCaeserShift(string text)
        {
            int bestShiftAmount = 0;
            double bestDifference = double.MaxValue;

            for (int shiftAmount = 0; shiftAmount < Language.AlphLength; ++shiftAmount)
            {
                var plaintext = caesar.Decrypt(text, shiftAmount);
                var index = IndexOfCoincidence(GetFrequencyTable(plaintext));
                var difference = Math.Abs(index - Language.MainFreq);

                if (difference < bestDifference)
                {
                    bestDifference = difference;
                    bestShiftAmount = shiftAmount;
                }
            }

            return (bestShiftAmount, bestDifference);
        }

        public IOrderedEnumerable<(double, string)> Crack(string text, int maxKeylength)
        {
            text = caesar.Strip(text);
            var overall = new List<(double, string)>();

            for (int keyLen = 1; keyLen < maxKeylength; keyLen++)
            {
                var groups = Divide(text, keyLen);

                var totalDiff = 0d;
                List<string> plainTexts = new List<string>();

                foreach (var group in groups)
                {
                    var (shiftAmount, difference) = BestCaeserShift(group);

                    totalDiff += difference;

                    plainTexts.Add(caesar.Decrypt(group, shiftAmount));
                }

                var plainText = new StringBuilder();

                for (int i = 0; i < text.Length; i++)
                {
                    plainText.Append(plainTexts[i % keyLen][i / keyLen]);
                }

                overall.Add((totalDiff, plainText.ToString()));
            }

            return overall.OrderBy(t => t.Item1);
        }

        public string Encrypt(string text, string key)
        {
            text = caesar.Strip(text);
            StringBuilder output = new StringBuilder();
            var len = key.Length;
            for (int i = 0; i < text.Length; i++)
            {
                int index = ((text[i] + key[i % len] - 2 * Language.StartLetter) % (Language.AlphLength)) + Language.StartLetter;
                output.Append((char)index);

            }

            return output.ToString();
        }

        public string GetKey(string origin, string encrypted)
        {
            var builder = new StringBuilder();

            origin = caesar.Strip(origin);
            encrypted = caesar.Strip(encrypted);

            if (origin.Length != encrypted.Length)
                return "";

            for (int i = 0; i < origin.Length; i++)
            {
                var value = origin[i] - encrypted[i] - Rot;

                if (value < 0)
                    value += Language.AlphLength;

                builder.Append((char)(Language.StartLetter + value));
            }

            var delta = builder.ToString();

            IEnumerable<string> Divide(string str, int len)
            {
                for (int i = 0; i < str.Length; i += len)
                {
                    yield return str.Substring(i, Math.Min(len, str.Length - i));
                }
            }

            for (int i = 1; i < delta.Length; i++)
            {
                var groups = Divide(delta, i);

                if (groups.Where(t => t.Count() == i).All(t => t == groups.First()))
                    return groups.First();
            }
            return "";

        }
    }
    enum ROT
    {
        ROT0 = 0,
        ROT1 = 1
    }

    class Caesar
    {
        public Language Language { get; set; }

        public Caesar(Language language)
        {
            Language = language;
        }

        public string Strip(string str)
        {
            str = str.ToUpper();

            var reg = new Regex($"[{Language.LettersFrequency.First().Key}-{Language.LettersFrequency.Last().Key}]");

            StringBuilder builder = new StringBuilder();
            foreach (Match match in reg.Matches(str.ToUpper()))
                builder.Append(match.Value[0]);
                
            return builder.ToString();
        }

        string Substitute(string text, string sequence)
        {
            text = Strip(text).ToUpper();
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
                builder.Append(sequence[text[i] - Language.StartLetter]);

            return builder.ToString();
        }

        public string Encrypt(string text, int shift)
        {
            var sequence = new StringBuilder();
            for (int i = 0; i < Language.AlphLength; i++)
                sequence.Append((char)(((shift + i) % Language.AlphLength) + Language.StartLetter));

            return Substitute(text, sequence.ToString());
        }

        public string Decrypt(string text, int shift)
        {
            var sequence = new char[Language.AlphLength];
            for (int i = 0; i < Language.AlphLength; i++)
                sequence[(shift + i) % Language.AlphLength] = (char)(Language.StartLetter + i);

            return Substitute(text, string.Join("", sequence));
        }
    }

    class FreqPair
    {
        public char Char { get; set; }
        public double Freq { get; set; }
    }

    static class Languages
    {
        public static Language English = new Language
        {
            AlphLength = 26,
            StartLetter = 'A',
            MainFreq = 0.065,
            LettersFrequency = new Dictionary<char, double>{
            { 'A', 0.082 },
            { 'B', 0.015 },
            { 'C', 0.028 },
            { 'D', 0.043 },
            { 'E', 0.127 },
            { 'F', 0.022 },
            { 'G', 0.020 },
            { 'H', 0.061 },
            { 'I', 0.070 },
            { 'J', 0.002 },
            { 'K', 0.008 },
            { 'L', 0.040 },
            { 'M', 0.024 },
            { 'N', 0.067 },
            { 'O', 0.075 },
            { 'P', 0.019 },
            { 'Q', 0.001 },
            { 'R', 0.060 },
            { 'S', 0.063 },
            { 'T', 0.091 },
            { 'U', 0.028 },
            { 'V', 0.010 },
            { 'W', 0.023 },
            { 'X', 0.001 },
            { 'Y', 0.020 },
            { 'Z', 0.001 }
        }
        };
        public static Language Russian = new Language
        {
            AlphLength = 32,
            StartLetter = 'А',
            MainFreq = 0.0553,
            LettersFrequency = new Dictionary<char, double> {
                { 'А', 0.07998 },
                { 'Б', 0.01592 },
                { 'В', 0.04533 },
                { 'Г', 0.01687 },
                { 'Д', 0.02977 },
                { 'Е', 0.08483 },
                { 'Ж', 0.00940 },
                { 'З', 0.01641 },
                { 'И', 0.07367 },
                { 'Й', 0.01208 },
                { 'К', 0.03486 },
                { 'Л', 0.04343 },
                { 'М', 0.03203 },
                { 'Н', 0.06700 },
                { 'О', 0.10983 },
                { 'П', 0.02804 },
                { 'Р', 0.04746 },
                { 'С', 0.05473 },
                { 'Т', 0.06318 },
                { 'У', 0.02615 },
                { 'Ф', 0.00267 },
                { 'Х', 0.00966 },
                { 'Ц', 0.00486 },
                { 'Ч', 0.01450 },
                { 'Ш', 0.00718 },
                { 'Щ', 0.00361 },
                { 'Ъ', 0.00037 },
                { 'Ы', 0.01898 },
                { 'Ь', 0.01735 },
                { 'Э', 0.00331 },
                { 'Ю', 0.00639 },
                { 'Я', 0.02001 }
            }
        };
    }

    class Language
    {
        public char StartLetter { get; set; }
        public double MainFreq { get; set; }
        public int AlphLength { get; set; }
        public Dictionary<char, double> LettersFrequency { get; set; }
    }
}
