using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel; 
using IGS.Helperclasses;
using System.Windows;
using Microsoft.Office.Interop.Excel;

namespace IGS.KNN
{
    class Crossvalidator
    {
        public List<WallProjectionSample> WPSlist { get; set; }
        public List<List<WallProjectionSample>> folds { get; set;  }

        public ClassificationHandler classHandler { get; set; }
        public Microsoft.Office.Interop.Excel.Application xlApp  {get;set;}

        public List<String> labels { get; set; }
        public int[,] cfMatrix { get; set; }
        
        


        public Crossvalidator(ClassificationHandler classhandler)
        {
            WPSlist = new List<WallProjectionSample>();
            folds = new List<List<WallProjectionSample>>();
            labels = new List<string>();
            xlApp = new Microsoft.Office.Interop.Excel.Application();
            xlApp.Visible = false;
            classHandler = classhandler;

            foreach (KNNClassifier.deviceRep devRep in classHandler.knnClassifier.devicesRepresentation)
            {
                String label = devRep.deviceName;
                labels.Add(label);
            }
            cfMatrix = new int[labels.Count , labels.Count];

            WPSlist = XMLComponentHandler.readWallProjectionSamplesFromXML();

            int k = 10;
            int foldsize = WPSlist.Count() / k;

            folds = splitWallProjectionSamplelRandomForOnline(foldsize);
            if(folds.Count > 10){
                int iterator = 0;
            foreach(WallProjectionSample wps in folds[10])
            {
                folds[iterator].Add(wps);
                folds[10].Remove(wps);
                iterator++;
                if(iterator == 10){
                    iterator = 0;
                }
            }
            }
        }

        public void crossValidate()
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "Crossval.xls";
            object misValue = System.Reflection.Missing.Value;
            Workbook wb = xlApp.Workbooks.Add(misValue);
            _Worksheet ws = (Worksheet)wb.Sheets.get_Item("Tabelle1");
            int error = 0;
            double totalAVGError = 0;
            int sampleSum = 0;
            int correct = 0;
            int actualDevPos = -1;
            int predictedPos = -1;
            for (int i = 0; i < folds.Count; i++)
            {
                List<WallProjectionSample> trainingSet = mergeTrainingSet(i);
                classHandler.knnClassifier.trainClassifier(trainingSet);

                foreach (WallProjectionSample wps in folds[i])
                {
                    WallProjectionSample newSample = new WallProjectionSample(wps);

                    for (int k = 0; k < labels.Count; k++)
                    {
                        if (wps.sampleDeviceName.Equals(labels[k]))
                        {
                            actualDevPos = k;
                            break;
                        }
                    }

                    classHandler.knnClassifier.classify(newSample);

                   for (int j = 0; j < labels.Count; j++)
                   {
                       if (newSample.sampleDeviceName.ToLower() == labels[j].ToLower())
                       {
                           predictedPos = j;
                           break;
                       }
                   }

                   cfMatrix[actualDevPos,predictedPos]++;

                   if (wps.sampleDeviceName.ToLower() == newSample.sampleDeviceName.ToLower())
                   {
                       correct += 1;
                   }
                   else
                   {
                       error += 1;
                   }

                }
            }

            for (int m = 0; m < cfMatrix.GetLength(0); m++)
            {
                for (int n = 0; n < cfMatrix.GetLength(1); n++)
                {

                    ws.Cells[m + 2, n + 2] = cfMatrix[m, n];
                }
            }
            for (int r = 0; r < labels.Count; r++)
            {
                ws.Cells[1, r + 2] = labels[r];
                ws.Cells[r + 2, 1] = labels[r];
            }

            foreach (List<WallProjectionSample> wpsList in folds)
            {
                sampleSum += wpsList.Count();
            }

            totalAVGError = error / sampleSum;

            wb.SaveAs(path, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            wb.Close(true, misValue, misValue);
            xlApp.Quit();

           
        }


        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }




        public List<WallProjectionSample> mergeTrainingSet(int testPlace)
        {
            List<WallProjectionSample> trainSet = new List<WallProjectionSample>();

            for (int i = 0; i < folds.Count; i++)
            {
                if (testPlace == i)
                {
                    continue;
                }
                else
                {
                    foreach (WallProjectionSample s in folds[i])
                    {
                        trainSet.Add(s);
                    }
                }
            }

            return trainSet;
        }

