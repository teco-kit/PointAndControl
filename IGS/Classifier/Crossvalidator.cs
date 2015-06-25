using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel; 
using IGS.Helperclasses;
using System.Windows;
using Microsoft.Office.Interop.Excel;
using IGS.Server.Location;
using System.Windows.Media.Media3D;
using IGS.Server.IGS;
using IGS.Server.Devices;
using System.Diagnostics;

namespace IGS.Classifier
{
    class Crossvalidator
    {
        public List<WallProjectionSample> WPSlist { get; set; }
        public List<collisionPackage> collisionPacksList { get; set; }
        public List<collisionPackage> hoppColPackList { get; set; }
        public Locator locator { get; set; }

        public List<List<collisionPackage>> lineFolds { get; set; }
        public List<List<WallProjectionSample>> folds { get; set;  }

        public List<List<collisionPackage>> hoppFolds { get; set; }

        public List<List<int>> foldNumbers { get; set; }
        public List<List<int>> foldNumbersHopp { get; set; }
        
        public Microsoft.Office.Interop.Excel.Application xlApp  {get;set;}

        public List<String> labels { get; set; }
        public List<String> lineLabels { get; set; }

        public List<String> labelsTrue { get; set; }
        public List<String> lineLabelsTrue { get; set; }

        public List<String> hoppLabels { get; set; }
        public KNNClassifier knnClass { get; set; }
        public int[,] cfMatrix { get; set; }

        public int[,] cfMatrixCollision { get; set; }

        public int[,] cfMatrixHopp { get; set; }

        public String[] dropOutNames { get; set; }
        public struct collisionPackage
        {
           public Vector3D[] vecs;
           public String devName;
        }

        public double timeForPreprocessingClassification { get; set; }
        public double timeForTrainingClassification { get; set; }
        public double timeForClassifikationClassification { get; set; }

        public double timeForTrainingCollsion { get; set; }
        public double timeForClassifikationCollision { get; set; }
        public double timeForPreprocessingCollision = 0;

