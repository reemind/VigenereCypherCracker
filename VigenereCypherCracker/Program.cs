using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vigenere_Cypher_Cracker
{

    class Program
    {
        static void Main(string[] args)
        {
            //https://planetcalc.com/2468/
            var maxLen = 10;
            var rot = ROT.ROT0;
            var lang = Languages.English;

            Console.Write("Key: ");
            var key = Console.ReadLine();
            Console.Write("Text: ");
            var input = Console.ReadLine();

            var vigenere = new Vigenere(lang);
            var encrypted = vigenere.Encrypt(input, key);
            var resultArr = vigenere.Crack(encrypted, maxLen);

            if (resultArr.Count() == 0)
                return;

            var result = resultArr.First().Item2;

            Console.WriteLine($"Decrypted: {result}\n");
            Console.WriteLine($"Code: {vigenere.GetKey(result, encrypted)}");
            

            //Console.WriteLine(delta.ToString());
            Console.ReadKey();
        }
    }
}