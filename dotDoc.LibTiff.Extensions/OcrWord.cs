// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the LICENSE.txt file in the solution root for more information.

namespace dotDoc.LibTiff.Extensions
{
    /// <summary>
    /// Ocr'ed Word
    /// </summary>
    public class OcrWord
    {
        /// <summary>
        /// Create instance of <see cref="OcrWord"/>.
        /// </summary>
        /// <param name="x">X co-ordinate of the Ocr'ed word.</param>
        /// <param name="y">Y co-ordinate of the Ocr'ed word.</param>
        /// <param name="width">Width of the Ocr'ed word.</param>
        /// <param name="height">Height of the Ocr'ed word.</param>
        /// <param name="line">Line number of the Ocr'ed word.</param>
        /// <param name="text">Text of the Ocr'ed word.</param>
        /// <param name="confidence">Recognition confidence (%) of the Ocr'ed word.</param>
        public OcrWord(int x, int y, int width, int height, int line, string text, float confidence)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Line = line;
            Text = text;
            Confidence = confidence;
        }

        /// <summary>
        /// Gets the X co-ordinate of the Ocr'ed word.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y co-ordinate of the Ocr'ed word.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the width of the Ocr'ed word.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the Ocr'ed word.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the line number of the Ocr'ed word.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the text of the Ocr'ed word.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the recognition confidence (%) of the Ocr'ed word.
        /// </summary>
        public float Confidence { get; }

        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns>A formatted string containing the values of all the properties.</returns>
        public override string ToString() => $"X: {X} Y: {Y} Width: {Width} Height: {Height} Line: {Line} Text: {Text} Confidence: {Confidence}";
    }
}