        public Crossvalidator(ClassificationHandler handler, KNNClassifier classifier, DataHolder data, CoordTransform transformer )
        {
            WPSlist = new List<WallProjectionSample>();
            folds = new List<List<WallProjectionSample>>();
            labels = new List<string>();
            labelsTrue = new List<string>();
            dropOutNames = new String[] { "Boxee_3", "Plugwise_3" };

            collisionPacksList = new List<collisionPackage>();
            lineFolds = new List<List<collisionPackage>>();
            lineLabels = new List<string>();

            hoppColPackList = new List<collisionPackage>();
            hoppFolds = new List<List<collisionPackage>>();
            hoppLabels = new List<string>();
          
            xlApp = new Microsoft.Office.Interop.Excel.Application();
            xlApp.Visible = false;
            locator = new Locator(data, null, transformer);
            knnClass = classifier;


            //set up for classification
            foreach (Device d in data.Devices)
            {
                String label = d.Id;
                if (dropOutNames.Contains(label))
                {
                    continue;
                }
                else
                {
                    labelsTrue.Add(label);
                }
                
            }

            for (int i = 0; i < labelsTrue.Count;i++ ) 
            {
                String del = labelsTrue[i].Replace("_", "");
                labels.Add(del);
            }
            cfMatrix = new int[labels.Count, labels.Count];

            var watch = Stopwatch.StartNew();
            watch.Start();
            WPSlist = handler.extractor.calculateWallProjectionSamples(handler.sCalculator, handler.extractor.rawSamplesPerSelect);
            watch.Stop();
            timeForPreprocessingClassification = (double)(watch.ElapsedMilliseconds) / WPSlist.Count;
            

            List<WallProjectionSample> tmpWPSList = new List<WallProjectionSample>();
            
                foreach (WallProjectionSample wps in WPSlist)
                {

                    if (!(dropOutNames.Contains(wps.sampledeviceIdentifier)))
                    {
                        tmpWPSList.Add(wps);
                    }
                }

            WPSlist = tmpWPSList;
            
            int k = 10;
            int foldsize = WPSlist.Count() / k;

            foldNumbers = createCrossvalidation(WPSlist.Count, k);

            folds = splitWallProjectionSamplelRandomForOnline(foldNumbers);
           
            


            
            // setup for colldet

            foreach (SampleExtractor.rawSample rs in handler.extractor.rawSamplesPerSelect)
            {
                collisionPackage package = new collisionPackage();
                package.devName = rs.label;
                package.vecs = rs.joints;
                collisionPacksList.Add(package);
            }

            List<collisionPackage> tmpColPackList = new List<collisionPackage>();

            
                foreach (collisionPackage p in collisionPacksList)
                {
                    if (!(dropOutNames.Contains(p.devName)))
                    {
                        tmpColPackList.Add(p);
                    }
                }
            

            collisionPacksList = tmpColPackList;

                foreach (collisionPackage p in collisionPacksList)
                {
                    if (!(lineLabels.Contains(p.devName)))
                    {
                        lineLabels.Add(p.devName);
                    }
                }

            cfMatrixCollision = new int[lineLabels.Count, lineLabels.Count];

            lineFolds = splitLineVectorsRandom(foldNumbers);
            
            //setup for colldet mit hoppe files 


            foreach (SampleExtractor.rawSample rs in handler.extractor.hoppRS)
            {
                collisionPackage package = new collisionPackage();
                package.devName = rs.label;
                package.vecs = rs.joints;
                hoppColPackList.Add(package);
            }

            List<collisionPackage> tmpColPackListHopp = new List<collisionPackage>();


            foreach (collisionPackage p in hoppColPackList)
            {
                if (!(dropOutNames.Contains(p.devName)))
                {
                    tmpColPackListHopp.Add(p);
                }
            }


            hoppColPackList = tmpColPackListHopp;

            foreach (collisionPackage p in hoppColPackList)
            {
                if (!(hoppLabels.Contains(p.devName)))
                {
                    hoppLabels.Add(p.devName);
                }
            }

             cfMatrixHopp = new int[hoppLabels.Count, hoppLabels.Count];
                
        
             int foldsizeHopp = hoppColPackList.Count() / k;
             foldNumbersHopp = createCrossvalidation(hoppColPackList.Count, k);
             hoppFolds = splitLineVectorsRandomHopp(foldNumbersHopp);
        }



        public void crossValidateCollision()
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "CrossvalCollision.xls";
            object misValue = System.Reflection.Missing.Value;
            Workbook wb = xlApp.Workbooks.Add(misValue);
            _Worksheet ws = (Worksheet)wb.Sheets.get_Item(1);
            Stopwatch trainingsWatch = new Stopwatch();
            Stopwatch classificationWatch = new Stopwatch();
            int nrClass = 0;
            int nrTrain = 0;
            int error = 0;
            double totalAVGError = 0;
            int sampleSum = 0;
            int correct = 0;
            int actualDevPos = -1;
            int predictedPos = -1;

            for (int i = 0; i < lineFolds.Count; i++)
            {

                List<collisionPackage> trainingSet = mergeTrainSetLine(i);
                nrTrain++;
                foreach (String label in lineLabels)
                {
                    List<Vector3D[]> trainVecs = getVectorsForDevice(trainingSet, label);
                    trainingsWatch.Start();
                    locator.ChangeDeviceLocation(trainVecs, label);
                    trainingsWatch.Stop();
                   
                }

                foreach (collisionPackage pack in lineFolds[i])
                {

                    for (int k = 0; k < lineLabels.Count; k++)
                    {
                        if (pack.devName.Equals(lineLabels[k]))
                        {
                            actualDevPos = k;
                            break;
                        }
                    }

                    classificationWatch.Start();
                    String collisionDev = CollisionDetection.getNameOfDeviceWithMinDist(locator.Data.Devices, pack.vecs);
                    classificationWatch.Stop();
                    nrClass++;
                    for (int j = 0; j < lineLabels.Count; j++)
                    {
                        if (collisionDev.Equals(lineLabels[j]))
                        {
                            predictedPos = j;
                            break;
                        }
                    }

                    cfMatrixCollision[actualDevPos, predictedPos]++;

                    if (pack.devName.Equals(collisionDev))
                    {
                        correct += 1;
                    }
                    else
                    {
                        error += 1;
                    }

                }


            }

