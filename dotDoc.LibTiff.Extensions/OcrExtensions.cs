// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dotDoc.LibTiff.Extensions
{
    /// <summary>
    /// LibTiff Ocr Extension Methods.
    /// </summary>
    public static class OcrExtensions
    {
        /// <summary>
        /// Ocr the specified page range.
        /// </summary>
        /// <param name="tiff">The <see cref="Tiff"/> image.</param>
        /// <param name="startPage">The start page number.</param>
        /// <param name="endPage">The end page number.</param>
        /// <param name="ocrOptions">Ocr engine options.</param>
        /// <param name="progress">OCR Progress handler <see cref="IProgress{OcrProgress}"/>.</param>
        /// <param name="cancellationToken">Cancellation token <see cref="CancellationToken"/>.</param>
        /// <returns>A dictionary containing the results <see cref="IDictionary{int, List{OcrWord}}"/>.</returns>
        public static async Task<IDictionary<int, IList<OcrWord>>> OcrPagesAsync(this Tiff tiff, int startPage = 0, int endPage = int.MaxValue, OcrOptions ocrOptions = null, IProgress<OcrProgress> progress = null, CancellationToken cancellationToken = default)
        {
            using TesseractOcr tesseractOcr = new(ocrOptions);
            return await tesseractOcr.OcrPagesAsync(tiff, startPage, endPage, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Convert a tiff image into a searchable Pdf.
        /// </summary>
        /// <param name="tiff">The <see cref="Tiff"/> image.</param>
        /// <param name="fileName">The Pdf filename.</param>
        /// <param name="startPage">The start page number.</param>
        /// <param name="endPage">The end page number.</param>
        /// <param name="ocrOptions">Ocr engine options.</param>
        /// <param name="progress">OCR Progress handler <see cref="IProgress{OcrProgress}"/>.</param>
        /// <param name="cancellationToken">Cancellation token <see cref="CancellationToken"/>.</param>
        public static async Task CreateSearchablePdfAsync(this Tiff tiff, string fileName, int startPage = 0, int endPage = int.MaxValue, OcrOptions ocrOptions = null, IProgress<OcrProgress> progress = null, CancellationToken cancellationToken = default)
        {
            using TesseractOcr tesseractOcr = new(ocrOptions);
            await tesseractOcr.CreateSearchablePdfAsync(tiff, fileName, startPage, endPage, progress, cancellationToken).ConfigureAwait(false);
        }
    }
}
