using System;
using Tesseract;

namespace OpenDMSBackend.Core.Services
{
    public class OCRService : IOCRService
    {
        public string GetOCRContent(byte[] content, string mimeType)
        {
            string result = string.Empty;
            //from https://github.com/charlesw/tesseract-samples/blob/master/src/Tesseract.ConsoleDemo/Program.cs
            using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using var img = Pix.LoadFromMemory(content);
                using var page = engine.Process(img);
                result = result + Environment.NewLine + page.GetText();
            }
            return result;
        }
    }
}
