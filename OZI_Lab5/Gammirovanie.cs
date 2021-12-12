using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using ConsoleMenu;

namespace OZI_Lab4
{
    class Gamification 
    {
        private static ImmutableArray<char> EncodingAlphabet { get; }

        static Gamification()
        {
            var alphabet = new List<char>();
            alphabet.Add('!');


            for (int i = 0; i < 32; i++)
                alphabet.Add((char)('а' + i));

            alphabet.Add(' ');
            alphabet.Insert(alphabet.IndexOf('е') + 1, 'ё');

            for (int i = 0; i < 10; i++)
                alphabet.Add((char)('0' + i));

            for (int i = 0; i < 26; i++)
                alphabet.Add((char)('a' + i));

            EncodingAlphabet = alphabet.ToImmutableArray();
        }

        //логическая неэквивалентность.
        uint CalculateInequality(uint x, uint y) => ((~(x | y) | (x & y)) << 26) >> 26;

        //логическая эквивалентность.
        uint CalculateEquivalence(uint x, uint y) => (x | y) & ~(x & y);

        public string Encrypt(string key, string rawLine)
        {
            StringBuilder encodedMessage = new StringBuilder();

            for (int i = 0, keyInd = 0; i < rawLine.Length; i++, keyInd++)
            {
                if (keyInd == key.Length)
                    keyInd = 0;
                ////////////////////////////////

                var msgCharacter = rawLine[i];
                var gammaCharacter = key[keyInd];

                var msgCharacterPosition = EncodingAlphabet.IndexOf(msgCharacter);
                var gammaCharacterPosition = EncodingAlphabet.IndexOf(gammaCharacter);

                var encodedCharacterPosition = CalculateInequality((uint) msgCharacterPosition, (uint) gammaCharacterPosition);
                var encodedCharacter = EncodingAlphabet.ItemRef((int) encodedCharacterPosition);

                encodedMessage.Append(encodedCharacter);
            }

            return encodedMessage.ToString();
        }
    }
}