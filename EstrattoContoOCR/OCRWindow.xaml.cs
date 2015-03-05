using System;
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

using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;

using Tesseract;

using OfficeOpenXml;

namespace EstrattoContoOCR
{
    /// <summary>
    /// Logica di interazione per OCRWindow.xaml
    /// </summary>
    public partial class OCRWindow : Window, ISelectionAreaDelegate
    {

        private double mAspectRatio;
        private BitmapImage mImage;

        private String mImagePath;
        private ContextMenu mOptionsMenu;

        private SelectionArea mDataOperazioneArea;
        private SelectionArea mDataValutaArea;
        private SelectionArea mDareArea;
        private SelectionArea mAvereArea;
        private SelectionArea mDescrizioneArea;


        private SelectionArea mDraggingArea;
        private Rectangle mDraggingRectangle;
        private TesseractEngine mOcrEngine;
        //private Pix mAnalyzableImage;
        private System.Drawing.Bitmap mAnalyzableImage;

        public OCRWindow()
        {
            InitializeComponent();

            mOptionsMenu = new ContextMenu();
            //RefreshOptionsMenu();

            mImageCanvas.ContextMenu = mOptionsMenu;
            mImageCanvas.ContextMenuOpening += mImageCanvas_ContextMenuOpening;

            mDataOperazioneArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DataOperazioneArea, this);
            mDataValutaArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DataValutaArea, this);
            mDareArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DareArea, this);
            mAvereArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.AvereArea, this);
            mDescrizioneArea = new SelectionArea(50, 50, 100, 100, SelectionAreaType.DescrizioneArea, this);

            mDataValutaArea.AddToCanvas(mImageCanvas);
            mDataOperazioneArea.AddToCanvas(mImageCanvas);
            mDareArea.AddToCanvas(mImageCanvas);
            mAvereArea.AddToCanvas(mImageCanvas);
            mDescrizioneArea.AddToCanvas(mImageCanvas);

            mImageCanvas.MouseDown += SelectionArea_MouseLeftButtonDown;
            mImageCanvas.MouseUp += SelectionArea_MouseLeftButtonUp;
            mImageCanvas.MouseMove += SelectionArea_MouseMove;
            mImageCanvas.MouseEnter += SelectionArea_MouseEnter;
            mImageCanvas.MouseLeave += SelectionArea_MouseLeave;
            mDraggingArea = null;

            ////////TEMPORANEO !!! USARE CON VARIABILE D'AMBIENTE TESSDATA_PREFIX
            mOcrEngine = new TesseractEngine("C:\\Users\\Marco\\Documents\\ocr\\tesseract-ocr\\tessdata", "ita", EngineMode.TesseractOnly);

            mOcrEngine.SetVariable("load_system_dawg", false);
            mOcrEngine.SetVariable("load_freq_dawg", false);
            mOcrEngine.SetVariable("load_punc_dawg", false);
            mOcrEngine.SetVariable("load_number_dawg", false);
            mOcrEngine.SetVariable("load_unambig_dawg", false);
            mOcrEngine.SetVariable("load_bigram_dawg", false);
            mOcrEngine.SetVariable("load_fixed_length_dawgs", false);

            this.Closing += OCRWindow_Closing;
        }

        public void SetImagePath( string img_path )
        {
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

            //svuoto le selection areas

            mDataOperazioneArea.ClearRecognizedArea();
            mDataValutaArea.ClearRecognizedArea();
            mDareArea.ClearRecognizedArea();
            mAvereArea.ClearRecognizedArea();
            mDescrizioneArea.ClearRecognizedArea();

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
                
                System.Drawing.Image tmp_img = _rasterizer.GetPage(100, 100, 1);

                MemoryStream ms_o = new MemoryStream();

                tmp_img.Save(ms_o, System.Drawing.Imaging.ImageFormat.Bmp);

                System.Drawing.Bitmap orig = new System.Drawing.Bitmap(ms_o);

                mAnalyzableImage = orig.MedianFilter(3, 0, true);

                ms_o.Dispose();
                orig.Dispose();
                tmp_img.Dispose();

                //creo immagine anteprima
                MemoryStream ms = new MemoryStream();

                mAnalyzableImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                mImage = new BitmapImage();
                mImage.BeginInit();
                mImage.StreamSource = ms;
                mImage.EndInit();

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

                System.Drawing.Bitmap orig = new System.Drawing.Bitmap(mImagePath);
                
                mAnalyzableImage = orig.MedianFilter(3, 0, true);

                MemoryStream ms = new MemoryStream();

                mAnalyzableImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                mImage = new BitmapImage();
                mImage.BeginInit();
                mImage.StreamSource = ms;
                mImage.EndInit();

                orig.Dispose();
            }

            ImageBrush brush = new ImageBrush();

            brush.ImageSource = mImage;

            mImageCanvas.Background = brush;

            mAspectRatio = mImage.Height / mImage.Width;

            mImageCanvas.Width = mImage.PixelWidth;
            mImageCanvas.Height = mImage.PixelHeight;

            //mAnalyzableImage = new System.Drawing.Bitmap();
        }

        private void ScannerMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Elabora_Click(object sender, RoutedEventArgs e)
        {
            if ( !mDataOperazioneArea.HasRecognizedData &&
                 !mDataValutaArea.HasRecognizedData && 
                 !mDareArea.HasRecognizedData && !mAvereArea.HasRecognizedData )
            {
                MessageBox.Show("Nessun dato elaborato da salvare!");

                return;
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel Files (*.xlsx)|*.xlsx";
            dlg.CheckPathExists = false;
            dlg.CheckFileExists = false;

            Nullable<bool> result = dlg.ShowDialog();

            if ( result == false ) return;

            
            //salvo su un file excel
            FileInfo newFile = new FileInfo(dlg.FileName);

            ExcelPackage pck = new ExcelPackage(newFile);

            string title = "OCR-";
            title += DateTime.UtcNow.ToString();

            ExcelWorksheet ws = pck.Workbook.Worksheets.Add(title);
            
            ws.Cells.AutoFitColumns();

            ws.Cells["A1"].Value = "Data Operazione";
            ws.Cells["B1"].Value = "Data Valuta";
            ws.Cells["C1"].Value = "Dare";
            ws.Cells["D1"].Value = "Avere";
            ws.Cells["E1"].Value = "Descrizione";

            ws.Cells["A1:E1"].Style.Font.Bold = true;

            //scrittura dei valori

            int n_valuta = 0;
            int n_operaz = 0;

            //data operazione
            if ( mDataOperazioneArea.HasRecognizedData )
            {
                List<string> results = mDataOperazioneArea.GetResults();

                int row = 1;
 
                foreach ( string rs in results )
                {
                    ws.Cells[row + 1, 1].Value = rs;
                    row++;
                    n_operaz++;
                }
            }

            //data valuta
            if (mDataValutaArea.HasRecognizedData)
            {
                List<string> results = mDataValutaArea.GetResults();

                int row = 1;

                foreach (string rs in results)
                {
                    ws.Cells[row + 1, 2].Value = rs;
                    row++;
                    n_valuta++;
                }
            }

            //data dare
            /*if (mDareArea.HasRecognizedData)
            {
                List<string> results = mDareArea.GetResults();

                int row = 1;

                foreach (string rs in results)
                {
                    ws.Cells[row + 1, 3].Value = rs;
                    row++;
                }
            }

            //data avere
            if (mAvereArea.HasRecognizedData)
            {
                List<string> results = mAvereArea.GetResults();

                int row = 1;

                foreach (string rs in results)
                {
                    ws.Cells[row + 1, 4].Value = rs;
                    row++;
                }
            }*/

            //DARE / AVERE
            //inserisco in base alle coordinate delle aree
            int actual_dare = 0;
            int actual_avere = 0;
            int ad_row = 2;
            int ad_col = 0;

            int n_dare = mDareArea.GetNumResults();
            int n_avere = mAvereArea.GetNumResults();

            RecognizedArea ad_r = null;
            List<RecognizedArea> dare = mDareArea.GetAreas();
            List<RecognizedArea> avere = mAvereArea.GetAreas();
            RecognizedArea d = null;
            RecognizedArea a = null;

            do
            {
                if (actual_dare < n_dare && actual_avere < n_avere)
                {
                    d = dare[actual_dare];
                    a = avere[actual_avere];
                }
                else if (actual_avere < n_avere)
                {
                    d = null;
                    a = avere[actual_avere];
                }
                else if (actual_dare < n_dare)
                {
                    a = null;
                    d = dare[actual_dare];
                }
                else break;

                if ( ((d != null && a != null) && (d.AreaRect.Y1 < a.AreaRect.Y1)) || (d != null && a == null ) )
                {
                    //inserisco DARE

                    actual_dare++;
                    ad_col = 3;
                    ad_r = d;
                }
                else if ((d != null && a != null) && (a.AreaRect.Y1 < d.AreaRect.Y1) || (d == null && a != null ) )
                {
                    //inserisco avere

                    actual_avere++;
                    ad_col = 4;
                    ad_r = a;

                }
                

                ws.Cells[ad_row, ad_col].Value = ad_r.RecognizedData;

                ad_row++;
            }
            while (true);

            //descrizione
            if (mDescrizioneArea.HasRecognizedData)
            {
                List<string> results = mDescrizioneArea.GetResults();

                int row = 1;

                foreach (string rs in results)
                {
                    ws.Cells[row + 1, 5].Value = rs;
                    row++;
                }
            }

            pck.Save();
        }
          
        private void OCRWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            e.Cancel = true;

            this.Hide();
        }

        /*private void mImageCanvas_MouseWheel ( object sender, MouseWheelEventArgs args )
        {
            
        }*/

        private void mImageCanvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            RefreshOptionsMenu();
        }

        private void RefreshOptionsMenu ()
        {

            mOptionsMenu.Items.Clear();

            string[] titles = new string[5];

            titles[0] = "Area DATA OPERAZIONE";
            titles[1] = "Area DATA VALUTA";
            titles[2] = "Area DARE";
            titles[3] = "Area AVERE";
            titles[4] = "Area DESCRIZIONE";

            SelectionArea[] areas = new SelectionArea[5];

            areas[0] = mDataOperazioneArea;
            areas[1] = mDataValutaArea;
            areas[2] = mDareArea;
            areas[3] = mAvereArea;
            areas[4] = mDescrizioneArea;

            Separator sep = null;
            
            MenuItem mTitle = new MenuItem();
            mTitle.Header = "MENU OPZIONI";

            sep = new Separator();

            mOptionsMenu.Items.Add(mTitle);
            mOptionsMenu.Items.Add(sep);

            for ( int k = 0 ; k < 5 ; k++ )
            {

                SelectionArea area = areas[k];
                String title = titles[k];

                MenuItem mitem = new MenuItem();

                mitem.Header = title;

                MenuItem mShowHideItem = new MenuItem();

                if ( area.AreaVisibility() )
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
                    mShowHideItem.Tag = area;
                    mShowHideItem.Click += ShowHideAreaMenu_Click;
                }


                mitem.Items.Add(mShowHideItem);

                mOptionsMenu.Items.Add(mitem);
            }
            
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
            MenuItem item = sender as MenuItem;

            SelectionArea area = item.Tag as SelectionArea;
    
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

            }

        }


        public void AnalizeAreaMenu_Click (object sender, EventArgs args)
        {
            MenuItem item = sender as MenuItem;

            SelectionArea area = item.Tag as SelectionArea;

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


        private void AnalizeArea ( SelectionArea area )
        {
            if (area == null) return;

            Tesseract.Rect analize_area = area.GetOCRArea();
            
            if ( area != mDescrizioneArea )
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

                float conf = iter.GetConfidence(PageIteratorLevel.TextLine);

                area.AddRecognizedArea(word_rect, str_out, conf);

            } while (iter.Next(PageIteratorLevel.TextLine));


            iter.Dispose();

            page.Dispose();
        }

        private void  ShowHideRecognizedArea_Click (object sender, EventArgs args)
        {
            MenuItem item = sender as MenuItem;

            SelectionArea area = item.Tag as SelectionArea;

            if (area.RecognizedAreaVisible)
            {
                area.HideRecognizedAreas();
            }
            else
            {
                area.ShowRecognizedAreas();
            }
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
