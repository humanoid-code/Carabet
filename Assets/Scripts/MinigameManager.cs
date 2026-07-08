using System;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    private List<string> pieces;
    //private List<PhraseButton> buttons;

    private int currentCorrectIndex;
    private bool gameRunning;
    private float moveSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.DebugSplitPhrase("You will get money in the next month", 3);
    }

    void Update()
    {
    }

    public void StartGame(
        string phrase,
        int piecesCount,
        float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
        gameRunning = true;
        currentCorrectIndex = 0;
        pieces = Split(phrase, piecesCount);
        //buttons = new List<PhraseButton>();
    }

    public void DebugSplitPhrase(string phrase, int piecesCount)
    {
        var parts = Split(phrase, piecesCount);
        var logMessage = $"Split phrase: {phrase}\nRequested pieces: {piecesCount}\nResult:";

        for (var i = 0; i < parts.Count; i++)
        {
            logMessage += $"\n[{i + 1}] {parts[i]}";
        }

        Debug.Log(logMessage);
    }

    public static List<string> Split(string phrase, int piecesCount)
    {
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return new List<string>();
        }

        var trimmedPhrase = phrase.Trim();
        if (piecesCount <= 1)
        {
            return new List<string> { trimmedPhrase };
        }

        var words = trimmedPhrase.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return new List<string>();
        }

        if (words.Length >= piecesCount)
        {
            var actualPiecesCount = Mathf.Min(piecesCount, words.Length);
            var result = new List<string>(actualPiecesCount);
            var baseSize = words.Length / actualPiecesCount;
            var remainder = words.Length % actualPiecesCount;
            var index = 0;

            for (var i = 0; i < actualPiecesCount; i++)
            {
                var size = baseSize + (i < remainder ? 1 : 0);
                if (size <= 0)
                {
                    continue;
                }

                result.Add(string.Join(" ", words, index, size));
                index += size;
            }

            return result;
        }

        var chars = trimmedPhrase.ToCharArray();
        var charPiecesCount = Mathf.Min(piecesCount, chars.Length);
        var charResult = new List<string>(charPiecesCount);
        var charBaseSize = chars.Length / charPiecesCount;
        var charRemainder = chars.Length % charPiecesCount;
        var charIndex = 0;

        for (var i = 0; i < charPiecesCount; i++)
        {
            var size = charBaseSize + (i < charRemainder ? 1 : 0);
            if (size <= 0)
            {
                continue;
            }

            charResult.Add(new string(chars, charIndex, size));
            charIndex += size;
        }

        return charResult;
    }
}