            for (int m = 0; m < cfMatrixCollision.GetLength(0); m++)
            {
                for (int n = 0; n < cfMatrixCollision.GetLength(1); n++)
                {

                    ws.Cells[m + 2, n + 2] = cfMatrixCollision[m, n];
                }
            }
            for (int r = 0; r < lineLabels.Count; r++)
            {
                ws.Cells[1, r + 2] = lineLabels[r];
                ws.Cells[r + 2, 1] = lineLabels[r];
            }

            foreach (List<collisionPackage> lineFold in lineFolds)
            {
                sampleSum += lineFold.Count();
            }

            totalAVGError = error / sampleSum;
            timeForTrainingCollsion = trainingsWatch.ElapsedMilliseconds;
            timeForClassifikationCollision = classificationWatch.ElapsedMilliseconds;

            timeForClassifikationCollision = ((double)(timeForClassifikationCollision) / nrClass);
            timeForTrainingCollsion = ((double)(timeForTrainingCollsion) / nrTrain);
            wb.SaveAs(path, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            wb.Close(true, misValue, misValue);
            xlApp.Quit();



        }


       

        public void crossValidateClassifier()
        {
            String path = AppDomain.CurrentDomain.BaseDirectory + "CrossvalClassifier.xls";
            int nrClass = 0;
            int nrTrain = 0;
            object misValue = System.Reflection.Missing.Value;
            Workbook wb = xlApp.Workbooks.Add(misValue);
            _Worksheet ws = (Worksheet)wb.Sheets.get_Item(1);
            Stopwatch trainingsWatch = new Stopwatch();
            Stopwatch classificationWatch = new Stopwatch();
            int error = 0;
            long test = 0;
            double totalAVGError = 0;
            int sampleSum = 0;
            int correct = 0;
            int actualDevPos = -1;
            int predictedPos = -1;
            for (int i = 0; i < folds.Count; i++)
            {
                List<WallProjectionSample> trainingSet = mergeTrainingSet(i);

                trainingsWatch.Start();
                knnClass.trainClassifier(trainingSet);
                trainingsWatch.Stop();
                nrTrain++;
                foreach (WallProjectionSample wps in folds[i])
                {
                    WallProjectionSample newSample = new WallProjectionSample(wps);

                    for (int k = 0; k < labels.Count; k++)
                    {
                        if (wps.sampledeviceIdentifier.Equals(labelsTrue[k]))
                        {
                            actualDevPos = k;
                            break;
                        }
                    }

                   classificationWatch.Start();
                   knnClass.classify(newSample);
                   classificationWatch.Stop();
                    nrClass++;
                   for (int j = 0; j < labels.Count; j++)
                   {
                       if (newSample.sampledeviceIdentifier.ToLower() == labels[j].ToLower())
                       {
                           predictedPos = j;
                           break;
                       }
                   }

                   cfMatrix[actualDevPos,predictedPos]++;

                   if (wps.sampledeviceIdentifier.ToLower() == newSample.sampledeviceIdentifier.ToLower())
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
            timeForClassifikationClassification = ((double)(classificationWatch.ElapsedMilliseconds) / nrClass) ;
            timeForTrainingClassification = ((double)(trainingsWatch.ElapsedMilliseconds)/nrTrain);
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

        public List<collisionPackage> mergeTrainSetLine(int testPlace)
        {

            List<collisionPackage> trainSet = new List<collisionPackage>();


            for (int i = 0; i < lineFolds.Count; i++)
            {
                if (testPlace == i)
                {
                    continue;
                }
                else
                {
                    foreach (collisionPackage package in lineFolds[i])
                    {
                        trainSet.Add(package);
                    }
                }
            }

            return trainSet;
        }

        public List<collisionPackage> mergeTrainSetHopp(int testPlace)
        {

            List<collisionPackage> trainSet = new List<collisionPackage>();


            for (int i = 0; i < hoppFolds.Count; i++)
            {
                if (testPlace == i)
                {
                    continue;
                }
                else
                {
                    foreach (collisionPackage package in hoppFolds[i])
                    {
                        trainSet.Add(package);
                    }
                }
            }

            return trainSet;
        }

        public List<Vector3D[]> getVectorsForDevice(List<collisionPackage> set, String name)
        {
            List<Vector3D[]> result = new List<Vector3D[]>();
            List<collisionPackage> searchedPackages = new List<collisionPackage>();

            foreach (collisionPackage package in set)
            {
                if (name.Equals(package.devName))
                {
                    searchedPackages.Add(package);
                }
            }

            foreach (collisionPackage package in searchedPackages)
            {
                result.Add(package.vecs);
            }

            return result;
        }

        public List<List<int>> createCrossvalidation(int sampleCount, int k)
        {
            Random rand = new Random();
            List<List<int>> resultList = new List<List<int>>();
            List<int> usedInts = new List<int>();
            int counter = 0;
            int pos = -1;
            int packageSize = (int)Math.Floor((double)sampleCount / k);
            int leftoversamples = sampleCount - packageSize * 10;
            bool allowedPos = false;
            for (int i = 0; i < k; i++)
            {
                List<int> fold = new List<int>();
                for (int j = 0; j < packageSize; j++)
                {
                    while (allowedPos == false)
                    {
                        pos = rand.Next(sampleCount);

                        if (usedInts.Contains(pos) == false)
                        {
                            usedInts.Add(pos);
                            fold.Add(pos);
                            allowedPos = true;
                        }


                    }
                    allowedPos = false;
                }
                resultList.Add(fold);
            }

            List<int> missingInts = new List<int>();
            int foldChoose = -1;
            for (int i = 0; i < sampleCount; i++)
            {
                if (usedInts.Contains(i) == false)
                {
                    usedInts.Add(i);
                    foldChoose = rand.Next(k);
                    resultList[foldChoose].Add(i);
                }
            }

            //test if all numers are traded and non is double
            bool allDifferen = false;
            int foundICount = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                foreach (List<int> fold in resultList)
                {
                    if (fold.Contains(i))
                    {
                        foundICount++;
                    }
                }

                if (foundICount != 1)
                {
                    Console.WriteLine("EINE POS ÖFTERS VERGEBEN!!");
                }
                else
                {
                    foundICount = 0;
                }
            }

                return resultList;

        }

        public List<List<WallProjectionSample>> splitWallProjectionSamplelRandomForOnline(List<List<int>> crossVal)
        {
            List<List<WallProjectionSample>> result = new List<List<WallProjectionSample>>();
            int chosenPlace = 0;
            Random rand = new Random();

            foreach (List<int> l in crossVal)
            {
                List<WallProjectionSample> fold = new List<WallProjectionSample>();
                foreach (int i in l)
                {
                    fold.Add(WPSlist[i]);
                }

                result.Add(fold);
            }

            return result;
        }

        public List<List<collisionPackage>> splitLineVectorsRandom(List<List<int>> crossVal)
        {
            
            List<List<collisionPackage>> result = new List<List<collisionPackage>>();

             foreach (List<int> l in crossVal)
            {
                List<collisionPackage> fold = new List<collisionPackage>();
                foreach (int i in l)
                {
                    fold.Add(collisionPacksList[i]);
                }

                result.Add(fold);
            }

             return result;
        }

        public List<List<collisionPackage>> splitLineVectorsRandomHopp(List<List<int>> crossVal)
        {

            List<List<collisionPackage>> result = new List<List<collisionPackage>>();

            foreach (List<int> l in crossVal)
            {
                List<collisionPackage> fold = new List<collisionPackage>();
                foreach (int i in l)
                {
                    fold.Add(hoppColPackList[i]);
                }

                result.Add(fold);
            }

            return result;
        }

        public List<Vector3D[]> getVectorSamplesPerDevice(List<Vector3D[]> set,List<String> setLabels,  String name)
        {

            List<Vector3D[]> result = new List<Vector3D[]>();
            
            for (int i = 0; i < set.Count; i++)
            {
                if (setLabels[i].Equals(name))
                {
                    result.Add(set[i]);
                }
            }

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
