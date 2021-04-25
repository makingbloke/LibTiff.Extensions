// Copyright (c)2021 Mike King.
// This file is licensed to you under the MIT license.
// See the LICENSE.txt file in the solution root for more information.

using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using dotDoc.LibTiff.Extensions.Enumerations;
using dotDoc.LibTiff.Extensions.Exceptions;

namespace dotDoc.LibTiff.Extensions
{
    /// <summary>
    /// Tesseract OCR engine wrapper.
    /// </summary>
    public class TesseractOcr : IDisposable
    {
        #region api delegate declarations

        // cancel function delegate used by the API
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CancelFuncDelegate(IntPtr cancelThis, int words);

        // API delegates
        private delegate IntPtr TessBaseApiCreateDelegate();
        private delegate void TessBaseApiDeleteDelegate(IntPtr apiHandle);

        // This method takes ANSI strings. Turn off best fit mapping so no conversions are made and throw an exception if we hit a non-standard character.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, BestFitMapping = false, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true)]
        private delegate int TessBaseApiInitDelegate(IntPtr apiHandle, string dataPath, string language, TesseractEngineModes engineMode);

        private delegate void TessBaseApiSetPageSegModeDelegate(IntPtr apiHandle, TesseractPageSegModes pageSegMode);
        private delegate void TessBaseApiSetImageDelegate(IntPtr apiHandle, IntPtr pixHandle);
        private delegate void TessBaseApiClearDelegate(IntPtr apiHandle);
        private delegate int TessBaseApiRecognizeDelegate(IntPtr apiHandle, IntPtr monitorHandle);
        private delegate IntPtr TessBaseApiGetIteratorDelegate(IntPtr apiHandle);
        private delegate void TessPageIteratorDeleteDelegate(IntPtr iteratorHandle);
        private delegate void TessPageIteratorBeginDelegate(IntPtr iteratorHandle);
        private delegate int TessPageIteratorNextDelegate(IntPtr iteratorHandle, TesseractPageIteratorLevels pageIteratorLevel);
        private delegate int TessPageIteratorIsAtBeginningOfDelegate(IntPtr iteratorHandle, TesseractPageIteratorLevels pageIteratorLevel);
        private delegate IntPtr TessResultIteratorGetUtf8TextDelegate(IntPtr iteratorHandle, TesseractPageIteratorLevels pageIteratorLevel);
        private delegate void TessDeleteTextDelegate(IntPtr textHandle);
        private delegate float TessResultIteratorConfidenceDelegate(IntPtr iteratorHandle, TesseractPageIteratorLevels pageIteratorLevel);
        private delegate int TessPageIteratorBoundingBoxDelegate(IntPtr iteratorHandle, TesseractPageIteratorLevels pageIteratorLevel, out int left, out int top, out int right, out int bottom);

        // This method takes ANSI strings. Turn off best fit mapping so no conversions are made and throw an exception if we hit a non-standard character.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, BestFitMapping = false, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true)]
        private delegate IntPtr TessPdfRendererCreateDelegate(string outputBase, string dataPath, int textOnly);

        private delegate void TessDeleteResultRendererDelegate(IntPtr rendererHandle);

        // This method takes an ANSI string. Because it is a title we can safely use best fit mapping to convert unsupported characters to similar ANSI ones.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, BestFitMapping = true, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = false)]
        private delegate int TessResultRendererBeginDocumentDelegate(IntPtr rendererHandle, string title);

        private delegate int TessResultRendererEndDocumentDelegate(IntPtr rendererHandle);
        private delegate int TessResultRendererAddImageDelegate(IntPtr rendererHandle, IntPtr apiHandle);
        private delegate IntPtr TessMonitorCreateDelegate();
        private delegate void TessMonitorDeleteDelegate(IntPtr monitorHandle);
        private delegate void TessMonitorSetCancelFuncDelegate(IntPtr monitorHandle, [MarshalAs(UnmanagedType.FunctionPtr)] CancelFuncDelegate cancelFunc);
        private delegate IntPtr PixReadMemTiffDelegate(byte[] data, int length, int page);
        private delegate void PixDestroyDelegate(ref IntPtr pixHandle);

        #endregion api delegate declarations

        #region constants

        private const string TesseractDllName = "tesseract41";
        private const string LeptonicaDllName = "leptonica-1.80.0";

        #endregion constants

        #region private variables

        private bool _isDisposed;

        private readonly OcrOptions _ocrOptions;
        private readonly IntPtr _tesseractDllHandle;
        private readonly IntPtr _leptonicaDllHandle;

