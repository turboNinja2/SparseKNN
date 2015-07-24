﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DataScienceECom.Phis
{
    public delegate Tuple<int, List<string>> Phi(string line, string header);

    public delegate int PriceTransform(double price);

    public delegate string StringTransform(string inputString);

    public static class Phis
    {
        public static List<string> CartesianProduct(this List<string> input)
        {
            List<string> res = new List<string>();

            for (int i = 0; i < input.Count; i++)
                for (int j = i; j < input.Count; j++)
                    res.Add(input[i] + "_" + input[j]);
            return res;
        }

        private static Tuple<int, List<string>> phiRS_PT(string line, string header,
            StringTransform sf,
            int minLetters, int maxLetters,
            PriceTransform pt)
        {
            string[] predictors = (sf(line)).Split(';');
            string[] headerElements = header.Split(';');

            List<string> hashedPredictors = new List<string>();
            int answer = 0;
            string priceHash = "";
            string brandHash = "";

            for (int i = 0; i < headerElements.Length; i++)
            {
                string currentHeaderElt = headerElements[i];

                if (currentHeaderElt == "Categorie3")
                {
                    answer = Convert.ToInt32(predictors[i]);
                    continue;
                }

                if (currentHeaderElt.StartsWith("Cat") || currentHeaderElt.StartsWith("Ident")) continue;

                if (currentHeaderElt == "prix")
                {
                    priceHash = "px_" + pt(Convert.ToDouble(predictors[i], CultureInfo.GetCultureInfo("en-US")));
                    continue;
                }

                if (currentHeaderElt == "Marque")
                {
                    string brandName = predictors[i];
                    if (String.Equals(brandName, "aucune")) brandName = "";
                    brandHash = "Mq_" + brandName;
                    continue;
                }

                string[] splittedElt = predictors[i].Split(' ');
                foreach (string str in splittedElt)
                    if (str.Length > minLetters && str.Length < maxLetters)
                        hashedPredictors.Add(currentHeaderElt.Substring(0, 1) + "_" + str.ToLower());
            }

            hashedPredictors.Add(priceHash);
            hashedPredictors.Add(brandHash);
            hashedPredictors.Add(brandHash + "_" + priceHash);

            return new Tuple<int, List<string>>(answer, hashedPredictors);
        }

        private static Tuple<int, List<string>> phiRS_PT2(string line, string header,
            StringTransform sf,
            int minLetters, int maxLetters, int maxLibelleLength,
            PriceTransform pt)
        {
            line = line.ToLower();
            string[] predictors = (sf(line)).Split(';');
            string[] headerElements = header.Split(';');

            List<string> hashedPredictors = new List<string>();
            int answer = 0;
            string priceHash = "";
            string brandHash = "";

            for (int i = 0; i < headerElements.Length; i++)
            {
                string currentHeaderElt = headerElements[i];

                if (currentHeaderElt == "Categorie3")
                {
                    answer = Convert.ToInt32(predictors[i]);
                    continue;
                }

                if (currentHeaderElt.StartsWith("Cat") || currentHeaderElt.StartsWith("Ident")) continue;

                if (currentHeaderElt == "prix")
                {
                    priceHash = "px_" + pt(Convert.ToDouble(predictors[i], CultureInfo.GetCultureInfo("en-US")));
                    continue;
                }

                if (currentHeaderElt == "Marque")
                {
                    string brandName = predictors[i];
                    if (String.Equals(brandName, "aucune")) brandName = "";
                    brandHash = "Mq_" + brandName;
                    continue;
                }

                if (predictors[i].StartsWith("de ") && currentHeaderElt.StartsWith("Des"))
                    hashedPredictors.Add("L_1de");

                if (predictors[i].Contains("coffret") && predictors[i].Contains("cd"))
                    hashedPredictors.Add("L_coffretCD");

                string[] splittedElt = predictors[i].Split(' ');
                foreach (string str in splittedElt)
                    if (str.Length > minLetters && str.Length < maxLetters)
                        hashedPredictors.Add(currentHeaderElt.Substring(0, 1) + "_" + str);

                if (currentHeaderElt.StartsWith("Lib") && predictors[i].Length < maxLibelleLength)
                    hashedPredictors.Add(predictors[i]);
            }

            if (priceHash != "px_-1") hashedPredictors.Add(priceHash);

            if (brandHash != "Mq_")
            {
                hashedPredictors.Add(brandHash);
                if (priceHash != "px_-1") hashedPredictors.Add(brandHash + "_" + priceHash);
            }

            return new Tuple<int, List<string>>(answer, hashedPredictors);
        }

        private static Tuple<int, List<string>> phiRS_PT_spe(string line, string header,
    StringTransform sf,
    int minLetters, int maxLetters, int maxLibelleLength,
    PriceTransform pt)
        {
            line = line.ToLower();
            string[] predictors = (sf(line)).Split(';');
            string[] headerElements = header.Split(';');

            List<string> hashedPredictors = new List<string>();
            int answer = 0;
            string priceHash = "";
            string brandHash = "";

            for (int i = 0; i < headerElements.Length; i++)
            {
                string currentHeaderElt = headerElements[i];

                if (currentHeaderElt == "Categorie2")
                {
                    answer = Convert.ToInt32(predictors[i]);
                    continue;
                }

                if (currentHeaderElt.StartsWith("Cat") || currentHeaderElt.StartsWith("Ident")) continue;

                if (currentHeaderElt == "prix")
                {
                    priceHash = "px_" + pt(Convert.ToDouble(predictors[i], CultureInfo.GetCultureInfo("en-US")));
                    continue;
                }

                if (currentHeaderElt == "Marque")
                {
                    string brandName = predictors[i];
                    if (String.Equals(brandName, "aucune")) brandName = "";
                    brandHash = "Mq_" + brandName;
                    continue;
                }

                if (predictors[i].StartsWith("de ") && currentHeaderElt.StartsWith("Des"))
                    hashedPredictors.Add("L_1de");

                if (predictors[i].Contains("coffret") && predictors[i].Contains("cd"))
                    hashedPredictors.Add("L_coffretCD");

                string[] splittedElt = predictors[i].Split(' ');
                foreach (string str in splittedElt)
                    if (str.Length > minLetters && str.Length < maxLetters)
                        hashedPredictors.Add(currentHeaderElt.Substring(0, 1) + "_" + str);

                if (currentHeaderElt.StartsWith("Lib") && predictors[i].Length < maxLibelleLength)
                    hashedPredictors.Add(predictors[i]);
            }

            if (priceHash != "px_-1") hashedPredictors.Add(priceHash);

            if (brandHash != "Mq_")
            {
                hashedPredictors.Add(brandHash);
                if (priceHash != "px_-1") hashedPredictors.Add(brandHash + "_" + priceHash);
            }

            return new Tuple<int, List<string>>(answer, hashedPredictors);
        }

        private static Tuple<int, List<string>> phiSmart(string line, string header,
            StringTransform sf,
            int minLetters, int maxLetters, int minLetters2,
            PriceTransform pt)
        {
            line = line.ToLower();
            string[] predictors = (sf(line)).Split(';');
            string[] headerElements = header.Split(';');

            List<string> hashedPredictors = new List<string>();
            int answer = 0;
            string priceHash = "";
            string brandHash = "";

            for (int i = 0; i < headerElements.Length; i++)
            {
                string currentHeaderElt = headerElements[i];

                if (currentHeaderElt == "Categorie3")
                {
                    answer = Convert.ToInt32(predictors[i]);
                    continue;
                }

                if (currentHeaderElt.StartsWith("Cat") || currentHeaderElt.StartsWith("Ident")) continue;

                if (currentHeaderElt == "prix")
                {
                    priceHash = "px_" + pt(Convert.ToDouble(predictors[i], CultureInfo.GetCultureInfo("en-US")));
                    continue;
                }

                if (currentHeaderElt == "Marque")
                {
                    string brandName = predictors[i];
                    if (String.Equals(brandName, "aucune")) brandName = "";
                    brandHash = "Mq_" + brandName;
                    continue;
                }


                string[] splittedElt = predictors[i].Split(' ');

                string previous = "";
                foreach (string str in splittedElt)
                {
                    if (str.Length > minLetters && str.Length < maxLetters)
                    {
                        hashedPredictors.Add(str);
                        if (previous.Length > 0)
                            hashedPredictors.Add(str + "_" + previous);
                        previous = str;
                        continue;
                    }
                    if (str.Length > minLetters2)
                        hashedPredictors.Add(str);
                }
            }

            if (priceHash != "px_-1") hashedPredictors.Add(priceHash);

            if (brandHash != "Mq_")
            {
                hashedPredictors.Add(brandHash);
                if (priceHash != "px_-1") hashedPredictors.Add(brandHash + "_" + priceHash);
            }

            return new Tuple<int, List<string>>(answer, hashedPredictors);
        }

        private static Tuple<int, List<string>> phiSmart2(string line, string header,
    StringTransform sf,
    int minLetters, int maxLetters, int minLetters2,
    PriceTransform pt)
        {
            line = line.ToLower();
            string[] predictors = (sf(line)).Split(';');
            string[] headerElements = header.Split(';');

            List<string> hashedPredictors = new List<string>();
            int answer = 0;
            string priceHash = "";
            string brandHash = "";

            for (int i = 0; i < headerElements.Length; i++)
            {
                string currentHeaderElt = headerElements[i];

                if (currentHeaderElt == "Categorie3")
                {
                    answer = Convert.ToInt32(predictors[i]);
                    continue;
                }

                if (currentHeaderElt.StartsWith("Cat") || currentHeaderElt.StartsWith("Ident")) continue;

                if (currentHeaderElt == "prix")
                {
                    priceHash = "px_" + pt(Convert.ToDouble(predictors[i], CultureInfo.GetCultureInfo("en-US")));
                    continue;
                }

                if (currentHeaderElt == "Marque")
                {
                    string brandName = predictors[i];
                    if (String.Equals(brandName, "aucune")) brandName = "";
                    brandHash = "Mq_" + brandName;
                    continue;
                }


                string[] splittedElt = predictors[i].Split(' ');

                string previous = "";
                foreach (string str in splittedElt)
                {
                    if (str.Length > minLetters && str.Length < maxLetters)
                    {
                        string strToAdd = str;
                        if (str.EndsWith("s"))
                            strToAdd = str.Substring(0, str.Length - 1);

                        hashedPredictors.Add(strToAdd);
                        if (previous.Length > 0)
                            hashedPredictors.Add(strToAdd + "_" + previous);
                        previous = strToAdd;
                        continue;
                    }

                    if (str.Length > minLetters2)
                        hashedPredictors.Add(str);
                }
            }

            if (priceHash != "px_-1") hashedPredictors.Add(priceHash);

            if (brandHash != "Mq_")
            {
                hashedPredictors.Add(brandHash);
                if (priceHash != "px_-1") hashedPredictors.Add(brandHash + "_" + priceHash);
            }

            return new Tuple<int, List<string>>(answer, hashedPredictors);
        }

        public static Tuple<int, List<string>> phi10(string line, string header)
        {
            return phiSmart(line, header, StringCleaner.RemoveMorePunctuationAndAccents,
                3, 15, 35, PriceTransforms.LogPrice);
        }

        public static Tuple<int, List<string>> phi11(string line, string header)
        {
            return phiSmart(line, header, StringCleaner.RemoveMorePunctuationAndAccents2,
                3, 20, 2, PriceTransforms.LogPrice);
        }

        public static Tuple<int, List<string>> phi12(string line, string header)
        {
            string newLine = Regex.Replace(line, "<br*>", "");

            return phiSmart(newLine, header, StringCleaner.RemoveMorePunctuationAndAccents2,
                3, 20, 2, PriceTransforms.LogPrice);
        }

        public static Tuple<int, List<string>> phi13(string line, string header)
        {
            string newLine = Regex.Replace(line, "<br*>", "");

            return phiSmart(newLine, header, StringCleaner.RemoveMorePunctuationAndAccents2,
                2, 20, 2, PriceTransforms.LogPrice);
        }

        public static Tuple<int, List<string>> phi14(string line, string header)
        {
            return phiSmart(line, header, StringCleaner.RemoveMorePunctuationAndAccents3,
                2, 20, 2, PriceTransforms.LogPrice);

        }

        public static Tuple<int, List<string>> phi15(string line, string header)
        {
            string newLine = Regex.Replace(line, "<br*>", "");
            return phiSmart2(line, header, StringCleaner.RemoveMorePunctuationAndAccents2,
                2, 20, 2, PriceTransforms.LogPrice);

        }

    }
}