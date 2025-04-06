using System;
using Tesseract;

namespace OpenDMSBackend.Core.Services
{
    public class OCRService : IOCRService
    {
        public string GetOCRContent(byte[] content)
        {
            string result = string.Empty;
            //from https://github.com/charlesw/tesseract-samples/blob/master/src/Tesseract.ConsoleDemo/Program.cs
            using (TesseractEngine engine = new TesseractEngine(@"./OCRData", "eng+deu", EngineMode.Default))//TODO make the languages confgurable
            {
                using Pix img = Pix.LoadFromMemory(content);
                using Page page = engine.Process(img);
                result = result + Environment.NewLine + page.GetText();
            }
            return result;
        }
    }
}
