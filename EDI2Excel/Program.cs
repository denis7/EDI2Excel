using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            edi2CSV();         



        }

        static private int countFilesByFileType(string directoryPath, string fileType)
        {
            string searchFileType = "*." + fileType;
            int xmlFileCount = System.IO.Directory.GetFiles(directoryPath, searchFileType).Length;

            return xmlFileCount;
        }


        static private void edi2CSV()
        {

            var stringEdiSeparator = ConfigurationManager.AppSettings["EdiSeparator"];                    
            var fileType = ConfigurationManager.AppSettings["EdiFileType"];
            var inputPath = ConfigurationManager.AppSettings["InputPath"];
            var outputPath = ConfigurationManager.AppSettings["OutputPath"];

            //TODO function that checks if appsettings is null and raises exception  OutputPath
            if (string.IsNullOrEmpty(stringEdiSeparator) ||
                string.IsNullOrEmpty(fileType) ||
                string.IsNullOrEmpty(inputPath) ||
                string.IsNullOrEmpty(outputPath) 
              )
                {                   
                    throw new ConfigurationErrorsException("Missing config value for EdiSeparator, EdiFileType, InputPath or Outpath ");
                }

            char EdiSeparator = stringEdiSeparator.ToCharArray()[0];

            string[] listFiles = System.IO.Directory.GetFiles(inputPath, "*." + fileType);

            foreach (string strFile in listFiles)
            {
                processFile(strFile,outputPath, EdiSeparator);
            }        
                                             

        }

        private static void processFile(string inputFile,string outputPath, char EdiSeparator)
        {

            var fileType = ConfigurationManager.AppSettings["EdiFileType"];
            var listLines = new List<string>();
            string line;
            using (var reader = System.IO.File.OpenText(inputFile))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    listLines.Add(line);
                }

            }                     


            // max tidles  in each line what is max tidles
            int maxSoFar = 0;
            foreach (string zeline in listLines)
            {
                int zetotal = zeline.Count(x => x == EdiSeparator);
                if (zetotal > maxSoFar)
                {
                    maxSoFar = zetotal;
                }

            }

            // determine lines   (totalLines-2)/2
            int totalLines = (listLines.Count() - 2) / 2;



            List<string> pathParts = inputFile.Split(@"\".ToCharArray()[0]).ToList<string>();

            string fileName = "";

            fileName = pathParts.Last<string>();

            fileName = fileName.Replace(fileType, "csv");

            //make sure outputPath ends in /
            outputPath = outputPath.EndsWith(@"\") ? outputPath : outputPath + @"\";
            
       

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputPath + fileName))
            {
                for (int i = 0; i < totalLines; i++)
                {
                    string outline = listLines[i];
                    string outline2 = listLines[i + totalLines + 1];  //1 is to skip over EOF line

                    outline = makeCSVmax(maxSoFar, outline,EdiSeparator);
                    outline2 = makeCSVmax(maxSoFar, outline2, EdiSeparator);

                    file.WriteLine(outline);
                    file.WriteLine(outline2);

                }

                file.WriteLine();

            }
        }

        private static string makeCSVmax(int maxSoFar, string outline, char EdiSeparator)
        {
            int currentTotal = outline.Count(x => x == EdiSeparator);
            int extraTotal = maxSoFar - currentTotal;
            outline = outline.Replace(EdiSeparator, ',') + new String(',', extraTotal);
            return outline;
        }     


    }
}