        public List<List<WallProjectionSample>> splitWallProjectionSamplelRandomForOnline(int packageSize)
        {
            List<List<WallProjectionSample>> result = new List<List<WallProjectionSample>>();
            int chosenPlace = 0;
            Random rand = new Random();

            while (WPSlist.Count >= packageSize)
            {
                List<WallProjectionSample> package = new List<WallProjectionSample>();
                for (int i = 0; i < packageSize; i++)
                {
                    chosenPlace = rand.Next(0, WPSlist.Count());
                    package.Add(WPSlist[chosenPlace]);
                    WPSlist.RemoveAt(chosenPlace);
                }
                result.Add(package);
            }
            //if (WPSlist.Count > 0)
            //{
            //    List<WallProjectionSample> endSamples = new List<WallProjectionSample>();
            //    foreach (WallProjectionSample wps in WPSlist)
            //    {
            //        endSamples.Add(wps);
            //    }

            //    result.Add(endSamples);
            //}

            return result;
        }
    }
}
 static class ExcelKonstanten
{
    /// <summary>
    /// Specifies the way links in the file are updated. If this
    /// argument is omitted, the user is prompted to specify how
    /// links will be updated. Otherwise, this argument is one of
    /// the values listed in the following table.
    /// </summary>
    public enum UpdateLinks
    {
        DontUpdate = 0,
        ExternalOnly = 1,
        RemoteOnly = 2,
        ExternalAndRemote = 3
    };

    /// <summary>
    /// True to open the workbook in read-only mode.
    /// </summary>
    public const bool ReadOnly = true;
    public const bool ReadWrite = false;

    /// <summary>
    /// If Microsoft Excel is opening a text file, this argument
    /// specifies the delimiter character, as shown in the following
    /// table. If this argument is omitted, the current delimiter
    /// is used.
    /// </summary>
    public enum Format
    {
        Tabs = 1,
        Commas = 2,
        Spaces = 3,
        Semicolons = 4,
        Nothing = 5,
        CustomCharacter = 6
    };

    /// <summary>
    /// True to have Microsoft Excel not display the read-only
    /// recommended message (if the workbook was saved with the
    /// Read-Only Recommended option).
    /// </summary>
    public const bool IgnoreReadOnlyRecommended = true;
    public const bool DontIgnoreReadOnlyRecommended = false;

    /// <summary>
    /// If the file is a Microsoft Excel 4.0 add-in, this argument
    /// is True to open the add-in so that its a visible window.
    /// If this argument is False or omitted, the add-in is opened
    /// as hidden, and it cannot be unhidden. This option doesn’t
    /// apply to add-ins created in Microsoft Excel 5.0 or later.
    /// If the file is an Excel template, True to open the specified
    /// template for editing. False to open a new workbook based on
    /// the specified template. The default value is False.
    /// </summary>
    public const bool Editable = true;
    public const bool NotEditable = false;

    /// <summary>
    /// If the file cannot be opened in read/write mode, this
    /// argument is True to add the file to the file notification
    /// list. Microsoft Excel will open the file as read-only, poll
    /// the file notification list, and then notify the user when
    /// the file becomes available. If this argument is False or
    /// omitted, no notification is requested, and any attempts to
    /// open an unavailable file will fail.
    /// </summary>
    public const bool Notify = true;
    public const bool DontNotifiy = false;

    /// <summary>
    /// The index of the first file converter to try when opening
    /// the file. The specified file converter is tried first; if
    /// this converter doesnt recognize the file, all other converters
    /// are tried. The converter index consists of the row numbers
    /// of the converters returned by the FileConverters property.
    /// </summary>
    public enum Converter
    {
        Default = 0
    };

    /// <summary>
    /// True to add this workbook to the list of recently used files.
    /// The default value is False.
    /// </summary>
    public const bool AddToMru = true;
    public const bool DontAddToMru = false;

    /// <summary>
    /// True saves files against the language of Microsoft Excel
    /// (including control panel settings). False (default) saves
    /// files against the language of Visual Basic for Applications
    /// (VBA) (which is typically US English unless the VBA project
    /// where Workbooks.Open is run from is an old internationalized
    /// XL5/95 VBA project).
    /// </summary>
    public const bool Local = true;
    public const bool NotLocal = false;

    public enum CorruptLoad
    {
        NormalLoad = 0,
        RepairFile = 1,
        ExtractData = 2
    };
}
