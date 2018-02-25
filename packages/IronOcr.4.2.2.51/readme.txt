IronOCR - An advanced OCR library for .Net.  

The OCR engine reads and extracts text from images and scanned documents.  “Image to text” functionality  is added to Desktop, Console and Web applications in about 5 minuites.

C# Example 1 - Automated Ocr:
==============================
using IronOcr;
//..
var Ocr = new AutoOcr();
var Result = Ocr.Read(@"C:\path\to\image.png");
Console.WriteLine(Result.Text);



C# Example 2 - Advanced Ocr:
==============================
using IronOcr;
//..
var Ocr = new AdvancedOcr()
{
    CleanBackgroundNoise = true,
    EnhanceContrast = true,
    EnhanceResolution = true,
    Language =  IronOcr.Languages.English.OcrLanguagePack,
    Strategy = IronOcr.AdvancedOcr.OcrStrategy.Advanced,
    ColorSpace = AdvancedOcr.OcrColorSpace.Color,
    DetectWhiteTextOnDarkBackgrounds = true,
    InputImageType = AdvancedOcr.InputTypes.AutoDetect,
    RotateAndStraighten = true,
    ReadBarCodes = true,
    ColorDepth =4
};

var testImage = @"C:\path\to\scan.tiff";

var Results = Ocr.Read(testImage);

var Barcodes = Results.Barcodes.Select(b => b.Value);

Console.WriteLine(Results.Text);
Console.WriteLine("Barcodes:" + String.Join(",", Barcodes));

C# Example 3 - PDF Ocr:
==============================
using IronOcr;
//..
 var Ocr = new AdvancedOcr()
            {
                CleanBackgroundNoise = false,
                ColorDepth = 4,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                EnhanceContrast = false,
                DetectWhiteTextOnDarkBackgrounds = false,
                RotateAndStraighten = false,
                Language = IronOcr.Languages.English.OcrLanguagePack,
                EnhanceResolution = false,
                InputImageType = AdvancedOcr.InputTypes.Document,
                ReadBarCodes = true,
                Strategy = AdvancedOcr.OcrStrategy.Fast
            };

var Results = Ocr.ReadPdf(@"C:\Users\Me\Desktop\Invoice.pdf");
var Barcodes = Results.Barcodes.Select(b => b.Value);

Console.WriteLine(Results.Text);
Console.WriteLine("Barcodes:" + String.Join(",", Barcodes));


Supports: Console Applications, ASP.NET Web Applications, MVC, Web APIs, Desktop Applications written in any .Net language such as C# or VB.Net

Requires: .Net framework 4.0 or above on the Windows Platform or Azure Cloud.                      

For more information visit http://ironsoftware.com/csharp/ocr/