        private readonly TessBaseApiDeleteDelegate _tessBaseApiDelete;
        private readonly TessBaseApiSetPageSegModeDelegate _tessBaseApiSetPageSegMode;
        private readonly TessBaseApiSetImageDelegate _tessBaseApiSetImage;
        private readonly TessBaseApiClearDelegate _tessBaseApiClear;
        private readonly TessBaseApiRecognizeDelegate _tessBaseApiRecognize;
        private readonly TessBaseApiGetIteratorDelegate _tessBaseApiGetIterator;
        private readonly TessPageIteratorDeleteDelegate _tessPageIteratorDelete;
        private readonly TessPageIteratorBeginDelegate _tessPageIteratorBegin;
        private readonly TessPageIteratorNextDelegate _tessPageIteratorNext;
        private readonly TessPageIteratorIsAtBeginningOfDelegate _tessPageIteratorIsAtBeginningOf;
        private readonly TessResultIteratorGetUtf8TextDelegate _tessResultIteratorGetUtf8Text;
        private readonly TessDeleteTextDelegate _tessDeleteText;
        private readonly TessResultIteratorConfidenceDelegate _tessResultIteratorConfidence;
        private readonly TessPageIteratorBoundingBoxDelegate _tessPageIteratorBoundingBox;
        private readonly TessPdfRendererCreateDelegate _tessPdfRendererCreate;
        private readonly TessDeleteResultRendererDelegate _tessDeleteResultRenderer;
        private readonly TessResultRendererBeginDocumentDelegate _tessResultRendererBeginDocument;
        private readonly TessResultRendererEndDocumentDelegate _tessResultRendererEndDocument;
        private readonly TessResultRendererAddImageDelegate _tessResultRendererAddImage;
        private readonly TessMonitorCreateDelegate _tessMonitorCreate;
        private readonly TessMonitorDeleteDelegate _tessMonitorDelete;
        private readonly TessMonitorSetCancelFuncDelegate _tessMonitorSetCancelFunc;
        private readonly PixReadMemTiffDelegate _pixReadMemTiff;
        private readonly PixDestroyDelegate _pixDestroy;

        private readonly IntPtr _apiHandle;

        #endregion private variables

        #region constructors and destructors

        /// <summary>
        /// Create instance of <see cref="TesseractOcr"/>.
        /// </summary>
        /// <param name="ocrOptions">OCR options <see cref="OcrOptions"/>.</param>
        public TesseractOcr(OcrOptions ocrOptions = null)
        {
            _ocrOptions = ocrOptions ?? new();

            // load the tesseract and leptonica libraries
            string libraryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.Is64BitProcess ? "x64" : "x86");
            _tesseractDllHandle = NativeLibrary.Load(Path.Combine(libraryPath, TesseractDllName + ".dll"));
            _leptonicaDllHandle = NativeLibrary.Load(Path.Combine(libraryPath, LeptonicaDllName + ".dll"));

            // create a delegate for each of the api calls 
            TessBaseApiCreateDelegate tessBaseApiCreate = Marshal.GetDelegateForFunctionPointer<TessBaseApiCreateDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPICreate"));
            TessBaseApiInitDelegate tessBaseApiInit = Marshal.GetDelegateForFunctionPointer<TessBaseApiInitDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPIInit2"));

