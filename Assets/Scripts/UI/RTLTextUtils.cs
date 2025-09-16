using UnityEngine;
using System.Text;
using System.Globalization;

public static class RTLTextUtils
{
    public static bool IsRTLCharacter(char c)
    {
        // Arabic Unicode range
        if (c >= 0x0600 && c <= 0x06FF)
            return true;
        
        // Hebrew Unicode range
        if (c >= 0x0590 && c <= 0x05FF)
            return true;
        
        // Persian/Farsi extensions
        if (c >= 0xFB50 && c <= 0xFDFF)
            return true;
        
        // Arabic presentation forms
        if (c >= 0xFE70 && c <= 0xFEFF)
            return true;
        
        return false;
    }

    public static bool IsNeutralCharacter(char c)
    {
        return char.IsNumber(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c);
    }

    public static string ReverseString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        char[] chars = input.ToCharArray();
        int start = 0;
        int end = chars.Length - 1;

        while (start < end)
        {
            while (end > start && char.GetUnicodeCategory(chars[end]) == UnicodeCategory.NonSpacingMark)
                end--;

            while (start < end && char.GetUnicodeCategory(chars[start]) == UnicodeCategory.NonSpacingMark)
                start++;

            if (start < end)
            {
                char temp = chars[start];
                chars[start] = chars[end];
                chars[end] = temp;
                start++;
                end--;
            }
        }

        return new string(chars);
    }
}