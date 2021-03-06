﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Globalization;

using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;

//using Tesseract;
using IronOcr;

using OfficeOpenXml;

namespace EstrattoContoOCR
{
    /// <summary>
    /// Logica di interazione per OCRWindow.xaml
    /// </summary>
    /// 

    public class CorrectionObject
    {
        public List<Visual> elements;
        public EditingToolType type;

        public CorrectionObject()
        {
            type = EditingToolType.Pencil;
            elements = new List<Visual>();
        }

        public int Count
        {
            get
            {
                return elements.Count;
            }
        }

        public CorrectionObject Add ( Visual obj )
        {
            elements.Add(obj);

            return this;
        }

        public void Clear()
        {
            elements.Clear();
        }
    };

    public class OperationEntry
    {

        public RecognizedArea DataOperazione;
        public RecognizedArea DataValuta;
        public RecognizedArea Dare;
        public RecognizedArea Avere;
        public RecognizedArea Descrizione;

        public OperationEntry ()
        {
            
        }

        
    };

    public partial class OCRWindow : Window, ISelectionAreaDelegate, IEditingToolDialogDelegate
    {

        private double mAspectRatio;
        private BitmapImage mImage;

        private String mImagePath;
        private ContextMenu mOptionsMenu;

        /*private SelectionArea mDataOperazioneArea;
        private SelectionArea mDataValutaArea;
        private SelectionArea mDareArea;
        private SelectionArea mAvereArea;
        private SelectionArea mDescrizioneArea;*/

        private List<SelectionArea> mDataOperazioneAreas;
        private List<SelectionArea> mDataValutaAreas;
        private List<SelectionArea> mDareAreaAreas;
        private List<SelectionArea> mAvereAreaAreas;
        private List<SelectionArea> mDescrizioneAreas;


        private SelectionArea mDraggingArea;
        private Rectangle mDraggingRectangle;
        //private TesseractEngine mOcrEngine;
        private AdvancedOcr mOcrEngine;
        //private Pix mAnalyzableImage;
        private System.Drawing.Bitmap mAnalyzableImage;

        private Dictionary<int,CorrectionObject> mCorrections;
        //private List<Visual> mElementToDraw;
        private CorrectionObject mElementToDraw;

        private bool mReady;

        private MemoryStream mAnalizeStream;
        private MemoryStream mImageStream;

        EditingDialogBox mEditDialogBox;

        private Point mCurrentEditPoint;
        private Brush mCurrentBrush;
        private double mCurrentThickness;
        private int mLastCorrectionSelected;

        private Stack<string> mTemporaryFiles;
        private string mTemporaryDir;

        private FileInfo mFileInfo;
        private string mWorksheetName;
        private ExcelPackage mExcelFile;
        private int mLastRowInserted;
        private ExcelWorksheet mExcelActiveWorksheet;
        private bool mEditingActive;

        public OCRWindow()
        {
            InitializeComponent();

            mOptionsMenu = new ContextMenu();
            //RefreshOptionsMenu();

            mImageCanvas.ContextMenu = mOptionsMenu;
            mImageCanvas.ContextMenuOpening += mImageCanvas_ContextMenuOpening;

            /*mDataOperazioneArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DataOperazioneArea, this);
            mDataValutaArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DataValutaArea, this);
            mDareArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DareArea, this);
            mAvereArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.AvereArea, this);
            mDescrizioneArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DescrizioneArea, this);

            mDataValutaArea.AddToCanvas(mImageCanvas);
            mDataOperazioneArea.AddToCanvas(mImageCanvas);
            mDareArea.AddToCanvas(mImageCanvas);
            mAvereArea.AddToCanvas(mImageCanvas);
            mDescrizioneArea.AddToCanvas(mImageCanvas);*/

            mDataOperazioneAreas = new List<SelectionArea>();
            mDataValutaAreas = new List<SelectionArea>();
            mDareAreaAreas = new List<SelectionArea>();
            mAvereAreaAreas = new List<SelectionArea>();
            mDescrizioneAreas = new List<SelectionArea>();

            mImageCanvas.MouseDown += SelectionArea_MouseLeftButtonDown;
            mImageCanvas.MouseUp += SelectionArea_MouseLeftButtonUp;
            mImageCanvas.MouseMove += SelectionArea_MouseMove;
            mImageCanvas.MouseEnter += SelectionArea_MouseEnter;
            mImageCanvas.MouseLeave += SelectionArea_MouseLeave;
            mDraggingArea = null;

            ////////TEMPORANEO !!! USARE CON VARIABILE D'AMBIENTE TESSDATA_PREFIX
            /*mOcrEngine = new TesseractEngine("tesseract-ocr\\tessdata", "ita", EngineMode.TesseractOnly);

            mOcrEngine.SetVariable("load_system_dawg", false);
            mOcrEngine.SetVariable("load_freq_dawg", false);
            mOcrEngine.SetVariable("load_punc_dawg", false);
            mOcrEngine.SetVariable("load_number_dawg", false);
            mOcrEngine.SetVariable("load_unambig_dawg", false);
            mOcrEngine.SetVariable("load_bigram_dawg", false);*/

            mOcrEngine = new AdvancedOcr()
            {
                CleanBackgroundNoise = true,
                EnhanceContrast = true,
                EnhanceResolution = true,
                Language = IronOcr.Languages.Italian.OcrLanguagePack,
                Strategy = IronOcr.AdvancedOcr.OcrStrategy.Advanced,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                DetectWhiteTextOnDarkBackgrounds = false,
                InputImageType = AdvancedOcr.InputTypes.Document,
                RotateAndStraighten = true,
                ReadBarCodes = false,
                ColorDepth = 4
            };

            this.Closing += OCRWindow_Closing;

            mEditDialogBox = new EditingDialogBox(this);

            mEditDialogBox.Hide();

            mCorrections = new Dictionary<int, CorrectionObject>();
            //mElementToDraw = new List<Visual>();
            mElementToDraw = new CorrectionObject();

            mLastCorrectionSelected = -2;

            mTemporaryFiles = new Stack<string>();
            mTemporaryDir = Guid.NewGuid().ToString();

            try
            {
                Directory.CreateDirectory(mTemporaryDir);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Errore creazione directory file temporanei...Chiusura applicazione!");

                Application.Current.Shutdown();
            }

            mExcelFile = null;
        }

        public bool OCRWindowReady
        {
            get
            {
                return mReady;
            }
        }

        public void EditingToggle(bool actualState)
        {
            //attivazione della modalita di editing
            if ( actualState )
            {
                //attivato!! 
                //nascondo tutte le aree
                HideAllSelectionAreas();

                //attivo gli eventi mouse down/move/up per il canvas
                mImageCanvas.MouseDown -= SelectionArea_MouseLeftButtonDown;
                mImageCanvas.MouseUp -= SelectionArea_MouseLeftButtonUp;
                mImageCanvas.MouseMove -= SelectionArea_MouseMove;

                mImageCanvas.MouseDown += Editing_MouseDown;
                mImageCanvas.MouseMove += Editing_MouseMove;
                mImageCanvas.MouseUp += Editing_MouseUp;

                //cambio cursore
                mImageCanvas.Cursor = Cursors.Cross;
            }
            else
            {
                //disattivato
                //mostro le aree
                ShowAllSelectionAreas();

                //attivo gli eventi mouse down/move/up per il canvas
                mImageCanvas.MouseDown -= Editing_MouseDown;
                mImageCanvas.MouseMove -= Editing_MouseMove;
                mImageCanvas.MouseUp -= Editing_MouseUp;

                mImageCanvas.MouseDown += SelectionArea_MouseLeftButtonDown;
                mImageCanvas.MouseUp += SelectionArea_MouseLeftButtonUp;
                mImageCanvas.MouseMove += SelectionArea_MouseMove;


                //ripristino cursore
                mImageCanvas.Cursor = Cursors.Arrow;

                mElementToDraw = null;
            }

            mEditingActive = actualState;

            /*if (mEditingActive)
            {
                mImageCanvas.ContextMenuOpening -= mImageCanvas_ContextMenuOpening;
            }
            else
            {
                mImageCanvas.ContextMenuOpening += mImageCanvas_ContextMenuOpening;
            }*/
        }