            _tessBaseApiDelete = Marshal.GetDelegateForFunctionPointer<TessBaseApiDeleteDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPIDelete"));
            _tessBaseApiSetPageSegMode = Marshal.GetDelegateForFunctionPointer<TessBaseApiSetPageSegModeDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPISetPageSegMode"));
            _tessBaseApiSetImage = Marshal.GetDelegateForFunctionPointer<TessBaseApiSetImageDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPISetImage2"));
            _tessBaseApiClear = Marshal.GetDelegateForFunctionPointer<TessBaseApiClearDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPIClear"));
            _tessBaseApiRecognize = Marshal.GetDelegateForFunctionPointer<TessBaseApiRecognizeDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPIRecognize"));
            _tessBaseApiGetIterator = Marshal.GetDelegateForFunctionPointer<TessBaseApiGetIteratorDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessBaseAPIGetIterator"));
            _tessPageIteratorDelete = Marshal.GetDelegateForFunctionPointer<TessPageIteratorDeleteDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessPageIteratorDelete"));
            _tessPageIteratorBegin = Marshal.GetDelegateForFunctionPointer<TessPageIteratorBeginDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessPageIteratorBegin"));
            _tessPageIteratorNext = Marshal.GetDelegateForFunctionPointer<TessPageIteratorNextDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessPageIteratorNext"));
            _tessPageIteratorIsAtBeginningOf = Marshal.GetDelegateForFunctionPointer<TessPageIteratorIsAtBeginningOfDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessPageIteratorIsAtBeginningOf"));
            _tessResultIteratorGetUtf8Text = Marshal.GetDelegateForFunctionPointer<TessResultIteratorGetUtf8TextDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessResultIteratorGetUTF8Text"));
            _tessDeleteText = Marshal.GetDelegateForFunctionPointer<TessDeleteTextDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessDeleteText"));
            _tessResultIteratorConfidence = Marshal.GetDelegateForFunctionPointer<TessResultIteratorConfidenceDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessResultIteratorConfidence"));
            _tessPageIteratorBoundingBox = Marshal.GetDelegateForFunctionPointer<TessPageIteratorBoundingBoxDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessPageIteratorBoundingBox"));
            _tessPdfRendererCreate = Marshal.GetDelegateForFunctionPointer<TessPdfRendererCreateDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessPDFRendererCreate"));
            _tessDeleteResultRenderer = Marshal.GetDelegateForFunctionPointer<TessDeleteResultRendererDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessDeleteResultRenderer"));
            _tessResultRendererBeginDocument = Marshal.GetDelegateForFunctionPointer<TessResultRendererBeginDocumentDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessResultRendererBeginDocument"));
            _tessResultRendererEndDocument = Marshal.GetDelegateForFunctionPointer<TessResultRendererEndDocumentDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessResultRendererEndDocument"));
            _tessResultRendererAddImage = Marshal.GetDelegateForFunctionPointer<TessResultRendererAddImageDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessResultRendererAddImage"));
            _tessMonitorCreate = Marshal.GetDelegateForFunctionPointer<TessMonitorCreateDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessMonitorCreate"));
            _tessMonitorDelete = Marshal.GetDelegateForFunctionPointer<TessMonitorDeleteDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessMonitorDelete"));
            _tessMonitorSetCancelFunc = Marshal.GetDelegateForFunctionPointer<TessMonitorSetCancelFuncDelegate>(NativeLibrary.GetExport(_tesseractDllHandle, "TessMonitorSetCancelFunc"));
            _pixReadMemTiff = Marshal.GetDelegateForFunctionPointer<PixReadMemTiffDelegate>(NativeLibrary.GetExport(_leptonicaDllHandle, "pixReadMemTiff"));
            _pixDestroy = Marshal.GetDelegateForFunctionPointer<PixDestroyDelegate>(NativeLibrary.GetExport(_leptonicaDllHandle, "pixDestroy"));

            // initialise the tesseract API.
            _apiHandle = tessBaseApiCreate();

            if (tessBaseApiInit(_apiHandle, _ocrOptions.DataPath, _ocrOptions.Language, _ocrOptions.EngineMode) != 0) throw new TesseractException($"Failure in {nameof(tessBaseApiInit)}");
        }

        /// <summary>
        /// Releases all resources used by <see cref="TesseractOcr"/>.
        /// </summary>
        ~TesseractOcr() => Dispose(false);

        #endregion constructors and destructors

        #region protected methods

        /// <summary>
        /// Releases all resources used by <see cref="TesseractOcr"/>.
        /// </summary>
        /// <param name="_">(Disposing) Here as it is part of the pattern. Not used as we only need to free unmanaged items.</param>
        protected virtual void Dispose(bool _)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                _tessBaseApiDelete(_apiHandle);

