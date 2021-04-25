// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the LICENSE.txt file in the solution root for more information.

using dotDoc.LibTiff.Extensions.Enumerations;

namespace dotDoc.LibTiff.Extensions
{
    /// <summary>
    /// Ocr Progress Object.
    /// </summary>
    public class OcrProgress
    {
        /// <summary>
        /// Create instance of <see cref="OcrProgress"/>.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="ocrStage">OCR Stage.</param>
        public OcrProgress(int page, OcrStages ocrStage)
        {
            Page = page;
            OcrStage = ocrStage;
        }

        /// <summary>
        /// Gets the page number..
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// Gets the Ocr Stage.
        /// </summary>
        public OcrStages OcrStage { get; }
    }
}