        void Editing_MouseUp(object sender, MouseButtonEventArgs e)
        {

            //aggiungo
            int item = mEditDialogBox.AddCorrection(mElementToDraw.Count);

            mCorrections.Add(item, mElementToDraw);

            //resetto
            //mElementToDraw.Clear();
            mElementToDraw = null;

        }

        void Editing_MouseMove(object sender, MouseEventArgs e)
        {
            if ( mElementToDraw != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();
                line.SetValue(Canvas.ZIndexProperty, 1);
                line.Stroke = mCurrentBrush;
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;
                line.StrokeLineJoin = PenLineJoin.Round;
                line.StrokeThickness = mCurrentThickness;
                line.X1 = mCurrentEditPoint.X;
                line.Y1 = mCurrentEditPoint.Y;
                line.X2 = e.GetPosition(mImageCanvas).X;
                line.Y2 = e.GetPosition(mImageCanvas).Y;

                mCurrentEditPoint = e.GetPosition(mImageCanvas);

                mImageCanvas.Children.Add(line);

                mElementToDraw.Add(line);

            }
        }

        void Editing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                mCurrentEditPoint = e.GetPosition(mImageCanvas);
                mCurrentBrush = mEditDialogBox.CurrentBrush;
                mCurrentThickness = (double)mEditDialogBox.ToolThickness;
                
