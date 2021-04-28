// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace dotDoc.LibTiff.Extensions.Enumerations
{
    /// <summary>
    /// Tesseract OCR engine modes.
    /// </summary>
    public enum TesseractEngineModes
    {
        /// <summary>
        /// Only the legacy tesseract OCR engine is used.
        /// </summary>
        TesseractOnly = 0,

        /// <summary>
        /// Only the new LSTM-based OCR engine is used.
        /// </summary>
        LstmOnly = 1,

        /// <summary>
        /// Both the legacy and new LSTM based OCR engine is used.
        /// </summary>
        TesseractAndLstm = 2,

        /// <summary>
        /// The default OCR engine is used (currently LSTM-based OCR engine).
        /// </summary>
        Default = 3
    }
}