﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataScienceECom;
using DataScienceECom.Models;
using DataScienceECom.Phis;
using NDSB.FileUtils;
using NDSB.Models.SparseModels;
using NDSB.Models.Streaming.Phis;
using NDSB.SparseMappings;
using NDSB.SparseMethods;

namespace NDSB
{
    public partial class MainScreen : Form
    {
        public MainScreen()
        {
            InitializeComponent();
        }

        private void processBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show(IntPtr.Size.ToString());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Multiselect = true;
            fdlg.Title = "Files to merge";
            string[] filePaths = new string[1];
            if (fdlg.ShowDialog() == DialogResult.OK)
                filePaths = fdlg.FileNames;

            CSVHelper.ColumnBind(filePaths);
        }

        private void shuffleBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            int[] seeds = shuffleSeedTbx.Text.Split(';').Select(c => Convert.ToInt32(c)).ToArray();

            fdlg.Multiselect = true;
            fdlg.Title = "Files to shuffle";
            string[] filePaths = new string[1];
            if (fdlg.ShowDialog() == DialogResult.OK)
                filePaths = fdlg.FileNames;

            foreach (int seed in seeds)
                for (int i = 0; i < filePaths.Length; i++)
                    FShuffler.Shuffle(filePaths[i], seed);
        }

        private void splitBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            double part = Convert.ToDouble(splitTbx.Text);

            fdlg.Multiselect = true;
            fdlg.Title = "Files to split";
            string[] filePaths = new string[1];
            if (fdlg.ShowDialog() == DialogResult.OK)
                filePaths = fdlg.FileNames;

            for (int i = 0; i < filePaths.Length; i++)
                FSplitter.SplitRelative(filePaths[i], part);
        }

        private void downSampleBtn_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Multiselect = true;
            fdlg.Title = "Files to down sample";
            string[] filePaths = new string[1];
            if (fdlg.ShowDialog() == DialogResult.OK)
                filePaths = fdlg.FileNames;

            int[] eltsPerClass = downSampleTbx.Text.Split(';').Select(c => Convert.ToInt32(c)).ToArray();
            for (int i = 0; i < eltsPerClass.Length; i++)
                for (int j = 0; j < filePaths.Length; j++)
                    DownSample.Run(filePaths[j], eltsPerClass[i], DSCdiscountUtils.GetLabelCDiscountDB);
        }

        private void getHistogramBtn_Click(object sender, EventArgs e)
        {
            string trainFilePath = "";
            OpenFileDialog fdlg = new OpenFileDialog();

            if (fdlg.ShowDialog() == DialogResult.OK)
                trainFilePath = fdlg.FileName;

            int[] labels = DSCdiscountUtils.ReadLabelsFromTraining(trainFilePath);

            EmpiricScore<int> es = new EmpiricScore<int>();
            for (int i = 0; i < labels.Length; i++)
                es.UpdateKey(labels[i], 1);

            //es = es.Normalize();

            string[] text = es.Scores.OrderBy(c => c.Value).Select(c => c.Key + " " + c.Value).ToArray();
            File.WriteAllLines(Path.GetDirectoryName(trainFilePath) + "\\" + Path.GetFileNameWithoutExtension(trainFilePath) + "_hist.txt", text);

            MessageBox.Show("h");
        }

        private void predictSGDBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog trainingFilesOFD = new OpenFileDialog();
            trainingFilesOFD.Multiselect = true;
            trainingFilesOFD.Title = "Train files path";
            trainingFilesOFD.ShowDialog();
            if (!trainingFilesOFD.CheckFileExists) return;

            OpenFileDialog validationFilesOFD = new OpenFileDialog();
            validationFilesOFD.Multiselect = true;
            validationFilesOFD.Title = "Validation files path";
            validationFilesOFD.ShowDialog();

            if (!validationFilesOFD.CheckFileExists) return;

            OpenFileDialog testFileOFD = new OpenFileDialog();
            testFileOFD.Title = "Test file path";
            testFileOFD.ShowDialog();

            if (!testFileOFD.CheckFileExists) return;

            string currentDirectory = Path.GetDirectoryName(trainingFilesOFD.FileNames[0]),
                testFilePath = testFileOFD.FileName;

            List<Phi<int>> phis = new List<Phi<int>> { Phis.phi20, Phis.phi19 };

            string[] learningFiles = trainingFilesOFD.FileNames;
            Array.Sort(learningFiles);
            string[] validationFiles = validationFilesOFD.FileNames;
            Array.Sort(validationFiles);

            foreach (IStreamingModel<int, int> model in ModelGenerators.Entropia6())
                for (int i = 0; i < learningFiles.Length; i++)
                    foreach (Phi<int> phi in phis)
                    {
                        string file = learningFiles[i];
                        model.ClearModel();
                        TrainModels.TrainStreamingModel(model, phi, file);
                        string modelString = Path.GetFileNameWithoutExtension(file) + ";" + Path.GetFileNameWithoutExtension(validationFiles[i]) + ";" +
                            phi.Method.Name + ";" + model.Description();

                        List<int> testModelPredictions = TrainModels.Predict(model, phi, testFilePath);

                        File.WriteAllText(Path.GetDirectoryName(file) + "\\test\\" +
                            modelString + "_pred.csv", modelString + Environment.NewLine);

                        File.AppendAllLines(Path.GetDirectoryName(file) + "\\test\\" +
                            modelString + "_pred.csv",
                            testModelPredictions.Select(t => t.ToString()));

                        string validationFileName = validationFiles[i];

                        var validationModelPredictions = TrainModels.Predict(model, phi, validationFileName);

                        File.WriteAllText(Path.GetDirectoryName(file) + "\\validation\\" +
                            modelString + "_val.csv",
                            modelString + Environment.NewLine);

                        File.AppendAllLines(Path.GetDirectoryName(file) + "\\validation\\" +
                            modelString + "_val.csv",
                            validationModelPredictions.Select(t => t.ToString()));

                    }
        }

        private void splitTbx_TextChanged(object sender, EventArgs e)
        {

        }

        private static int[] ToIntArray(TextBox tbx)
        {
            return tbx.Text.Split(';').Select(c => Convert.ToInt32(c)).ToArray();
        }

        private void nearestCentroidPredictBtn_Click(object sender, EventArgs e)
        {
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            models.Add(new SparseNearestCentroid<string>(new PureInteractions(1, 20)));

            TranslateTrainAndPredict(models, false);
        }

        private void NCTrainValidatePredictBtn_Click(object sender, EventArgs e)
        {
            string testFilePath = "",
                validationFilePath = "";
            string[] trainFilePath = new string[0];

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Test file path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                testFilePath = fdlg.FileName;

            fdlg = new OpenFileDialog();
            fdlg.Title = "Train file(s) path";
            fdlg.Multiselect = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
                trainFilePath = fdlg.FileNames;

            fdlg = new OpenFileDialog();
            fdlg.Title = "Validation file path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                validationFilePath = fdlg.FileName;

            for (int i = 0; i < trainFilePath.Length; i++)
            {
                List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();

                models.Add(new DecisionTree(4500, 5));

                GenericMLHelper.TranslateTrainPredictAndValidate(models.ToArray(), testFilePath, trainFilePath[i], validationFilePath, false);
            }
        }

        private void extractColumnBtn_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "File path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                filePath = fdlg.FileName;

            int columnIndex = Convert.ToInt32(extractColumnTbx.Text);

            CSVHelper.ExtractColumn(filePath, columnIndex);
        }

        private void countCommonBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog file1OFD = new OpenFileDialog();
            file1OFD.Title = "File 1";
            file1OFD.ShowDialog();
            if (!file1OFD.CheckFileExists) return;

            OpenFileDialog file2OFD = new OpenFileDialog();
            file2OFD.Title = "File 2";
            file2OFD.ShowDialog();
            if (!file2OFD.CheckFileExists) return;

            MessageBox.Show((100 * CSVHelper.CountElementsInCommont(file1OFD.FileName, file2OFD.FileName)).ToString());
        }

        private void predictKNNBtn_Click(object sender, EventArgs e)
        {
            int nbNeighbours = Convert.ToInt32(nbNeighbTbx.Text);
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            models.Add(new SparseKNN<string>(SparseDistances.SumSquares<string>, nbNeighbours, 0.15, new ToSphere<string>()));
            TranslateTrainAndPredict(models, false);
        }

        private void trainAndPredictBtn_Click(object sender, EventArgs e)
        {
            string[] trainFilePaths = new string[1];
            string testFilePath = "";

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Test file path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                testFilePath = fdlg.FileName;

            fdlg = new OpenFileDialog();
            fdlg.Multiselect = true;
            fdlg.Title = "Train file(s) path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                trainFilePaths = fdlg.FileNames;

            // Best version of phi ;) (actually, really close to phi16)
            Phi<int> phi = Phis.phi17;

            foreach (IStreamingModel<int, int> model in ModelGenerators.Entropia6())
                for (int i = 0; i < trainFilePaths.Length; i++)
                {
                    string file = trainFilePaths[i];
                    model.ClearModel();
                    TrainModels.TrainStreamingModel(model, phi, file);
                    string modelString = Path.GetFileNameWithoutExtension(file) + ";" + Path.GetFileNameWithoutExtension(trainFilePaths[i]) + ";" +
                        phi.Method.Name + ";" + model.Description();

                    List<int> testModelPredictions = TrainModels.Predict(model, phi, testFilePath);

                    File.WriteAllText(Path.GetDirectoryName(file) + "\\test\\" +
                        modelString + "_pred.csv", modelString + Environment.NewLine);

                    File.AppendAllLines(Path.GetDirectoryName(file) + "\\test\\" +
                        modelString + "_pred.csv",
                        testModelPredictions.Select(t => t.ToString()));
                }
        }

        private void translateAndPredictRFBtn_Click(object sender, EventArgs e)
        {
            int minLeafSize = Convert.ToInt32(minEltsLeafTbx.Text);
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            models.Add(new DecisionTree(4500, minLeafSize));
            TranslateTrainAndPredict(models,false);
        }

        private void decisionTreePredictBtn_Click(object sender, EventArgs e)
        {
            int[] minSizes = ToIntArray(minEltsLeafTbx);
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            for (int i = 0; i < minSizes.Length; i++)
                models.Add(new DecisionTree(4500, minSizes[i]));
            TrainPredictAndValidateFromTFIDF(models);
        }

        private void KNNcvTfidfBtn_Click(object sender, EventArgs e)
        {
            int[] nbNeighbours = ToIntArray(nbNeighbTbx);
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            TrainPredictAndValidateFromTFIDF(models);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog trainingFilesOFD = new OpenFileDialog();
            trainingFilesOFD.Multiselect = true;
            trainingFilesOFD.Title = "Train files path";
            trainingFilesOFD.ShowDialog();
            if (!trainingFilesOFD.CheckFileExists) return;


            /*
            OpenFileDialog validationFilesOFD = new OpenFileDialog();
            validationFilesOFD.Multiselect = true;
            validationFilesOFD.Title = "Validation files path";
            validationFilesOFD.ShowDialog();


            if (!validationFilesOFD.CheckFileExists) return;
            */
            OpenFileDialog testFileOFD = new OpenFileDialog();
            testFileOFD.Title = "Test file path";
            testFileOFD.ShowDialog();

            if (!testFileOFD.CheckFileExists) return;

            string currentDirectory = Path.GetDirectoryName(trainingFilesOFD.FileNames[0]),
                testFilePath = testFileOFD.FileName;

            List<Phi<Hierarchy>> phis = new List<Phi<Hierarchy>> { HierarchicalPhis.HPhi1 };

            string[] learningFiles = trainingFilesOFD.FileNames;
            Array.Sort(learningFiles);

            /*
            string[] validationFiles = validationFilesOFD.FileNames;
            Array.Sort(validationFiles);
            */

            foreach (IStreamingModel<Hierarchy, int> model in ModelGenerators.HEntropia())
                for (int i = 0; i < learningFiles.Length; i++)
                    foreach (Phi<Hierarchy> phi in phis)
                    {
                        string file = learningFiles[i];
                        model.ClearModel();
                        TrainModels.TrainStreamingModel(model, phi, file);

                        string modelString = Path.GetFileNameWithoutExtension(file) + ";" + Path.GetFileNameWithoutExtension(testFilePath) + ";" +
                            phi.Method.Name + ";" + model.Description();

                        List<int> testModelPredictions = TrainModels.Predict(model, phi, testFilePath);

                        File.WriteAllText(Path.GetDirectoryName(file) + "\\test\\" +
                            modelString + "_pred.csv", modelString + Environment.NewLine);

                        File.AppendAllLines(Path.GetDirectoryName(file) + "\\test\\" +
                            modelString + "_pred.csv",
                            testModelPredictions.Select(t => t.ToString()));

                        /*
                        string validationFileName = validationFiles[i];

                        var validationModelPredictions = TrainModels.Predict(model, phi, validationFileName);

                        File.WriteAllText(Path.GetDirectoryName(file) + "\\validation\\" +
                            modelString + "_val.csv",
                            modelString + Environment.NewLine);

                        File.AppendAllLines(Path.GetDirectoryName(file) + "\\validation\\" +
                            modelString + "_val.csv",
                            validationModelPredictions.Select(t => t.ToString()));
                        */
                    }
        }

        private void bowRunBtn_Click(object sender, EventArgs e)
        {
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            models.Add(new BagOfWords<string>(4, 8, 500, 0.5, 3));
            models.Add(new BagOfWords<string>(4, 5, 1000, 0.5, 3));
            models.Add(new BagOfWords<string>(4, 5, 1000, 0.25, 3));
            models.Add(new BagOfWords<string>(4, 5, 1000, 0.15, 3));
            models.Add(new BagOfWords<string>(4, 5, 2000, 0.5, 3));
            models.Add(new BagOfWords<string>(4, 5, 2000, 0.25, 3));
            models.Add(new BagOfWords<string>(4, 5, 2000, 0.15, 3));
            TrainPredictAndValidateFromTFIDF(models);
        }

        private void TrainPredictAndValidateFromTFIDF(List<IModelClassification<Dictionary<string, double>>> models)
        {
            string testTFIDFFilePath = "",
                validationTFIDFFilePath = "",
                trainFilePath = "";

            string[] trainFilePathTFIDF = new string[0];

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Test file path (TFIDF)";
            if (fdlg.ShowDialog() == DialogResult.OK)
                testTFIDFFilePath = fdlg.FileName;

            fdlg = new OpenFileDialog();
            fdlg.Title = "Train file(s) path (TFIDF)";
            fdlg.Multiselect = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
                trainFilePathTFIDF = fdlg.FileNames;

            fdlg = new OpenFileDialog();
            fdlg.Title = "Train file(s) path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                trainFilePath = fdlg.FileName;

            fdlg = new OpenFileDialog();
            fdlg.Title = "Validation file path (TFIDF)";
            if (fdlg.ShowDialog() == DialogResult.OK)
                validationTFIDFFilePath = fdlg.FileName;

            for (int i = 0; i < trainFilePathTFIDF.Length; i++)
                GenericMLHelper.TrainPredictAndValidateFromTFIDF(models.ToArray(), trainFilePath, trainFilePathTFIDF[i], testTFIDFFilePath, validationTFIDFFilePath);
        }

        private void TranslateTrainAndPredict(List<IModelClassification<Dictionary<string, double>>> models, bool stem)
        {
            string[] trainFilePaths = new string[1];
            string testFilePath = "";

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Test file path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                testFilePath = fdlg.FileName;

            fdlg = new OpenFileDialog();
            fdlg.Multiselect = true;
            fdlg.Title = "Train file(s) path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                trainFilePaths = fdlg.FileNames;

            for (int i = 0; i < trainFilePaths.Length; i++)
            {
                GenericMLHelper.TranslateTrainPredictAndWrite(models.ToArray(), trainFilePaths[i], testFilePath, stem);
                models.Clear();
            }
        }

        private void TranslateTrainValidatePredict(List<IModelClassification<Dictionary<string, double>>> models, bool stem = false)
        {
            string testFilePath = "",
                validationFilePath = "";
            string[] trainFilePath = new string[0];

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Test file path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                testFilePath = fdlg.FileName;

            fdlg = new OpenFileDialog();
            fdlg.Title = "Train file(s) path";
            fdlg.Multiselect = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
                trainFilePath = fdlg.FileNames;

            fdlg = new OpenFileDialog();
            fdlg.Title = "Validation file path";
            if (fdlg.ShowDialog() == DialogResult.OK)
                validationFilePath = fdlg.FileName;

            for (int i = 0; i < trainFilePath.Length; i++)
                GenericMLHelper.TranslateTrainPredictAndValidate(models.ToArray(), testFilePath, trainFilePath[i], validationFilePath, stem);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.7, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.6, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.5, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.25, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.15, 3));
            TranslateTrainValidatePredict(models, false);
        }

        private void bowTranslateAndPredict_Click(object sender, EventArgs e)
        {
            List<IModelClassification<Dictionary<string, double>>> models = new List<IModelClassification<Dictionary<string, double>>>();
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.7, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.6, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.5, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.25, 3));
            models.Add(new BagOfWords<string>(4, 4, 2000, 0.15, 3));
            TranslateTrainAndPredict(models, false);
        }
    }
}