                NativeLibrary.Free(_leptonicaDllHandle);
                NativeLibrary.Free(_tesseractDllHandle);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Releases all resources used by <see cref="TesseractOcr"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ocr the specified page range.
        /// </summary>
        /// <param name="tiff">The <see cref="Tiff"/> image.</param>
        /// <param name="startPage">The start page number.</param>
        /// <param name="endPage">The end page number.</param>
        /// <param name="progress">OCR Progress handler <see cref="IProgress{OcrProgress}"/>.</param>
        /// <param name="cancellationToken">Cancellation token <see cref="CancellationToken"/>.</param>
        /// <returns>A dictionary containing the results <see cref="IDictionary{int, List{OcrWord}}"/>.</returns>
        public Task<IDictionary<int, IList<OcrWord>>> OcrPagesAsync(Tiff tiff, int startPage = 0, int endPage = int.MaxValue, IProgress<OcrProgress> progress = null, CancellationToken cancellationToken = default)
        {
            if (tiff == null) throw new ArgumentNullException(nameof(tiff));

            if (startPage < 0) throw new ArgumentException("Invalid start page (< 0)", nameof(startPage));

            int maxPage = tiff.NumberOfDirectories() - 1;

            if (endPage == int.MaxValue)
            {
                endPage = maxPage;
            }
            else if (endPage > maxPage)
            {
                throw new ArgumentException($"Invalid end page (> {maxPage})", nameof(endPage));
            }

            if (startPage > endPage) throw new ArgumentException("Invalid start page (greater than end page)", nameof(startPage));

            return Task.Run(() =>
            {
                try
                {
                    IDictionary<int, IList<OcrWord>> ocrResult = new Dictionary<int, IList<OcrWord>>();

                    for (int page = startPage; page <= endPage; page++)
                    {
                        OcrPage(page, tiff, progress, cancellationToken);

                        OnOcrProgress(page, OcrStages.ProcessResults, progress, cancellationToken);

                        IntPtr iteratorHandle = _tessBaseApiGetIterator(_apiHandle);
                        try
                        {
                            _tessPageIteratorBegin(iteratorHandle);

                            bool newLine = true;
                            int line = 0;

                            do
                            {
                                // if we are on a line with text (!newline) and we hit then start of a new line then move down a line
                                if (!newLine && _tessPageIteratorIsAtBeginningOf(iteratorHandle, TesseractPageIteratorLevels.TextLine) != 0)
                                {
                                    line++;
                                    newLine = true;
                                }

                                // if we are not at the beginning of a word then skip to the next word (This may only happen on the first time in).
                                if (_tessPageIteratorIsAtBeginningOf(iteratorHandle, TesseractPageIteratorLevels.Word) != 0)
                                {
                                    // get the text of the word, copy it to a managed string and then delete the text handle.
                                    string text;
                                    IntPtr textHandle = _tessResultIteratorGetUtf8Text(iteratorHandle, TesseractPageIteratorLevels.Word);
                                    try
                                    {
                                        text = Marshal.PtrToStringUTF8(textHandle);
                                    }
                                    finally
                                    {
                                        _tessDeleteText(textHandle);
                                    }

                                    // if the word isn't just whitespace then add it to the results
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        if (_tessPageIteratorBoundingBox(iteratorHandle, TesseractPageIteratorLevels.Word, out int left, out int top, out int right, out int bottom) == 0) throw new TesseractException($"Failure in {nameof(_tessPageIteratorBoundingBox)}");

                                        float confidence = _tessResultIteratorConfidence(iteratorHandle, TesseractPageIteratorLevels.Word);

                                        OcrWord ocrWord = new (left, top, right - left + 1, bottom - top + 1, line, text, confidence);
                                        if (ocrResult.TryGetValue(page, out IList<OcrWord> ocrWordList))
                                        {
                                            ocrWordList.Add(ocrWord);
                                        }
                                        else
                                        {
                                            ocrResult[page] = new List<OcrWord> { ocrWord };
                                        }

                                        newLine = false;
                                    }
                                }
                            } while (_tessPageIteratorNext(iteratorHandle, TesseractPageIteratorLevels.Word) != 0);
                        }
                        finally
                        {
                            _tessPageIteratorDelete(iteratorHandle);
                        }

                        _tessBaseApiClear(_apiHandle);
                    }

                    OnOcrProgress(0, OcrStages.Completed, progress, cancellationToken);
                    return ocrResult;
                }
                catch (Exception)
                {
                    OnOcrProgress(0, OcrStages.Failed, progress, cancellationToken);
                    throw;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Convert a tiff image into a searchable Pdf.
        /// </summary>
        /// <param name="tiff">The <see cref="Tiff"/> image.</param>
        /// <param name="fileName">The Pdf filename.</param>
        /// <param name="startPage">The start page number.</param>
        /// <param name="endPage">The end page number.</param>
        /// <param name="progress">OCR Progress handler <see cref="IProgress{OcrProgress}"/>.</param>
        /// <param name="cancellationToken">Cancellation token <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task"/>.</returns>
        public Task CreateSearchablePdfAsync(Tiff tiff, string fileName, int startPage = 0, int endPage = int.MaxValue, IProgress<OcrProgress> progress = null, CancellationToken cancellationToken = default)
        {

            if (tiff == null) throw new ArgumentNullException(nameof(tiff));

            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            if (startPage < 0) throw new ArgumentException("Invalid start page (< 0)", nameof(startPage));

            int maxPage = tiff.NumberOfDirectories() - 1;

            if (endPage == int.MaxValue)
            {
                endPage = maxPage;
            }
            else if (endPage > maxPage)
            {
                throw new ArgumentException($"Invalid end page (> {maxPage})", nameof(endPage));
            }

            if (startPage > endPage) throw new ArgumentException("Invalid start page (greater than end page)", nameof(startPage));

            return Task.Run(() =>
            {
                try
                {
                    IntPtr rendererHandle = _tessPdfRendererCreate(Path.ChangeExtension(fileName, null), _ocrOptions.DataPath, 0);

                    try
                    {
                        if (_tessResultRendererBeginDocument(rendererHandle, Path.GetFileNameWithoutExtension(fileName)) == 0) throw new TesseractException($"Failure in {nameof(_tessResultRendererBeginDocument)}");

                        try
                        {
                            for (int page = startPage; page <= endPage; page++)
                            {
                                OcrPage(page, tiff, progress, cancellationToken);

                                OnOcrProgress(page, OcrStages.RenderPage, progress, cancellationToken);

                                if (_tessResultRendererAddImage(rendererHandle, _apiHandle) == 0) throw new TesseractException($"Failure in {nameof(_tessResultRendererAddImage)}");

                                _tessBaseApiClear(_apiHandle);
                            }
                        }
                        finally
                        {
                            _tessResultRendererEndDocument(rendererHandle);
                        }
                    }
                    finally
                    {
                        _tessDeleteResultRenderer(rendererHandle);
                    }

                    OnOcrProgress(0, OcrStages.Completed, progress, cancellationToken);
                }
                catch (Exception)
                {
                    OnOcrProgress(0, OcrStages.Failed, progress, cancellationToken);
                    throw;
                }
            }, cancellationToken);
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// OCR a Tiff page.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="tiff">The <see cref="Tiff"/> image to be processed.</param>
        /// <param name="progress">Tesseract progress handler <see cref="IProgress{OcrProgress}"/>.</param>
        /// <param name="cancellationToken">Cancellation token <see cref="CancellationToken"/>.</param>
        private void OcrPage(int page, Tiff tiff, IProgress<OcrProgress> progress, CancellationToken cancellationToken)
        {
            OnOcrProgress(page, OcrStages.LoadPage, progress, cancellationToken);

            // Read the Tiff page into an array and then open this with Leptonica (PixReadMemTiff).
            byte[] tiffBytes;

            using (MemoryStream ms = new())
            {
                tiff.SetPage(page);

                using (Tiff tmpTiff = UtilityExtensions.Open(ms, "w", nameof(tmpTiff)))
                {
                    tiff.CopyPage(tmpTiff);
                }

                tiffBytes = ms.ToArray();
            }

            OnOcrProgress(page, OcrStages.OcrPage, progress, cancellationToken);

            IntPtr pixHandle = _pixReadMemTiff(tiffBytes, tiffBytes.Length, 0);
            try
            {
                // set the page segmentation mode and set the image tesseract will use
                _tessBaseApiSetPageSegMode(_apiHandle, _ocrOptions.PageSegMode);
                _tessBaseApiSetImage(_apiHandle, pixHandle);

                // setup the cancellation function and OCR (recognize) the page
                IntPtr monitorHandle = _tessMonitorCreate();

                try
                {
                    // set the Cancel Function - returns 1 if cancellation has been requested by the cancellation token otherwise 0
                    int CancelFunc(IntPtr cancelThis, int words) => cancellationToken.IsCancellationRequested ? 1 : 0;
                    _tessMonitorSetCancelFunc(monitorHandle, CancelFunc);

                    // run the OCR on the image. This returns non zero if there's been a failure (such as the Cancel Function returning 1).
                    if (_tessBaseApiRecognize(_apiHandle, monitorHandle) != 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new TesseractException($"Failure in {nameof(_tessBaseApiRecognize)}");
                    }
                }
                finally
                {
                    _tessMonitorDelete(monitorHandle);
                }
            }
            finally
            {
                _pixDestroy(ref pixHandle);
            }
        }

        /// <summary>
        /// OCR progress method.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="ocrStage">The current Ocr stage being processed.</param>
        /// <param name="progress">Tesseract progress handler <see cref="IProgress{OcrProgress}"/>.</param>
        /// <param name="cancellationToken">Cancellation token <see cref="CancellationToken"/>.</param>
        private static void OnOcrProgress(int page, OcrStages ocrStage, IProgress<OcrProgress> progress, CancellationToken cancellationToken)
        {
            progress?.Report(new OcrProgress(page, ocrStage));
            cancellationToken.ThrowIfCancellationRequested();
        }

        #endregion private methods
    }
}
