// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the LICENSE.txt file in the solution root for more information.

using System;
using System.IO;
using dotDoc.LibTiff.Extensions.Enumerations;

namespace dotDoc.LibTiff.Extensions
{
    /// <summary>
    /// OCR options.
    /// </summary>
    public class OcrOptions
    {
        /// <summary>
        /// Tesseract data folder.
        /// </summary>
        public string DataPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TesseractData");

        /// <summary>
        /// Tesseract language.
        /// </summary>
        public string Language { set; get; } = "eng";

        /// <summary>
        /// Tesseract engine mode.
        /// </summary>
        public TesseractEngineModes EngineMode { set; get; } = TesseractEngineModes.Default;

        /// <summary>
        /// Tesseract page layout analysis mode.
        /// </summary>
        public TesseractPageSegModes PageSegMode { set; get; } = TesseractPageSegModes.Auto;
    }
}
