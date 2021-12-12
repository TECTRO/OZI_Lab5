using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using ConsoleMenu;
using OZI_Lab4;

namespace OZI_Lab5
{
    public class ElectronicSignature : IForceExecute
    {
        public class OpenKey
        {
            public int E { get; internal set; }
            public int N { get; internal set; }
            public override string ToString() => $"(E = {E}, N = {N})";
        }
        public class ClosedKey
        {
            public int D { get; internal set; }
            public int N { get; internal set; }
            public override string ToString() => $"(D = {D}, N = {N})";

        }
        public class Keys
        {
            public OpenKey OpenKey { get; internal set; }
            public ClosedKey ClosedKey { get; internal set; }
            public override string ToString() => $"Открытый ключ: {OpenKey}, закрытый ключ: {ClosedKey}";
        }

        public string Title { get; } = "Генерация электронной подписи";

        public int GetSimpleDigit(int index)
        {
            var internalIndex = 0;

            var digit = 2;
            var simpleDigit = digit;
            do
            {
                bool isSimple = true;
                for (int i = 2; i < digit; i++)
                    if (digit % i == 0 && digit != i)
                    {
                        isSimple = false;
                        break;
                    }

                if (isSimple)
                {
                    internalIndex++;
                    simpleDigit = digit;
                }

                digit++;
            }
            while (internalIndex <= index);

            return simpleDigit;
        }

        public int GetSimpleIndex(int simpleDigit)
        {
            int index = 0;
            while (GetSimpleDigit(index) < simpleDigit)
                index++;

            return index;
        }

        public Keys GenerateRsaKeys(int maxDigitIndex = 10)
        {
            int Gcd(int a, int b)
            {
                while (a != 0 && b != 0)
                {
                    if (a > b)
                        a %= b;
                    else
                        b %= a;
                }

                return a | b;
            }

            var rand = new Random();
            int p;
            int q;
            do
            {
                p = GetSimpleDigit(rand.Next(maxDigitIndex));
                q = GetSimpleDigit(rand.Next(maxDigitIndex));
            }
            while (p == q);

            var n = p * q;

            var phi = (p - 1) * (q - 1);

            #region calculating d

            int d;
            do
            {
                d = GetSimpleDigit(rand.Next(maxDigitIndex));
            }
            while (Gcd(d, phi) != 1);

            #endregion

            #region calculating e

            int index = 0;
            int e;
            do
            {
                e = GetSimpleDigit(index);
                index++;

            } while (e * d % phi != 1);

            #endregion


            return new Keys
            {
                OpenKey = new OpenKey { E = e, N = n },
                ClosedKey = new ClosedKey { D = d, N = n }
            };
        }

        public string GenerateDigest(string message)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < message.Length; i++)
                if (i % 2 == 0)
                    result.Append(message[i]);

            return result.ToString();
        }

        public BigInteger GenerateHash(string message, OpenKey key, int maxH0 = 100)
        {
            BigInteger h = message.First();

            foreach (var character in message)
                h = BigInteger.Pow(h + character, 2) % key.N;

            return h;
        }

        public BigInteger GenerateElectronicSignature(BigInteger hash, ClosedKey key) =>
            BigInteger.Pow(hash, key.D) % key.N;
        public BigInteger CheckElectronicSignature(BigInteger electronicSignature, OpenKey key) =>
            BigInteger.Pow(electronicSignature, key.E) % key.N;

        public ExitEvent Execute()
        {
            var encryption = new Gamification();
            Console.WriteLine("\nВведите исходное сообщение");
            var message = Console.ReadLine().PrintLn("Исходное сообщение");
            Console.WriteLine("Введите ключ шифрования");
            var messageKey = Console.ReadLine().PrintLn("ключ шифрования исходного сообщения");
            var encryptedMessage = encryption.Encrypt(messageKey, message).PrintLn("зашифрованное сообщение");

            var keys = GenerateRsaKeys().PrintLn("Сгенерированные ключи");
            var messageDigest = GenerateDigest(encryptedMessage).PrintLn("Сгенерированный дайджест");
            var messageHash = GenerateHash(messageDigest, keys.OpenKey)
                .PrintLn("Сгенерированный хеш дайджеста");
            var electronicSignature = GenerateElectronicSignature(messageHash, keys.ClosedKey)
                .PrintLn("Сгенерированное значение электронной подписи");

            Send(encryptedMessage, keys.OpenKey, electronicSignature);

            var localHash = GenerateHash(GenerateDigest(encryptedMessage), keys.OpenKey)
                .PrintLn("Восстановленный хеш дайджеста сообщения");
            var electronicSignatureHash = CheckElectronicSignature(electronicSignature, keys.OpenKey)
                .PrintLn("Хеш, восстановленный из электронной подписи");

            Console.WriteLine(localHash == electronicSignatureHash
                ? "Хэши совпадают, сообщение подлинно"
                : "Хэши не совпадают, сообщение подделано");

            if (localHash == electronicSignatureHash)
                encryption.Encrypt(messageKey, encryptedMessage)
                    .PrintLn($"Сообщение, расшифрованное с помощью ключа {messageKey}");

            return ExitEvent.Wait;
        }

        void Send(string message, OpenKey key, BigInteger signature)
        {
            Console.WriteLine();
            message.Print("Отправка сообщения");
            signature.Print(", ЭП");
            key.Print(" и открытого колюча");

            var rand = new Random();
            Console.Write(' ');
            for (int i = 0; i < rand.Next(6, 15); i++)
            {
                Console.Write('.');
                Thread.Sleep(rand.Next(100, 600));
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }

    public static class Ext
    {
        public static T PrintLn<T>(this T value, string desc = "")
        {
            value.Print(desc);
            Console.WriteLine();
            return value;
        }

        public static T Print<T>(this T value, string desc = "")
        {
            if (!string.IsNullOrWhiteSpace(desc))
                Console.Write($"{desc}: ");
            Console.Write(value);
            return value;
        }
    }
}