// Copyright ©2021 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace dotDoc.LibTiff.Extensions.Enumerations
{
    public enum TesseractPageIteratorLevels
    {
        /// <summary>
        /// Block of text/image/separator line.
        /// </summary>
        Block = 0,

        /// <summary>
        /// Paragraph within a block.
        /// </summary>
        Para = 1,

        /// <summary>
        /// Line within a paragraph.
        /// </summary>
        TextLine = 2,

        /// <summary>
        /// // Word within a textline.
        /// </summary>
        Word = 3,

        /// <summary>
        /// // Symbol/character within a word.
        /// </summary>
        Symbol = 4
    }
}