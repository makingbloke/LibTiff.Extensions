// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the LICENSE.txt file in the solution root for more information.

namespace dotDoc.LibTiff.Extensions.Enumerations
{
    /// <summary>
    /// Stages in the Ocr process.
    /// </summary>
    public enum OcrStages
    {
        /// <summary>
        ///No stage set (Default).
        /// </summary>
        None,

        /// <summary>
        /// Loading page.
        /// </summary>
        LoadPage,

        /// <summary>
        /// Performing OCR on page.
        /// </summary>
        OcrPage,

        /// <summary>
        /// Process OCR results.
        /// </summary>
        ProcessResults,

        /// <summary>
        /// Write page and OCR results.
        /// </summary>
        WritePage,

        /// <summary>
        /// Render a searchable PDF page.
        /// </summary>
        RenderPage,

        /// <summary>
        /// Process finished.
        /// </summary>
        Completed,

        /// <summary>
        /// Process failed.
        /// </summary>
        Failed
    }
}