                //nuova area;
                mElementToDraw = new CorrectionObject();
            }
        }

        public void EditingUndoCorrection(int idx)
        {
            if ( idx == -1 )
            {
                //rimuovo tutto
                foreach ( int key in mCorrections.Keys )
                {
                    CorrectionObject corr = mCorrections[key];

                    foreach (UIElement obj in corr.elements)
                    {
                        mImageCanvas.Children.Remove(obj);
                    }

                    corr.Clear();
                }

                mCorrections.Clear();

                mEditDialogBox.RemoveAllCorrection();

                return;
            }

            CorrectionObject to_be_removed = null;

            if ( mCorrections.ContainsKey(idx) )
            {
                to_be_removed = mCorrections[idx];
            }

            mCorrections.Remove(idx);

            foreach ( UIElement obj in to_be_removed.elements )
            {
                mImageCanvas.Children.Remove(obj);
            }

            //notifico
            mEditDialogBox.RemoveCorrection(idx);
        }

        public void EditingSaveCorrections()
        {
            //salvo tutto su file, salvo l'immagine attuale in temporaneo

            //creo file temporaneo
            string filename = null;

            try
            {

                filename = mTemporaryDir + "\\" + Guid.NewGuid().ToString() + ".tmp";

                MemoryStream ms_save = new MemoryStream();

                mAnalyzableImage.Save(ms_save, System.Drawing.Imaging.ImageFormat.Png);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(ms_save));
             
                FileStream fs = File.Create(filename);
                encoder.Save(fs);

                fs.Close();
                ms_save.Dispose();

                //push sullo stack

                mTemporaryFiles.Push(filename);

            }
            catch ( Exception exc )
            {
                MessageBox.Show("Errore durante il salvataggo dell'immagine..." + exc.Message );
            }

            //applico le modifiche all'immagine
            RenderTargetBitmap target = new RenderTargetBitmap(mAnalyzableImage.Width,
                mAnalyzableImage.Height, mAnalyzableImage.HorizontalResolution, mAnalyzableImage.VerticalResolution,
                System.Windows.Media.PixelFormats.Pbgra32);

            /*foreach ( Visual elem in elements )
            {
                target.Render(elem);
            }*/
            try
            {
                //rendo invisibili tutte le aree

                HideAllSelectionAreas();

                VisualBrush sourceBrush = new VisualBrush(mImageCanvas);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                Transform t = new ScaleTransform(96 / mAnalyzableImage.HorizontalResolution, 96 / mAnalyzableImage.VerticalResolution);

                drawingContext.PushTransform(t);
                drawingContext.DrawRectangle(sourceBrush, null, new System.Windows.Rect(0, 0, mAnalyzableImage.Width, mAnalyzableImage.Height));
                drawingContext.Close();

                target.Render(drawingVisual);
                
                //immagine analisi
                if (mAnalizeStream != null) mAnalizeStream.Dispose();
                mAnalizeStream = null;

                mAnalizeStream = new MemoryStream();

                //convertitore formato RGB24 per OCR
                FormatConvertedBitmap formatConverter = new FormatConvertedBitmap();
                formatConverter.BeginInit();
                formatConverter.DestinationFormat = PixelFormats.Rgb24;
                formatConverter.Source = target;
                formatConverter.EndInit();

                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(formatConverter));
                encoder.Save(mAnalizeStream);

                mAnalyzableImage = new System.Drawing.Bitmap(mAnalizeStream);
                
                //immagine canvas
                if (mImageStream != null) mImageStream.Dispose();
                mImageStream = null;

                mImageStream = new MemoryStream();

                mAnalyzableImage.Save(mImageStream, System.Drawing.Imaging.ImageFormat.Bmp);
                mImageStream.Seek(0, SeekOrigin.Begin);
                mImage = new BitmapImage();
                mImage.BeginInit();
                mImage.StreamSource = mImageStream;
                mImage.EndInit();

                //aggiorno canvas
                ImageBrush brush = new ImageBrush();

                brush.ImageSource = mImage;

                mImageCanvas.Background = brush;

                mAspectRatio = mImage.Height / mImage.Width;

                mImageCanvas.Width = mImage.PixelWidth;
                mImageCanvas.Height = mImage.PixelHeight;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Errore durante la scrittura dell'immagine...L'applicazione verra' chiusa" + exc.Message);
                Application.Current.Shutdown();
            }
            


        }

        public bool EditingHasSavedImages()
        {
            return mTemporaryFiles.Count > 0;
        }

        public void EditingUndoLastSave()
        {
            //ripristino l'ultimo salvataggio

            string last_save = mTemporaryFiles.Pop();

            if (mAnalizeStream != null) mAnalizeStream.Dispose();
            mAnalizeStream = null;

            if (mImageStream != null) mImageStream.Dispose();
            mImageStream = null;

            mAnalyzableImage = new System.Drawing.Bitmap(last_save);

            mImageStream = new MemoryStream();

            mAnalyzableImage.Save(mImageStream, System.Drawing.Imaging.ImageFormat.Bmp);
            mImageStream.Seek(0, SeekOrigin.Begin);
            mImage = new BitmapImage();
            mImage.BeginInit();
            mImage.StreamSource = mImageStream;
            mImage.EndInit();

            ImageBrush brush = new ImageBrush();

            brush.ImageSource = mImage;

            mImageCanvas.Background = brush;

            mAspectRatio = mImage.Height / mImage.Width;

            mImageCanvas.Width = mImage.PixelWidth;
            mImageCanvas.Height = mImage.PixelHeight;


        }

        public void EditingSelectCorrection(int idx)
        {
            //SelectCorrection(idx);
        }

        private void DeselctAllCorrection ()
        {
            foreach (int key in mCorrections.Keys)
            {
                CorrectionObject elems = mCorrections[key];

                foreach (Line obj in elems.elements)
                {
                    if (elems.type == EditingToolType.Pencil) obj.Stroke = mEditDialogBox.PencilBrush;
                    else obj.Stroke = mEditDialogBox.RubberBrush;
                }
            }
        }

        private void SelectCorrection ( int idx )
        {

            if ( mCorrections.ContainsKey(idx) )
            {
                SolidColorBrush sel_color = new SolidColorBrush(Color.FromArgb(150, 255, 0, 0));

                CorrectionObject to_be_mod = mCorrections[idx];
                CorrectionObject old_corr = null;

                if (mCorrections.ContainsKey(mLastCorrectionSelected)) old_corr = mCorrections[mLastCorrectionSelected];


                foreach ( Line e in to_be_mod.elements )
                {
                    e.Stroke = sel_color;
                }

                if ( old_corr != null )
                {
                    SolidColorBrush pencil = mEditDialogBox.PencilBrush as SolidColorBrush;
                    SolidColorBrush rubber = mEditDialogBox.RubberBrush as SolidColorBrush;

                    if ( old_corr.type == EditingToolType.Pencil )
                    {
                        foreach ( Line e in old_corr.elements )
                        {
                            e.Stroke = pencil;
                        }
                    }
                    else
                    {
                        foreach (Line e in old_corr.elements)
                        {
                            e.Stroke = rubber;
                        }
                    }
                }

                mLastCorrectionSelected = idx;
            }
        }

        private void ClearSelectionAreas()
        {
            foreach ( SelectionArea area in mDataOperazioneAreas )
            {
                area.ClearRecognizedArea();
                area.RemoveFromCanvas();
            }

            foreach ( SelectionArea area in mDataValutaAreas )
            {
                area.ClearRecognizedArea();
                area.RemoveFromCanvas();
            }

            foreach ( SelectionArea area in mDareAreaAreas )
            {
                area.ClearRecognizedArea();
                area.RemoveFromCanvas();
            }

            foreach ( SelectionArea area in mAvereAreaAreas )
            {
                area.ClearRecognizedArea();
                area.RemoveFromCanvas();
            }

            foreach ( SelectionArea area in mDescrizioneAreas )
            {
                area.ClearRecognizedArea();
                area.RemoveFromCanvas();
            }

            mDataOperazioneAreas.Clear();
            mDataValutaAreas.Clear();
            mDareAreaAreas.Clear();
            mAvereAreaAreas.Clear();
            mDescrizioneAreas.Clear();

        }


        public bool SetImagePath( string img_path )
        {

            if (mReady)
            {
                MessageBoxResult r1 = MessageBox.Show("Desideri passare ad una nuova immagine?", "Conferma", MessageBoxButton.YesNo);

                if (r1 == MessageBoxResult.No)
                {
                    return false;
                }
            }


            //pulizia
            if (mImage != null)
            {
                if (mImage.StreamSource != null) mImage.StreamSource.Dispose();
            }

            if (mAnalyzableImage != null)
            {
                mAnalyzableImage.Dispose();
            }

            mImageCanvas.Background = null;
            //pulisco tutti i figli e svuoto le selection areas
            ClearSelectionAreas();



            /*mDataOperazioneArea.ClearRecognizedArea();
            mDataValutaArea.ClearRecognizedArea();
            mDareArea.ClearRecognizedArea();
            mAvereArea.ClearRecognizedArea();
            mDescrizioneArea.ClearRecognizedArea();*/

            //rimuovo tutte le correzioni dal canvas
            EditingUndoCorrection(-1);

            //resetto il pannello di editing
            mEditDialogBox.RemoveAllCorrection();
            mCorrections.Clear();
            if (mElementToDraw != null) mElementToDraw.Clear();//////////////////////////////
            mElementToDraw = null;

            mEditDialogBox.ForceDeactive();
            mImageCanvas.Cursor = Cursors.Arrow;

            if ( mAnalizeStream != null ) mAnalizeStream.Dispose();
            mAnalizeStream = null;
            if (mImageStream != null) mImageStream.Dispose();
            mImageStream = null;

            //////
            mImagePath = img_path;

            if ( System.IO.Path.GetExtension(img_path).Equals(".pdf") )
            {
                //pdf
                GhostscriptVersionInfo _lastInstalledVersion = null;
                GhostscriptRasterizer _rasterizer = null;

                _lastInstalledVersion =
                GhostscriptVersionInfo.GetLastInstalledVersion(
                        GhostscriptLicense.GPL | GhostscriptLicense.AFPL,
                        GhostscriptLicense.GPL);

                _rasterizer = new GhostscriptRasterizer();

                _rasterizer.Open(img_path, _lastInstalledVersion, false);

                PDFPageSelector pageSel = new PDFPageSelector();
                pageSel.PageManager = this;
                pageSel.NumPages = _rasterizer.PageCount;

                pageSel.ShowDialog();

                //finestra chiusa
                int selected_page = pageSel.SelectedPage;

                System.Drawing.Image tmp_img = _rasterizer.GetPage(400, 400, selected_page);

                mAnalizeStream = new MemoryStream();

                tmp_img.Save(mAnalizeStream, System.Drawing.Imaging.ImageFormat.Bmp);

                System.Drawing.Bitmap orig = new System.Drawing.Bitmap(mAnalizeStream);

                MessageBoxResult r2 = MessageBox.Show("Desideri applicare la pulitura (può richiedere tempo)?", "Conferma", MessageBoxButton.YesNo);

                if ( r2 == MessageBoxResult.Yes )
                {
                    WaitDialogBox dbox = new WaitDialogBox();
                    dbox.Owner = (Window)this.Parent;
                    dbox.ShowDialog();

                    mAnalyzableImage = orig.FilterImage(3, 0, true, false);

                    dbox.Close();

                    mAnalizeStream.Dispose();
                }
                else
                {
                    //senza filtro
                    mAnalyzableImage = new System.Drawing.Bitmap(mAnalizeStream);
                }

                //senza filtro
                mAnalyzableImage = new System.Drawing.Bitmap(mAnalizeStream);

                //creo immagine anteprima
                mImageStream = new MemoryStream();

                mAnalyzableImage.Save(mImageStream, System.Drawing.Imaging.ImageFormat.Bmp);
                mImageStream.Seek(0, SeekOrigin.Begin);
                mImage = new BitmapImage();
                mImage.BeginInit();
                mImage.StreamSource = mImageStream;
                mImage.EndInit();

                orig.Dispose();
                tmp_img.Dispose();

                //mAnalyzableImage = Pix.LoadFromFile(mImagePath);
                //mAnalyzableImage = new System.Drawing.Bitmap(ms);

                _rasterizer.Close();
                _rasterizer.Dispose();
            }
            else
            {
                //mImage = new BitmapImage(new Uri(mImagePath));

                //mAnalyzableImage = Pix.LoadFromFile(mImagePath);
                //mAnalyzableImage = new System.Drawing.Bitmap(mImagePath);

                mAnalyzableImage = new System.Drawing.Bitmap(mImagePath);
                
                MessageBoxResult r2 = MessageBox.Show("Desideri applicare la pulitura (può richiedere tempo)?", "Conferma", MessageBoxButton.YesNo);

                if (r2 == MessageBoxResult.Yes)
                {
                    WaitDialogBox dbox = new WaitDialogBox();
                    dbox.Owner = (Window)this.Parent;
                    dbox.Show();


                    mAnalyzableImage = mAnalyzableImage.FilterImage(3, 0, true, false);

                    dbox.Close();
                    
                    //orig.Dispose();

                }

                mImageStream = new MemoryStream();

                mAnalyzableImage.Save(mImageStream, System.Drawing.Imaging.ImageFormat.Bmp);
                mImageStream.Seek(0, SeekOrigin.Begin);
                mImage = new BitmapImage();
                mImage.BeginInit();
                mImage.StreamSource = mImageStream;
                mImage.EndInit();

                
            }

            ImageBrush brush = new ImageBrush();

            brush.ImageSource = mImage;

            mImageCanvas.Background = brush;

            mAspectRatio = mImage.Height / mImage.Width;

            mImageCanvas.Width = mImage.PixelWidth;
            mImageCanvas.Height = mImage.PixelHeight;

            //mAnalyzableImage = new System.Drawing.Bitmap();

            mReady = true;

            return true;
        }

   

        private void ScannerMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Editing_Click(object sender, RoutedEventArgs e)
        {
            mEditDialogBox.Show();
        }


        private void Analize_Click(object sender, RoutedEventArgs e)
        {
            //avvio l'analisi di tutte le aree attive
            WaitDialogBox dialog = new WaitDialogBox();
            dialog.Show();

            foreach ( SelectionArea area in mDataOperazioneAreas )
            {
                AnalizeArea(area);
            }

            foreach (SelectionArea area in mDataValutaAreas)
            {
                AnalizeArea(area);
            }

            foreach (SelectionArea area in mDareAreaAreas)
            {
                AnalizeArea(area);
            }

            foreach (SelectionArea area in mAvereAreaAreas)
            {
                AnalizeArea(area);
            }

            foreach (SelectionArea area in mDescrizioneAreas)
            {
                AnalizeArea(area);
            }

            dialog.Close();
        }


        private bool CreateExcelWorksheet(FileInfo fl, string sheetname)
        {
            mFileInfo = null;

            mFileInfo = fl;


            if (mExcelFile != null) mExcelFile.Dispose();
            mExcelFile = null;

            do
            {
                try
                {
                    mExcelFile = new ExcelPackage(mFileInfo);
                }
                catch( Exception excp )
                {
                    MessageBoxResult res = MessageBox.Show("File Aperto! Chiudere il file e riprovare", "Conferma", MessageBoxButton.YesNo);

                    if (res == MessageBoxResult.Yes)
                    {
                        continue;
                    }
                    else return false;
                }

                //se non becco l'eccezione esco
                break;
            }
            while (true);
        
            
            
            
            mLastRowInserted = 3;

            //mWorksheetName = "Pratica-";
            //mWorksheetName += DateTime.UtcNow.ToString();

            mWorksheetName = sheetname;

            try
            {
                mExcelActiveWorksheet = mExcelFile.Workbook.Worksheets.Add(mWorksheetName);
            }
            catch ( Exception exc )
            {
                //gia esistente, creo uno nuovo

                string pratica = "Pratica-";
                //pratica += DateTime.UtcNow.ToLongTimeString().Replace(":", string.Empty);
                pratica += Guid.NewGuid().ToString().Substring(0, 6);

                mWorksheetName = pratica;
                mExcelActiveWorksheet = mExcelFile.Workbook.Worksheets.Add(mWorksheetName);

            }

            //configuro le colonne e la prima riga
            //mExcelActiveWorksheet.Cells.AutoFitColumns();
            ConfigureWorksheet();

            return true;
            
        }

        private void ConfigureWorksheet ()
        {
            mExcelActiveWorksheet.Cells["A1"].Value = "Data Operazione";
            mExcelActiveWorksheet.Column(1).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            mExcelActiveWorksheet.Column(1).Width = 20;

            mExcelActiveWorksheet.Cells["B1"].Value = "Data Valuta";
            mExcelActiveWorksheet.Column(2).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            mExcelActiveWorksheet.Column(2).Width = 20;

            mExcelActiveWorksheet.Cells["C1"].Value = "Dare";
            mExcelActiveWorksheet.Column(3).Style.Numberformat.Format = @"_(""€""* #,##0.00_);_(""€""* \(#,##0.00\);_(""€""* ""-""??_);_(@_)";
            mExcelActiveWorksheet.Column(3).Style.Font.Color.SetColor(System.Drawing.Color.Red);
            mExcelActiveWorksheet.Column(3).Width = 20;

            mExcelActiveWorksheet.Cells["D1"].Value = "Avere";
            mExcelActiveWorksheet.Column(4).Style.Numberformat.Format = @"_(""€""* #,##0.00_);_(""€""* \(#,##0.00\);_(""€""* ""-""??_);_(@_)";
            mExcelActiveWorksheet.Column(4).Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 128, 64));
            mExcelActiveWorksheet.Column(4).Width = 20;

            mExcelActiveWorksheet.Cells["E1"].Value = "Saldo";
            mExcelActiveWorksheet.Column(5).Style.Numberformat.Format = @"_(""€""* #,##0.00_);_(""€""* \(#,##0.00\);_(""€""* ""-""??_);_(@_)";
            mExcelActiveWorksheet.Column(5).Style.Font.Color.SetColor(System.Drawing.Color.Black);
            mExcelActiveWorksheet.Column(5).Width = 20;

            mExcelActiveWorksheet.Cells["F1"].Value = "Descrizione";
            mExcelActiveWorksheet.Column(6).Width = 50;

            mExcelActiveWorksheet.Cells["A1:F1"].Style.Font.Bold = true;

            //inserisco il saldo iniziale
            decimal zero;
            decimal.TryParse("0", out zero);
            mExcelActiveWorksheet.Cells["E2"].Value = zero;

        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            //verifica se ci sono dati da esportare
            //verifico solo data operazione e valuta
            if (mDataOperazioneAreas.Count() <= 0 || mDataValutaAreas.Count() <= 0)
            {
                MessageBox.Show("Nessun dato elaborato da salvare (Verificare Data Operazione/Valuta)!");

                return;
            }

            bool create_excel = true;

            if (mExcelFile != null)
            {
                MessageBoxResult res = MessageBox.Show("Desideri usare il file Excel corrente?", "Conferma", MessageBoxButton.YesNo);

                if (res == MessageBoxResult.Yes)
                {
                    create_excel = false;
                }
            }

            if (create_excel)
            {
                //creo il file excel
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.DefaultExt = ".xlsx";
                dlg.Filter = "Excel Files (*.xlsx)|*.xlsx";
                dlg.CheckPathExists = false;
                dlg.CheckFileExists = false;

                Nullable<bool> result = dlg.ShowDialog();

                if (result == false) return;

                string pratica = "Pratica-";
                pratica += DateTime.UtcNow.ToLongTimeString().Replace(":", string.Empty);

                FileInfo f = new FileInfo(dlg.FileName);

                bool ret = CreateExcelWorksheet(f, pratica);

                if ( !ret )
                {
                    MessageBox.Show("Esportazione abortita!");
                    return;
                }
            }
            else
            {
                //carico esistente
                if (mExcelFile != null) mExcelFile.Dispose();
                mExcelFile = null;

                do
                {
                    try
                    {
                        mExcelFile = new ExcelPackage(mFileInfo);
                    }
                    catch (Exception excp)
                    {
                        MessageBoxResult res = MessageBox.Show("File Aperto! Chiudere il file e riprovare", "Conferma", MessageBoxButton.YesNo);

                        if (res == MessageBoxResult.Yes)
                        {
                            continue;
                        }
                        else
                        {
                            MessageBox.Show("Esportazione Abortita!");
                            return;
                        }
                    }

                    //se non becco l'eccezione esco
                    break;
                }
                while (true);

                int idx = 1;
                int n_w = mExcelFile.Workbook.Worksheets.Count;

                do
                {
                    mExcelActiveWorksheet = mExcelFile.Workbook.Worksheets[idx++];

                    if ( mExcelActiveWorksheet.Name.Equals(mWorksheetName) )
                    {
                        break;
                    }

                }
                while (true);
                
                if ( mExcelActiveWorksheet == null )
                {
                    //creo nuovo foglio
                    string pratica = "Pratica-";
                    pratica += Guid.NewGuid().ToString().Substring(0,6);

                    //CreateExcelWorksheet(mFileInfo, pratica);
                    mExcelActiveWorksheet = mExcelFile.Workbook.Worksheets.Add(pratica);

                    ConfigureWorksheet();

                    mWorksheetName = pratica;
                }
            }

    
            //ho la griglia dei risultati, scrivo su excel
            List<OperationEntry> entries = CreateGrid();

            int num_entries = entries.Count;

            for ( int k = 0 ; k < num_entries ; k++ )
            {
                RecognizedArea op = entries[k].DataOperazione;
                RecognizedArea val = entries[k].DataValuta;
                RecognizedArea dare = entries[k].Dare;
                RecognizedArea avere = entries[k].Avere;
                RecognizedArea desc = entries[k].Descrizione;

                //elaboro i dati
                string data_op = op != null ? op.RecognizedData : string.Empty;
                string data_val = val != null ? val.RecognizedData : string.Empty;
                
                decimal vd, va;

                if (dare != null)
                {
                    decimal.TryParse(dare.RecognizedData, out vd);
                }
                else
                {
                    decimal.TryParse("0", out vd);
                    mExcelActiveWorksheet.Cells[mLastRowInserted + k, 3].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }

                if (avere != null)
                {
                    decimal.TryParse(avere.RecognizedData, out va);
                }
                else
                {
                    decimal.TryParse("0", out va);
                    mExcelActiveWorksheet.Cells[mLastRowInserted + k, 4].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }

                mExcelActiveWorksheet.Cells[mLastRowInserted + k, 1].Value = data_op;
                mExcelActiveWorksheet.Cells[mLastRowInserted + k, 2].Value = data_val;
                mExcelActiveWorksheet.Cells[mLastRowInserted + k, 3].Value = vd;
                mExcelActiveWorksheet.Cells[mLastRowInserted + k, 4].Value = va;

                //saldo
                string formulasum = "=SUM(E" + (mLastRowInserted + k-1) + "+D" + (mLastRowInserted + k) + "-C" + (mLastRowInserted + k) + ")";
                mExcelActiveWorksheet.Cells[mLastRowInserted + k, 5].Formula = formulasum;

                if ( desc != null ) mExcelActiveWorksheet.Cells[mLastRowInserted + k, 6].Value = desc.RecognizedData;
            }

            mLastRowInserted += num_entries;

            mExcelFile.Save();
        }

        private void old_Export_Click(object sender, RoutedEventArgs e)
        {
            //verifica se ci sono dati da esportare
            //verifico solo data operazione e valuta
            if ( mDataOperazioneAreas.Count() <= 0 || mDataValutaAreas.Count() <= 0 )
            {
                MessageBox.Show("Nessun dato elaborato da salvare (Verificare Data Operazione/Valuta)!");

                return;
            }

            bool create_excel = true;

            if ( mExcelFile != null )
            {
                MessageBoxResult res = MessageBox.Show("Desideri usare il file Excel corrente?", "Conferma", MessageBoxButton.YesNo);

                if ( res == MessageBoxResult.Yes )
                {
                    create_excel = false;
                }
            }
           
            if ( create_excel )
            {
                //creo il file excel
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.DefaultExt = ".xlsx";
                dlg.Filter = "Excel Files (*.xlsx)|*.xlsx";
                dlg.CheckPathExists = false;
                dlg.CheckFileExists = false;

                Nullable<bool> result = dlg.ShowDialog();

                if (result == false) return;
    
                
                FileInfo fileinfo = new FileInfo(dlg.FileName);

                if (mExcelFile != null) mExcelFile.Dispose();
                mExcelFile = null;

                mExcelFile = new ExcelPackage(fileinfo);
                mLastRowInserted = 3;
                
                string title = "Pratica-";
                title += DateTime.UtcNow.ToString();

                mExcelActiveWorksheet = mExcelFile.Workbook.Worksheets.Add(title);
                
                //configuro le colonne e la prima riga
                mExcelActiveWorksheet.Cells.AutoFitColumns();

                mExcelActiveWorksheet.Cells["A1"].Value = "Data Operazione";
                mExcelActiveWorksheet.Column(1).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

                mExcelActiveWorksheet.Cells["B1"].Value = "Data Valuta";
                mExcelActiveWorksheet.Column(2).Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

                mExcelActiveWorksheet.Cells["C1"].Value = "Dare";
                mExcelActiveWorksheet.Column(3).Style.Numberformat.Format = @"_(""€""* #,##0.00_);_(""€""* \(#,##0.00\);_(""€""* ""-""??_);_(@_)";
                mExcelActiveWorksheet.Column(3).Style.Font.Color.SetColor(System.Drawing.Color.Red);

                mExcelActiveWorksheet.Cells["D1"].Value = "Avere";
                mExcelActiveWorksheet.Column(4).Style.Numberformat.Format = @"_(""€""* #,##0.00_);_(""€""* \(#,##0.00\);_(""€""* ""-""??_);_(@_)";
                mExcelActiveWorksheet.Column(4).Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0,128,64));

                mExcelActiveWorksheet.Cells["E1"].Value = "Descrizione";

                mExcelActiveWorksheet.Cells["A1:E1"].Style.Font.Bold = true;
            }

            //a questo punto ho un file excel su cui salvare
            //

            /*for (int k = 0; k < n; k++ )
            {
                SelectionArea p = mDataOperazioneAreas[k];
                List<RecognizedArea> p_data = p.GetAreas();

                str += "Area " + k;

                if (p_data.Count > 0) str += ": " + p_data[0].RecognizedData + " ; " + p_data[0].AreaRect.Y1 + "\r\n";
                else str += "empty\r\n";
            }*/
            
            //ordino le liste

            OrderSelectionAreaByPosition(mDataOperazioneAreas);
            OrderSelectionAreaByPosition(mDataValutaAreas);
            OrderSelectionAreaByPosition(mDareAreaAreas);
            OrderSelectionAreaByPosition(mAvereAreaAreas);
            OrderSelectionAreaByPosition(mDescrizioneAreas);

            //ora ho le liste ordinate per Y, posso scrivere su file excel
            //rettifico su un unica lista tutti i campi

            List<RecognizedArea> operWrite = new List<RecognizedArea>();
            List<RecognizedArea> valWrite = new List<RecognizedArea>();
            List<RecognizedArea> dareWrite = new List<RecognizedArea>();
            List<RecognizedArea> avereWrite = new List<RecognizedArea>();
            List<RecognizedArea> descWrite = new List<RecognizedArea>();

            int num_op = MergeResult(mDataOperazioneAreas, operWrite);
            int num_val = MergeResult(mDataValutaAreas, valWrite);
            int num_dare = MergeResult(mDareAreaAreas, dareWrite);
            int num_avere = MergeResult(mAvereAreaAreas, avereWrite);
            int num_desc = MergeResult(mDescrizioneAreas, descWrite);
            
            //ora scrivo su excel data operazione & valuta
            
            int ov_row = 0;
            for ( int k = 0 ; k < (num_op >= num_val ? num_op : num_val)  ; k++ )
            {
                string data_op = string.Empty;
                string data_val = string.Empty;

                try 
                {
                    data_op = operWrite[k].RecognizedData;
                    data_val = valWrite[k].RecognizedData;
                }
                catch(Exception ex)
                {
                    //finito
                }

                mExcelActiveWorksheet.Cells[mLastRowInserted + ov_row, 1].Value = data_op;
                mExcelActiveWorksheet.Cells[mLastRowInserted + ov_row, 2].Value = data_val;

                ov_row++;
            }

            //dare / avere
            int actual_dare = 0, actual_avere = 0;
            int ad_row = 0;

            do
            {
                RecognizedArea d = null, a = null;

                if (actual_dare < num_dare && actual_avere < num_avere)
                {
                    d = dareWrite[actual_dare];
                    a = avereWrite[actual_avere];
                }
                else if (actual_avere < num_avere)
                {
                    d = null;
                    a = avereWrite[actual_avere];
                }
                else if (actual_dare < num_dare)
                {
                    a = null;
                    d = dareWrite[actual_dare];
                }
                else break;

                if (((d != null && a != null) && (d.AreaRect.Y < a.AreaRect.Y)) || (d != null && a == null))
                {
                    //inserisco DARE

                    actual_dare++;

                    decimal v1,v2;
                    decimal.TryParse(d.RecognizedData, out v1);

                    mExcelActiveWorksheet.Cells[mLastRowInserted + ad_row, 3].Value = v1;

                    decimal.TryParse("0", out v2);

                    mExcelActiveWorksheet.Cells[mLastRowInserted + ad_row, 4].Value = v2;
                    mExcelActiveWorksheet.Cells[mLastRowInserted + ad_row, 4].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }
                else if ((d != null && a != null) && (a.AreaRect.Y < d.AreaRect.Y) || (d == null && a != null))
                {
                    //inserisco avere

                    actual_avere++;

                    decimal v1,v2;
                    decimal.TryParse(a.RecognizedData, out v1);

                    mExcelActiveWorksheet.Cells[mLastRowInserted + ad_row, 4].Value = v1;

                    decimal.TryParse("0", out v2);

                    mExcelActiveWorksheet.Cells[mLastRowInserted + ad_row, 3].Value = v2;
                    mExcelActiveWorksheet.Cells[mLastRowInserted + ad_row, 3].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }

                ad_row++;
            }
            while (true);

            //descrizione
            int des_row = 0;
            for (int k = 0; k < num_desc; k++)
            {
                string decriz = descWrite[k].RecognizedData != null ? descWrite[k].RecognizedData : string.Empty;

                mExcelActiveWorksheet.Cells[mLastRowInserted + des_row, 5].Value = decriz;

                des_row++;
            }

            List<int> rows = new List<int>();
            rows.Add(ad_row);
            rows.Add(ov_row);
            rows.Add(des_row);
            
            int maxrow = rows.Max();
            mLastRowInserted += maxrow;

            mExcelFile.Save();
        }

        private List<OperationEntry> CreateGrid()
        {
            //ordino le liste per posizione 
            OrderSelectionAreaByPosition(mDataOperazioneAreas);
            OrderSelectionAreaByPosition(mDataValutaAreas);
            OrderSelectionAreaByPosition(mDareAreaAreas);
            OrderSelectionAreaByPosition(mAvereAreaAreas);
            OrderSelectionAreaByPosition(mDescrizioneAreas);

            //lista aree
            List<RecognizedArea> operaz     = new List<RecognizedArea>();
            List<RecognizedArea> valuta     = new List<RecognizedArea>();
            List<RecognizedArea> dare       = new List<RecognizedArea>();
            List<RecognizedArea> avere      = new List<RecognizedArea>();
            List<RecognizedArea> descriz    = new List<RecognizedArea>();

            //risultati
            MergeResult(mDataOperazioneAreas, operaz);
            MergeResult(mDataValutaAreas, valuta);
            MergeResult(mDareAreaAreas, dare);
            MergeResult(mAvereAreaAreas, avere);
            MergeResult(mDescrizioneAreas, descriz);

            List<OperationEntry> results = new List<OperationEntry>();


            //calcolo della distanza tra righe, stima...uso la colonna data operazione
            double rowDistance = EstimateRowHeight(operaz);


            //per ogni data operazione cerco i corrispondenti
            foreach ( RecognizedArea op in operaz )
            {

                OperationEntry entry = new OperationEntry();

                entry.DataOperazione = op;

                //centro dell'area
                double y_op = 0.0;

                y_op = (op.AreaRect.Y + op.AreaRect.Y) * 0.5;

                //valuta
                foreach (RecognizedArea val in valuta)
                {
                    double y_val = (val.AreaRect.Y + val.AreaRect.Y) * 0.5;

                    if (Math.Abs(y_val - y_op) <= rowDistance)
                    {
                        //ok, in riga

                        entry.DataValuta = val;

                        //rimuovo, velocizzo

                        valuta.Remove(val);

                        break;
                    }
                }

                //dare/avere

                foreach (RecognizedArea d in dare)
                {
                    double y_d = (d.AreaRect.Y + d.AreaRect.Y) * 0.5;

                    if (Math.Abs(y_d - y_op) <= rowDistance)
                    {
                        //ok, in riga
                        entry.Dare = d;

                        //rimuovo, velocizzo

                        dare.Remove(d);

                        break;
                    }
                }

                foreach (RecognizedArea a in avere)
                {
                    double y_a = (a.AreaRect.Y + a.AreaRect.Y) * 0.5;

                    if (Math.Abs(y_a - y_op) <= rowDistance)
                    {
                        //ok, in riga

                        entry.Avere = a;

                        //rimuovo, velocizzo

                        dare.Remove(a);

                        break;
                    }
                }
                

                //descrizione

                foreach (RecognizedArea de in descriz)
                {
                    double y_d = (de.AreaRect.Y + de.AreaRect.Y) * 0.5;

                    if (Math.Abs(y_d - y_op) <= rowDistance)
                    {
                        //ok, in riga

                        entry.Descrizione = de;

                        //rimuovo, velocizzo

                        descriz.Remove(de);

                        break;
                    }
                }

                //aggiungo
                results.Add(entry);
            }

            return results;
        }

        /*private double EstimateRowDistance ( List<RecognizedArea> area )
        {
            double rowdist = 20.0; //valore di default

            double medium = 0.0;

            if (area == null || area.Count == 0) return rowdist;
            //devo ricavare l'altezza media

            foreach ( RecognizedArea a in area )
            {
                medium += a.AreaRect.Height;
            }

            medium /= area.Count;

            rowdist = medium;

            return rowdist;
        }*/

        private double EstimateRowHeight(List<RecognizedArea> areas)
        {
            //stimo la distanza tra righe
            //prima estrapolo la distanza tra righe
            //hp: le aree sono gia in ordine

            double dist = 2000;
            
            for ( int k = 1; k < areas.Count-1; k++ )
            {
                double act_dist = areas[k].AreaRect.Y - areas[k - 1].AreaRect.Y;

                if (act_dist < dist) dist = act_dist;
            }

            return dist;
            
        }

        private int MergeResult (List<SelectionArea> areas, List<RecognizedArea> list)
        {
            
            if ( areas != null )
            {
                int num = areas.Count;

                for ( int k = 0 ; k < num ; k++ )
                {
                    SelectionArea elem = areas[k];

                    if ( elem.HasRecognizedData )
                    {
                        List<RecognizedArea> results = elem.GetAreas();

                        int nres = results.Count;

                        for ( int p = 0 ; p < nres ; p++ )
                        {
                            if ( results[p].Active ) list.Add(results[p]);
                        }
                    }
                }
            }

            return list.Count;
        }

        private void OrderSelectionAreaByPosition ( List<SelectionArea> list )
        {
            if ( list != null && list.Count > 0 )
            {
                list.Sort(new SelectionAreaPositionComparer());
            }
        }
        
        private int GetNextValidArea(List<SelectionArea> list, int start_idx, out SelectionArea area)
        {
            if ( list == null || list.Count <= 0 )
            {
                area = null;
                return -1;
            }

            //scorro sulla lista
            
            int idx = start_idx;

            try
            {
                do
                {
                    area = list[idx];
                    
                    if (area.HasRecognizedData) break;

                    idx++;
                }
                while (true);
            }
            catch (Exception exc)
            {
                //terminato
                area = null;
                return -1;
            }

            return idx;
        }

        private void OCRWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            e.Cancel = true;

            mEditDialogBox.Hide();
            this.Hide();
        }

        /*private void mImageCanvas_MouseWheel ( object sender, MouseWheelEventArgs args )
        {
            
        }*/

        private void mImageCanvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            RefreshOptionsMenu(e);
        }

        private void RefreshOptionsMenu (ContextMenuEventArgs e)
        {
            if (mEditingActive)
            {
                mOptionsMenu.Items.Clear();
                return;
            }

            mOptionsMenu.Items.Clear();

            string[] titles = new string[5];

            titles[0] = "Area DATA OPERAZIONE";
            titles[1] = "Area DATA VALUTA";
            titles[2] = "Area DARE";
            titles[3] = "Area AVERE";
            titles[4] = "Area DESCRIZIONE";

            /*SelectionArea[] areas = new SelectionArea[5];

            areas[0] = mDataOperazioneArea;
            areas[1] = mDataValutaArea;
            areas[2] = mDareArea;
            areas[3] = mAvereArea;
            areas[4] = mDescrizioneArea;*/

            Separator sep = null;
            
            MenuItem mTitle = new MenuItem();
            mTitle.Header = "MENU OPZIONI";

            sep = new Separator();

            mOptionsMenu.Items.Add(mTitle);
            mOptionsMenu.Items.Add(sep);

            for ( int areaIdx = 0 ; areaIdx < 5 ; areaIdx++ )
            {
                int k = areaIdx;

                //SelectionArea area = areas[k];
                String title = titles[k];

                MenuItem mitem = new MenuItem();

                mitem.Header = title;

                MenuItem mAddArea = new MenuItem();

                //intercetto se il click \'e su un'area

                /*if ( area.AreaVisibility() )
                {
                    //area visibile
                    mShowHideItem.Header = "Elimina Area";
                    mShowHideItem.Tag = area;
                    mShowHideItem.Click += ShowHideAreaMenu_Click;

                    MenuItem mAnalizeArea = new MenuItem();
                    mAnalizeArea.Header = "Analizza Area con OCR";
                    mAnalizeArea.Tag = area;
                    mAnalizeArea.Click += AnalizeAreaMenu_Click;

                    mitem.Items.Add(mAnalizeArea);

                    if ( area.HasRecognizedData )
                    {
                        MenuItem mShowReconAreas = new MenuItem();

                        if ( area.RecognizedAreaVisible )
                        {
                            mShowReconAreas.Header = "Nascondi Risultati OCR";
                        }
                        else
                        {
                            mShowReconAreas.Header = "Mostra Risultati OCR";
                        }

                        mShowReconAreas.Tag = area;
                        mShowReconAreas.Click += ShowHideRecognizedArea_Click;

                        mitem.Items.Add(mShowReconAreas);
                    }
                }
                else
                {
                    //area invisibile
                    mShowHideItem.Header = "Inserisci Area";
                    
                    //SelectionArea area = new SelectionArea(e.CursorTop, e.CursorLeft, 100, 100, (SelectionAreaType)areaIdx, this);
                    //area.AddToCanvas(mImageCanvas);

                    mShowHideItem.Tag = area;
                    mShowHideItem.Click += ShowHideAreaMenu_Click;
                }*/

                mAddArea.Header = "Inserisci Area";
                SelectionAreaType area_type = (SelectionAreaType)areaIdx;

                mAddArea.Tag = area_type;
                mAddArea.Click += mAddAreaMenu_Click;

                mitem.Items.Add(mAddArea);

                //mOptionsMenu.Items.Add(mitem);

                List<SelectionArea> list = null;
                
                switch(area_type)
                {
                    case SelectionAreaType.DataOperazioneArea:
                        list = mDataOperazioneAreas;
                        break;
                    case SelectionAreaType.DataValutaArea:
                        list = mDataValutaAreas;
                        break;
                    case SelectionAreaType.DareArea:
                        list = mDareAreaAreas;
                        break;
                    case SelectionAreaType.AvereArea:
                        list = mAvereAreaAreas;
                        break;
                    case SelectionAreaType.DescrizioneArea:
                        list = mDescrizioneAreas;
                        break;
                }

                if ( list.Count() > 0 )
                {
                    sep = new Separator();

                    mitem.Items.Add(sep);

                    MenuItem showHideMenuItem = new MenuItem();

                    showHideMenuItem.Header = "Mostra Aree";

                    showHideMenuItem.Click += showMenuItem_Click;

                    showHideMenuItem.Tag = list;

                    mitem.Items.Add(showHideMenuItem);
                }


                mOptionsMenu.Items.Add(mitem);
            }
            
        }

        void showMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //mostro/nascondo
            MenuItem item = sender as MenuItem;

            List<SelectionArea> areas = item.Tag as List<SelectionArea>;

            ShowSelectionAreas(areas);
        }

        public void RequireRemoveArea(SelectionArea area)
        {
            if ( area != null )
            {
                if (mDataOperazioneAreas.Contains(area)) mDataOperazioneAreas.Remove(area);
                if (mDataValutaAreas.Contains(area)) mDataValutaAreas.Remove(area);
                if (mDareAreaAreas.Contains(area)) mDareAreaAreas.Remove(area);
                if (mAvereAreaAreas.Contains(area)) mAvereAreaAreas.Remove(area);
                if (mDescrizioneAreas.Contains(area)) mDescrizioneAreas.Remove(area);
            }
        }

        private void HideAllSelectionAreas()
        {
            HideSelectionAreas(mDataOperazioneAreas);
            HideSelectionAreas(mDataValutaAreas);
            HideSelectionAreas(mDareAreaAreas);
            HideSelectionAreas(mAvereAreaAreas);
            HideSelectionAreas(mDescrizioneAreas);
        }

        private void ToggleSelectionAreas(List<SelectionArea> list)
        {
            if (list == null) return;

            foreach (SelectionArea area in list)
            {
                if (!area.AreaVisibility())
                {
                    area.ShowArea();
                }
            }
        }
        
        private void HideSelectionAreas(List<SelectionArea> list)
        {
            if (list == null) return;

            foreach (SelectionArea area in list)
            {
                area.HideArea();
            }
        }

        private void ShowSelectionAreas(List<SelectionArea> list)
        {
            if (list == null) return;

            foreach (SelectionArea area in list)
            {
                area.ShowArea();
            }
        }

        public void ShowAllSelectionAreas()
        {
            ShowSelectionAreas(mDataOperazioneAreas);
            ShowSelectionAreas(mDataValutaAreas);
            ShowSelectionAreas(mDareAreaAreas);
            ShowSelectionAreas(mAvereAreaAreas);
            ShowSelectionAreas(mDescrizioneAreas);
        }

        public void SelectionArea_MouseEnter(object sender, MouseEventArgs e)
        {
            /*SelectionArea area = null;
            Rectangle r = null;

            Point pt = Mouse.GetPosition(mImageCanvas);

            r = mImageCanvas.InputHitTest(pt) as Rectangle;

            if ( r != null )
            {
                area = r.Tag as SelectionArea;

                if (area != null)
                {
                    area.SetBorderCursor(r);
                }
            }*/

        }
        public void SelectionArea_MouseLeave(object sender, MouseEventArgs e)
        {
            /*Rectangle r = sender as Rectangle;
            SelectionArea area = r.Tag as SelectionArea;

            area.SetBorderCursor(r);*/

            //Point pt = new Point();

            //area.SetMoving(false, pt);
            //area.SetStretching(false, pt);
            //area.ResetTransformPoint();
        }

        public void ShowHideAreaMenu_Click (object sender, EventArgs args)
        {
            /*MenuItem item = sender as MenuItem;

            SelectionAreaType type = (SelectionAreaType)item.Tag;
            
            Point pt = Mouse.GetPosition(mImageCanvas);

            SelectionArea area = new SelectionArea(pt.X, pt.Y, 100, 100, type, this);
            area.AddToCanvas(mImageCanvas);*/


            /*SelectionArea area = item.Tag as SelectionArea;
    
            if ( area.AreaVisibility() )
            {
                area.ClearRecognizedArea();
                area.HideArea();
            }
            else
            {
                //mostro l'area nella posizione corrente
                Point pt = Mouse.GetPosition(mImageCanvas);

                area.RefreshArea(pt.X, pt.Y, 200, 200);
                area.ShowArea();

            }*/

        }

        public void EditingChangeToolType(EditingToolType newType)
        {

        }

        public void EditingChangeToolTickness(EditingToolThickness newTick)
        {

        }

        public void OCRAreaAnalysis (SelectionArea area)
        {
           
            if ( area.AreaVisibility() )
            {
                //analisi OCR

                if ( mAnalyzableImage != null && mOcrEngine != null )
                {

                    area.ClearRecognizedArea();

                    AnalizeArea(area);
                }

            }
        }


        /*private void AnalizeArea ( SelectionArea area )
        {
            if (area == null) return;

            Tesseract.Rect analize_area = area.GetOCRArea();
            
            if ( area.AreaType != SelectionAreaType.DescrizioneArea )
            {
                //per le aree numeriche disattivo tutti i vocabolari e
                //imposto i caratteri riconoscibili
           
                mOcrEngine.SetVariable("tessedit_char_whitelist", "0123456789.,/*"); ///impostare come variabile di configurazione
            }
            else
            {
                mOcrEngine.SetVariable("tessedit_char_whitelist", ""); 

            }


            Tesseract.Page page = mOcrEngine.Process(mAnalyzableImage, analize_area);

            Tesseract.ResultIterator iter = page.GetIterator();

            iter.Begin();

            do
            {

                Tesseract.Rect word_rect;

                iter.TryGetBoundingBox(PageIteratorLevel.TextLine, out word_rect);

                string str_out = iter.GetText(PageIteratorLevel.TextLine);

                if ( area.AreaType != SelectionAreaType.DescrizioneArea )
                {
                    str_out = str_out.Trim().Replace(" ", string.Empty);
                }

                float conf = iter.GetConfidence(PageIteratorLevel.TextLine);

                area.AddRecognizedArea(word_rect, str_out, conf);

            } while (iter.Next(PageIteratorLevel.TextLine));


            iter.Dispose();

            page.Dispose();
        }*/

        private void AnalizeArea(SelectionArea area)
        {
            if (area == null) return;

            System.Drawing.Rectangle analize_area = area.GetOCRArea();

            OcrResult res_area = mOcrEngine.Read(mAnalyzableImage, analize_area);

            if ( res_area != null && res_area.Pages[0].Words != null && res_area.Pages[0].Words.Count() > 0 )
            {
                IEnumerable<OcrResult.OcrWord> words = res_area.Pages[0].Words;

                foreach (OcrResult.OcrWord word in words)
                {
                    
                    //modifico il rettangolo di ricerca 
                    System.Drawing.Rectangle word_rect = new System.Drawing.Rectangle();
                    word_rect.X = word.Location.Location.X + analize_area.X;
                    word_rect.Y = word.Location.Location.Y + analize_area.Y - word.Location.Height/2;
                    word_rect.Width = word.Location.Width;
                    word_rect.Height = word.Location.Height;
                   

                    area.AddRecognizedArea(word_rect, word.Text, (float)word.Confidence);

                }
            }

        }




        private void mAddAreaMenu_Click(object sender, EventArgs args)
        {
            /*MenuItem item = sender as MenuItem;

            SelectionArea area = item.Tag as SelectionArea;

            if (area.RecognizedAreaVisible)
            {
                area.HideRecognizedAreas();
            }
            else
            {
                area.ShowRecognizedAreas();
            }*/

            MenuItem item = sender as MenuItem;
            
            SelectionAreaType type = (SelectionAreaType)item.Tag;

            //Point pt = Mouse.GetPosition(mImageCanvas);
            Point pt = new Point(0, 0);

            SelectionArea area = new SelectionArea(pt.X, pt.Y, 100, 100, type, this);
            area.AddToCanvas(mImageCanvas);
            area.ShowArea();

            List<SelectionArea> list = null;

            switch (type)
            {
                case SelectionAreaType.DataOperazioneArea:
                    list = mDataOperazioneAreas;
                    break;
                case SelectionAreaType.DataValutaArea:
                    list = mDataValutaAreas;
                    break;
                case SelectionAreaType.DareArea:
                    list = mDareAreaAreas;
                    break;
                case SelectionAreaType.AvereArea:
                    list = mAvereAreaAreas;
                    break;
                case SelectionAreaType.DescrizioneArea:
                    list = mDescrizioneAreas;
                    break;
            }

            list.Add(area);

        }

        public void SelectionArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           /* Rectangle r = sender as Rectangle;
            SelectionArea area = r.Tag as SelectionArea;

            Point pt = e.GetPosition(mImageCanvas);

            if ( area.StretchRectangle(r) )
            {
                //rettangolo di stretching
                area.SetStretching(true, pt);
                area.SetMoving(false, pt);
            }
            else
            {
                area.SetStretching(false, pt);
                area.SetMoving(true, pt);
            }*/

            SelectionArea area = null;
            Rectangle r = null;

            Point pt = Mouse.GetPosition(mImageCanvas);

            r = mImageCanvas.InputHitTest(pt) as Rectangle;

            if (r != null)
            {
                area = r.Tag as SelectionArea;

                if (area != null)
                {
                    mDraggingArea = area;
                    mDraggingRectangle = r;

                    area.SetBorderCursor(r);

                    if (area.StretchRectangle(r))
                    {
                        //rettangolo di stretching
                        area.SetStretching(true, pt);
                        area.SetMoving(false, pt);
                    }
                    else
                    {
                        area.SetStretching(false, pt);
                        area.SetMoving(true, pt);
                    }

                }
            }

        }

        public void SelectionArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

           /* Rectangle r = sender as Rectangle;
            SelectionArea area = r.Tag as SelectionArea;
            Point pt = e.GetPosition(mImageCanvas);

            //chiudo tutte le azioni attive
            area.SetStretching(false, pt);
            area.SetMoving(false, pt);
            area.ResetTransformPoint();*/

            Point pt = Mouse.GetPosition(mImageCanvas);

            /*r = mImageCanvas.InputHitTest(pt) as Rectangle;

            if (r != null)
            {
                area = r.Tag as SelectionArea;

                if (area != null)
                {
                    area.SetBorderCursor(r);

                    area.SetStretching(false, pt);
                    area.SetMoving(false, pt);
                    area.ResetTransformPoint();

                }
            }*/

            if ( mDraggingArea != null )
            {
                mImageCanvas.Cursor = Cursors.Arrow;

                mDraggingArea.SetStretching(false, pt);
                mDraggingArea.SetMoving(false, pt);
                mDraggingArea.ResetTransformPoint();

                mDraggingRectangle = null;
                mDraggingArea = null;
            }
        }

        public void SelectionArea_MouseMove(object sender, MouseEventArgs e)
        {
            /*Rectangle r = sender as Rectangle;
            SelectionArea area = r.Tag as SelectionArea;

            Point pt = e.GetPosition(mImageCanvas);

            if ( area.GetStretching() )
            {
                //sto strechando
                area.StretchArea(r, pt);

            }
            else if ( area.GetMoving() )
            {
                //sto trascinando
                area.MoveArea(pt);

            }*/

            SelectionArea area = null;
            Rectangle r = null;

            Point pt = Mouse.GetPosition(mImageCanvas);
           
            if ( mDraggingArea != null )
            {
                area = mDraggingArea;
                r = mDraggingRectangle;

                area.SetBorderCursor(r);

                if (area.GetStretching())
                {
                    //sto strechando
                    area.StretchArea(r, pt);

                }
                else if (area.GetMoving())
                {
                    //sto trascinando
                    area.MoveArea(pt);

                }

            }
     
        }
    }
}
