using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeGeneratorAI
{
	public class Matrix
	{
		public double[,] Data;
		public int Rows;
		public int Cols;

		public Matrix(int rows, int cols)
		{
			Rows = rows;
			Cols = cols;
			Data = new double[rows, cols];
		}

		public void RandomInit(Random rand)
		{
			double scale = Math.Sqrt(2.0 / (Rows + Cols));
			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < Cols; j++)
					Data[i, j] = (rand.NextDouble() * 2 - 1) * scale;
		}

		public static Matrix Multiply(Matrix a, Matrix b)
		{
			Matrix result = new Matrix(a.Rows, b.Cols);
			for (int i = 0; i < a.Rows; i++)
				for (int j = 0; j < b.Cols; j++)
					for (int k = 0; k < a.Cols; k++)
						result.Data[i, j] += a.Data[i, k] * b.Data[k, j];
			return result;
		}

		public static Matrix HadamardProduct(Matrix a, Matrix b)
		{
			Matrix result = new Matrix(a.Rows, a.Cols);
			for (int i = 0; i < a.Rows; i++)
				for (int j = 0; j < a.Cols; j++)
					result.Data[i, j] = a.Data[i, j] * b.Data[i, j];
			return result;
		}

		public static Matrix Add(Matrix a, Matrix b)
		{
			Matrix result = new Matrix(a.Rows, a.Cols);
			for (int i = 0; i < a.Rows; i++)
				for (int j = 0; j < a.Cols; j++)
					result.Data[i, j] = a.Data[i, j] + b.Data[i, j];
			return result;
		}

		public Matrix Transpose()
		{
			Matrix result = new Matrix(Cols, Rows);
			for (int i = 0; i < Rows; i++)
				for (int j = 0; j < Cols; j++)
					result.Data[j, i] = Data[i, j];
			return result;
		}

		public Matrix Clone()
		{
			Matrix result = new Matrix(Rows, Cols);
			Array.Copy(Data, result.Data, Data.Length);
			return result;
		}
	}

	public static class Activation
	{
		public static Matrix Sigmoid(Matrix m)
		{
			Matrix result = new Matrix(m.Rows, m.Cols);
			for (int i = 0; i < m.Rows; i++)
				for (int j = 0; j < m.Cols; j++)
					result.Data[i, j] = 1.0 / (1.0 + Math.Exp(-m.Data[i, j]));
			return result;
		}

		public static Matrix SigmoidDerivative(Matrix sigmoidOutput)
		{
			Matrix result = new Matrix(sigmoidOutput.Rows, sigmoidOutput.Cols);
			for (int i = 0; i < sigmoidOutput.Rows; i++)
				for (int j = 0; j < sigmoidOutput.Cols; j++)
				{
					double s = sigmoidOutput.Data[i, j];
					result.Data[i, j] = s * (1 - s);
				}
			return result;
		}

		public static Matrix Tanh(Matrix m)
		{
			Matrix result = new Matrix(m.Rows, m.Cols);
			for (int i = 0; i < m.Rows; i++)
				for (int j = 0; j < m.Cols; j++)
					result.Data[i, j] = Math.Tanh(m.Data[i, j]);
			return result;
		}

		public static Matrix TanhDerivative(Matrix tanhOutput)
		{
			Matrix result = new Matrix(tanhOutput.Rows, tanhOutput.Cols);
			for (int i = 0; i < tanhOutput.Rows; i++)
				for (int j = 0; j < tanhOutput.Cols; j++)
				{
					double t = tanhOutput.Data[i, j];
					result.Data[i, j] = 1 - t * t;
				}
			return result;
		}

		public static Matrix Softmax(Matrix m)
		{
			Matrix result = new Matrix(m.Rows, m.Cols);
			double max = double.MinValue;
			for (int i = 0; i < m.Rows; i++)
				if (m.Data[i, 0] > max) max = m.Data[i, 0];

			double sum = 0;
			for (int i = 0; i < m.Rows; i++)
			{
				result.Data[i, 0] = Math.Exp(m.Data[i, 0] - max);
				sum += result.Data[i, 0];
			}

			for (int i = 0; i < m.Rows; i++)
				result.Data[i, 0] /= sum;

			return result;
		}
	}

	public class DataPreprocessor
	{
		public Dictionary<char, int> CharToIndex = new Dictionary<char, int>();
		public Dictionary<int, char> IndexToChar = new Dictionary<int, char>();
		public int VocabSize;

		public void BuildVocabulary(string text)
		{
			var uniqueChars = text.Distinct().OrderBy(c => c).ToList();
			VocabSize = uniqueChars.Count;

			for (int i = 0; i < uniqueChars.Count; i++)
			{
				CharToIndex[uniqueChars[i]] = i;
				IndexToChar[i] = uniqueChars[i];
			}

			Console.WriteLine($"Vocabulary Size: {VocabSize}");
		}

		public Matrix CharToOneHot(char c)
		{
			Matrix oneHot = new Matrix(VocabSize, 1);
			if (CharToIndex.ContainsKey(c))
				oneHot.Data[CharToIndex[c], 0] = 1.0;
			return oneHot;
		}
	}

	public class CodeGeneratorModel
	{
		public int InputSize;
		public int HiddenSize;
		public int OutputSize;
		public double LearningRate = 0.01;

		public Matrix Wf, Wi, Wc, Wo;
		public Matrix Uf, Ui, Uc, Uo;
		public Matrix Bf, Bi, Bc, Bo;
		public Matrix Wy, By;

		public DataPreprocessor Preprocessor;

		public CodeGeneratorModel(int hiddenSize)
		{
			HiddenSize = hiddenSize;
			Preprocessor = new DataPreprocessor();
		}

		public void Initialize()
		{
			Random rand = new Random(42);
			InputSize = Preprocessor.VocabSize;
			OutputSize = Preprocessor.VocabSize;

			Wf = new Matrix(HiddenSize, InputSize); Wf.RandomInit(rand);
			Wi = new Matrix(HiddenSize, InputSize); Wi.RandomInit(rand);
			Wc = new Matrix(HiddenSize, InputSize); Wc.RandomInit(rand);
			Wo = new Matrix(HiddenSize, InputSize); Wo.RandomInit(rand);

			Uf = new Matrix(HiddenSize, HiddenSize); Uf.RandomInit(rand);
			Ui = new Matrix(HiddenSize, HiddenSize); Ui.RandomInit(rand);
			Uc = new Matrix(HiddenSize, HiddenSize); Uc.RandomInit(rand);
			Uo = new Matrix(HiddenSize, HiddenSize); Uo.RandomInit(rand);

			Bf = new Matrix(HiddenSize, 1);
			Bi = new Matrix(HiddenSize, 1);
			Bc = new Matrix(HiddenSize, 1);
			Bo = new Matrix(HiddenSize, 1);

			Wy = new Matrix(OutputSize, HiddenSize); Wy.RandomInit(rand);
			By = new Matrix(OutputSize, 1);
		}

		public void Train(string text, int epochs, int sequenceLength = 25)
		{
			Console.Clear();
			Console.WriteLine("=============================================================");
			Console.WriteLine("         C# Code Generator AI - Training Started           ");
			Console.WriteLine("=============================================================\n");

			Console.WriteLine($"[INFO] Text length: {text.Length} characters");
			Console.WriteLine($"[INFO] Sequence length: {sequenceLength}");

			List<double> lossHistory = new List<double>();
			DateTime startTime = DateTime.Now;

			int totalBatches = (text.Length - sequenceLength - 1) / sequenceLength;
			Console.WriteLine($"[INFO] Total batches per epoch: {totalBatches}");
			Console.WriteLine($"[INFO] Starting training...\n");

			for (int epoch = 0; epoch < epochs; epoch++)
			{
				double totalLoss = 0;
				int numBatches = 0;

				for (int i = 0; i < text.Length - sequenceLength - 1; i += sequenceLength)
				{
					string inputSeq = text.Substring(i, sequenceLength);
					string targetSeq = text.Substring(i + 1, sequenceLength);

					double loss = TrainSequence(inputSeq, targetSeq);
					totalLoss += loss;
					numBatches++;

					if (epoch == 0 && numBatches == 1)
						Console.WriteLine($"[INFO] First batch completed! Loss: {loss:F4}");

					if (numBatches % Math.Max(1, totalBatches / 20) == 0)
						DrawProgressBar(numBatches, totalBatches, epoch, epochs);
				}

				double avgLoss = totalLoss / Math.Max(1, numBatches);
				lossHistory.Add(avgLoss);

				Console.SetCursorPosition(0, 8);
				Console.WriteLine($"Epoch {epoch + 1}/{epochs} | Loss: {avgLoss:F4} | Avg: {lossHistory.Average():F4} | Min: {lossHistory.Min():F4}     ");

				TimeSpan elapsed = DateTime.Now - startTime;
				TimeSpan estimated = TimeSpan.FromSeconds(elapsed.TotalSeconds / (epoch + 1) * epochs);
				Console.WriteLine($"Time: {elapsed:hh\\:mm\\:ss} | Est: {(estimated - elapsed):hh\\:mm\\:ss}      ");

				if ((epoch + 1) % 10 == 0)
				{
					SaveModel($"model_epoch_{epoch + 1}.dat");
				}

				if (epoch % 10 == 0 || epoch == epochs - 1)
				{
					Console.WriteLine("\n--- Generated Sample ---");
					string sample = Generate("public class", 100);
					Console.WriteLine(sample.Substring(0, Math.Min(150, sample.Length)));
					Console.WriteLine("------------------------\n");
				}
			}

			SaveModel("model_final.dat");

			Console.WriteLine("\n=============================================================");
			Console.WriteLine("            Training Complete! Model is Ready!             ");
			Console.WriteLine("=============================================================\n");
		}

		private double TrainSequence(string input, string target)
		{
			int seqLen = input.Length;

			List<Matrix> xs = new List<Matrix>();
			List<Matrix> hs = new List<Matrix>();
			List<Matrix> cs = new List<Matrix>();
			List<Matrix> fs = new List<Matrix>();
			List<Matrix> iGates = new List<Matrix>();
			List<Matrix> cTildes = new List<Matrix>();
			List<Matrix> os = new List<Matrix>();
			List<Matrix> ys = new List<Matrix>();

			Matrix h = new Matrix(HiddenSize, 1);
			Matrix c = new Matrix(HiddenSize, 1);

			hs.Add(h.Clone());
			cs.Add(c.Clone());

			double loss = 0;

			for (int t = 0; t < seqLen; t++)
			{
				Matrix x = Preprocessor.CharToOneHot(input[t]);
				xs.Add(x);

				Matrix ft = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wf, x), Matrix.Multiply(Uf, h)), Bf));
				Matrix it = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wi, x), Matrix.Multiply(Ui, h)), Bi));
				Matrix cTilde = Activation.Tanh(Matrix.Add(Matrix.Add(Matrix.Multiply(Wc, x), Matrix.Multiply(Uc, h)), Bc));
				Matrix ot = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wo, x), Matrix.Multiply(Uo, h)), Bo));

				c = Matrix.Add(Matrix.HadamardProduct(ft, c), Matrix.HadamardProduct(it, cTilde));
				h = Matrix.HadamardProduct(ot, Activation.Tanh(c));

				Matrix y = Activation.Softmax(Matrix.Add(Matrix.Multiply(Wy, h), By));

				fs.Add(ft);
				iGates.Add(it);
				cTildes.Add(cTilde);
				os.Add(ot);
				hs.Add(h.Clone());
				cs.Add(c.Clone());
				ys.Add(y);

				int targetIdx = Preprocessor.CharToIndex[target[t]];
				loss += -Math.Log(y.Data[targetIdx, 0] + 1e-8);
			}

			Matrix dhnext = new Matrix(HiddenSize, 1);
			Matrix dcnext = new Matrix(HiddenSize, 1);

			Matrix dWf = new Matrix(HiddenSize, InputSize);
			Matrix dWi = new Matrix(HiddenSize, InputSize);
			Matrix dWc = new Matrix(HiddenSize, InputSize);
			Matrix dWo = new Matrix(HiddenSize, InputSize);
			Matrix dUf = new Matrix(HiddenSize, HiddenSize);
			Matrix dUi = new Matrix(HiddenSize, HiddenSize);
			Matrix dUc = new Matrix(HiddenSize, HiddenSize);
			Matrix dUo = new Matrix(HiddenSize, HiddenSize);
			Matrix dBf = new Matrix(HiddenSize, 1);
			Matrix dBi = new Matrix(HiddenSize, 1);
			Matrix dBc = new Matrix(HiddenSize, 1);
			Matrix dBo = new Matrix(HiddenSize, 1);
			Matrix dWy = new Matrix(OutputSize, HiddenSize);
			Matrix dBy = new Matrix(OutputSize, 1);

			for (int t = seqLen - 1; t >= 0; t--)
			{
				Matrix dy = ys[t].Clone();
				int targetIdx = Preprocessor.CharToIndex[target[t]];
				dy.Data[targetIdx, 0] -= 1;

				dWy = Matrix.Add(dWy, Matrix.Multiply(dy, hs[t + 1].Transpose()));
				dBy = Matrix.Add(dBy, dy);

				Matrix dh = Matrix.Add(Matrix.Multiply(Wy.Transpose(), dy), dhnext);

				Matrix dtanhc = Matrix.HadamardProduct(dh, os[t]);
				Matrix dc = Matrix.Add(Matrix.HadamardProduct(dtanhc, Activation.TanhDerivative(Activation.Tanh(cs[t + 1]))), dcnext);

				Matrix dot = Matrix.HadamardProduct(dh, Activation.Tanh(cs[t + 1]));
				dot = Matrix.HadamardProduct(dot, Activation.SigmoidDerivative(os[t]));

				Matrix dcTilde = Matrix.HadamardProduct(dc, iGates[t]);
				dcTilde = Matrix.HadamardProduct(dcTilde, Activation.TanhDerivative(cTildes[t]));

				Matrix dit = Matrix.HadamardProduct(dc, cTildes[t]);
				dit = Matrix.HadamardProduct(dit, Activation.SigmoidDerivative(iGates[t]));

				Matrix dft = Matrix.HadamardProduct(dc, cs[t]);
				dft = Matrix.HadamardProduct(dft, Activation.SigmoidDerivative(fs[t]));

				dWf = Matrix.Add(dWf, Matrix.Multiply(dft, xs[t].Transpose()));
				dWi = Matrix.Add(dWi, Matrix.Multiply(dit, xs[t].Transpose()));
				dWc = Matrix.Add(dWc, Matrix.Multiply(dcTilde, xs[t].Transpose()));
				dWo = Matrix.Add(dWo, Matrix.Multiply(dot, xs[t].Transpose()));

				dUf = Matrix.Add(dUf, Matrix.Multiply(dft, hs[t].Transpose()));
				dUi = Matrix.Add(dUi, Matrix.Multiply(dit, hs[t].Transpose()));
				dUc = Matrix.Add(dUc, Matrix.Multiply(dcTilde, hs[t].Transpose()));
				dUo = Matrix.Add(dUo, Matrix.Multiply(dot, hs[t].Transpose()));

				dBf = Matrix.Add(dBf, dft);
				dBi = Matrix.Add(dBi, dit);
				dBc = Matrix.Add(dBc, dcTilde);
				dBo = Matrix.Add(dBo, dot);

				dhnext = Matrix.Add(Matrix.Add(Matrix.Add(
					Matrix.Multiply(Uf.Transpose(), dft),
					Matrix.Multiply(Ui.Transpose(), dit)),
					Matrix.Multiply(Uc.Transpose(), dcTilde)),
					Matrix.Multiply(Uo.Transpose(), dot));

				dcnext = Matrix.HadamardProduct(dc, fs[t]);
			}

			ClipGradient(dWf, 5.0); ClipGradient(dWi, 5.0);
			ClipGradient(dWc, 5.0); ClipGradient(dWo, 5.0);
			ClipGradient(dUf, 5.0); ClipGradient(dUi, 5.0);
			ClipGradient(dUc, 5.0); ClipGradient(dUo, 5.0);
			ClipGradient(dWy, 5.0); ClipGradient(dBy, 5.0);

			UpdateWeights(Wf, dWf); UpdateWeights(Wi, dWi);
			UpdateWeights(Wc, dWc); UpdateWeights(Wo, dWo);
			UpdateWeights(Uf, dUf); UpdateWeights(Ui, dUi);
			UpdateWeights(Uc, dUc); UpdateWeights(Uo, dUo);
			UpdateWeights(Bf, dBf); UpdateWeights(Bi, dBi);
			UpdateWeights(Bc, dBc); UpdateWeights(Bo, dBo);
			UpdateWeights(Wy, dWy); UpdateWeights(By, dBy);

			return loss / seqLen;
		}

		private void ClipGradient(Matrix grad, double maxValue)
		{
			for (int i = 0; i < grad.Rows; i++)
				for (int j = 0; j < grad.Cols; j++)
					grad.Data[i, j] = Math.Max(-maxValue, Math.Min(maxValue, grad.Data[i, j]));
		}

		private void UpdateWeights(Matrix weight, Matrix gradient)
		{
			for (int i = 0; i < weight.Rows; i++)
				for (int j = 0; j < weight.Cols; j++)
					weight.Data[i, j] -= LearningRate * gradient.Data[i, j];
		}

		public string Generate(string seed, int length)
		{
			StringBuilder result = new StringBuilder(seed);
			Matrix h = new Matrix(HiddenSize, 1);
			Matrix c = new Matrix(HiddenSize, 1);

			foreach (char ch in seed)
			{
				if (Preprocessor.CharToIndex.ContainsKey(ch))
				{
					Matrix x = Preprocessor.CharToOneHot(ch);

					Matrix ft = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wf, x), Matrix.Multiply(Uf, h)), Bf));
					Matrix it = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wi, x), Matrix.Multiply(Ui, h)), Bi));
					Matrix cTilde = Activation.Tanh(Matrix.Add(Matrix.Add(Matrix.Multiply(Wc, x), Matrix.Multiply(Uc, h)), Bc));
					Matrix ot = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wo, x), Matrix.Multiply(Uo, h)), Bo));

					c = Matrix.Add(Matrix.HadamardProduct(ft, c), Matrix.HadamardProduct(it, cTilde));
					h = Matrix.HadamardProduct(ot, Activation.Tanh(c));
				}
			}

			Random rand = new Random();
			for (int i = 0; i < length; i++)
			{
				Matrix x = Preprocessor.CharToOneHot(result[result.Length - 1]);

				Matrix ft = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wf, x), Matrix.Multiply(Uf, h)), Bf));
				Matrix it = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wi, x), Matrix.Multiply(Ui, h)), Bi));
				Matrix cTilde = Activation.Tanh(Matrix.Add(Matrix.Add(Matrix.Multiply(Wc, x), Matrix.Multiply(Uc, h)), Bc));
				Matrix ot = Activation.Sigmoid(Matrix.Add(Matrix.Add(Matrix.Multiply(Wo, x), Matrix.Multiply(Uo, h)), Bo));

				c = Matrix.Add(Matrix.HadamardProduct(ft, c), Matrix.HadamardProduct(it, cTilde));
				h = Matrix.HadamardProduct(ot, Activation.Tanh(c));

				Matrix y = Activation.Softmax(Matrix.Add(Matrix.Multiply(Wy, h), By));

				int sampledIdx = SampleFromDistribution(y, 0.8, rand);
				char nextChar = Preprocessor.IndexToChar[sampledIdx];
				result.Append(nextChar);
			}

			return result.ToString();
		}

		private int SampleFromDistribution(Matrix probs, double temperature, Random rand)
		{
			double[] adjustedProbs = new double[probs.Rows];
			double sum = 0;

			for (int i = 0; i < probs.Rows; i++)
			{
				adjustedProbs[i] = Math.Pow(probs.Data[i, 0], 1.0 / temperature);
				sum += adjustedProbs[i];
			}

			for (int i = 0; i < adjustedProbs.Length; i++)
				adjustedProbs[i] /= sum;

			double r = rand.NextDouble();
			double cumulative = 0;

			for (int i = 0; i < adjustedProbs.Length; i++)
			{
				cumulative += adjustedProbs[i];
				if (r < cumulative)
					return i;
			}

			return adjustedProbs.Length - 1;
		}

		public void SaveModel(string filepath)
		{
			using (BinaryWriter writer = new BinaryWriter(File.Open(filepath, FileMode.Create)))
			{
				writer.Write(HiddenSize);
				writer.Write(InputSize);
				writer.Write(OutputSize);

				SaveMatrix(writer, Wf); SaveMatrix(writer, Wi);
				SaveMatrix(writer, Wc); SaveMatrix(writer, Wo);
				SaveMatrix(writer, Uf); SaveMatrix(writer, Ui);
				SaveMatrix(writer, Uc); SaveMatrix(writer, Uo);
				SaveMatrix(writer, Bf); SaveMatrix(writer, Bi);
				SaveMatrix(writer, Bc); SaveMatrix(writer, Bo);
				SaveMatrix(writer, Wy); SaveMatrix(writer, By);

				writer.Write(Preprocessor.VocabSize);
				foreach (var kvp in Preprocessor.CharToIndex)
				{
					writer.Write(kvp.Key);
					writer.Write(kvp.Value);
				}
			}
			Console.WriteLine($"Model saved to {filepath}");
		}

		public void LoadModel(string filepath)
		{
			using (BinaryReader reader = new BinaryReader(File.Open(filepath, FileMode.Open)))
			{
				HiddenSize = reader.ReadInt32();
				InputSize = reader.ReadInt32();
				OutputSize = reader.ReadInt32();

				Wf = LoadMatrix(reader); Wi = LoadMatrix(reader);
				Wc = LoadMatrix(reader); Wo = LoadMatrix(reader);
				Uf = LoadMatrix(reader); Ui = LoadMatrix(reader);
				Uc = LoadMatrix(reader); Uo = LoadMatrix(reader);
				Bf = LoadMatrix(reader); Bi = LoadMatrix(reader);
				Bc = LoadMatrix(reader); Bo = LoadMatrix(reader);
				Wy = LoadMatrix(reader); By = LoadMatrix(reader);

				int vocabSize = reader.ReadInt32();
				Preprocessor.VocabSize = vocabSize;
				Preprocessor.CharToIndex.Clear();
				Preprocessor.IndexToChar.Clear();

				for (int i = 0; i < vocabSize; i++)
				{
					char c = reader.ReadChar();
					int idx = reader.ReadInt32();
					Preprocessor.CharToIndex[c] = idx;
					Preprocessor.IndexToChar[idx] = c;
				}
			}
			Console.WriteLine($"Model loaded from {filepath}");
		}

		private void SaveMatrix(BinaryWriter writer, Matrix m)
		{
			writer.Write(m.Rows);
			writer.Write(m.Cols);
			for (int i = 0; i < m.Rows; i++)
				for (int j = 0; j < m.Cols; j++)
					writer.Write(m.Data[i, j]);
		}

		private Matrix LoadMatrix(BinaryReader reader)
		{
			int rows = reader.ReadInt32();
			int cols = reader.ReadInt32();
			Matrix m = new Matrix(rows, cols);
			for (int i = 0; i < rows; i++)
				for (int j = 0; j < cols; j++)
					m.Data[i, j] = reader.ReadDouble();
			return m;
		}

		private void DrawProgressBar(int current, int total, int epoch, int totalEpochs)
		{
			if (total <= 0) total = 1;

			int barWidth = 40;
			double percentage = (double)current / total;
			percentage = Math.Max(0.0, Math.Min(1.0, percentage));

			int progress = (int)(percentage * barWidth);
			progress = Math.Max(0, Math.Min(barWidth, progress));

			Console.SetCursorPosition(0, 6);
			Console.Write($"Epoch [{epoch + 1}/{totalEpochs}] [");
			Console.Write(new string('#', progress));
			Console.Write(new string('.', barWidth - progress));
			Console.Write($"] {percentage * 100:F1}%     ");
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("=== C# Code Generator AI ===\n");

			CodeGeneratorModel model = new CodeGeneratorModel(hiddenSize: 64);

			if (File.Exists("model_final.dat"))
			{
				Console.Write("Found saved model. Load it? (y/n): ");
				if (Console.ReadLine().ToLower() == "y")
				{
					model.LoadModel("model_final.dat");
					RunGenerationMode(model);
					return;
				}
			}

			string trainingFolder = @"C:\Develop\CsCodeGen\CsCodeGen\MyCodebase";

			if (!Directory.Exists(trainingFolder))
			{
				Console.WriteLine("Training folder not found. Using sample data.\n");
				string sampleCode = @"
public class SampleClass
{
    private int value;
    public SampleClass(int v) { value = v; }
    public int GetValue() { return value; }
    public void SetValue(int v) { value = v; }
}";
				model.Preprocessor.BuildVocabulary(sampleCode);
				model.Initialize();
				model.Train(sampleCode, epochs: 100, sequenceLength: 20);
			}
			else
			{
				string allCode = LoadAllCode(trainingFolder);

				Console.WriteLine($"[INFO] Total dataset size: {allCode.Length:N0} characters ({allCode.Length / 1024.0:F1} KB)");
				Console.WriteLine("\nSelect training data size:");
				Console.WriteLine("  1. 100 KB   (Quick test - 5-8 hours)");
				Console.WriteLine("  2. 500 KB   (Light training - 1-1.5 days)");
				Console.WriteLine("  3. 5 MB     (Serious training - 8-16 days)");
				Console.WriteLine("  4. Full dataset (Maximum quality - 40-80 days)");
				Console.WriteLine("  5. Custom size (specify in KB)");
				Console.Write("\nYour choice (1-5): ");

				string choice = Console.ReadLine();
				int targetSize = 0;

				switch (choice)
				{
					case "1":
						targetSize = 100 * 1024; // 100KB
						Console.WriteLine("[INFO] Using 100 KB for quick test.");
						break;
					case "2":
						targetSize = 500 * 1024; // 500KB
						Console.WriteLine("[INFO] Using 500 KB for light training.");
						break;
					case "3":
						targetSize = 5 * 1024 * 1024; // 5MB
						Console.WriteLine("[INFO] Using 5 MB for serious training.");
						break;
					case "4":
						targetSize = allCode.Length; // Full
						Console.WriteLine("[INFO] Using full dataset. This will take a LONG time!");
						break;
					case "5":
						Console.Write("Enter size in KB: ");
						if (int.TryParse(Console.ReadLine(), out int customKB))
						{
							targetSize = customKB * 1024;
							Console.WriteLine($"[INFO] Using custom size: {customKB} KB");
						}
						else
						{
							Console.WriteLine("[ERROR] Invalid input. Using 100 KB.");
							targetSize = 100 * 1024;
						}
						break;
					default:
						Console.WriteLine("[INFO] Invalid choice. Using 100 KB.");
						targetSize = 100 * 1024;
						break;
				}

				if (targetSize < allCode.Length)
				{
					allCode = allCode.Substring(0, Math.Min(targetSize, allCode.Length));
					Console.WriteLine($"[INFO] Training with {allCode.Length:N0} characters ({allCode.Length / 1024.0:F1} KB)");
				}

				model.Preprocessor.BuildVocabulary(allCode);
				model.Initialize();
				Console.WriteLine("[INFO] Auto-save every 10 epochs. Ctrl+C to stop anytime.\n");
				model.Train(allCode, epochs: 100, sequenceLength: 50);
			}

			RunGenerationMode(model);
		}

		static void RunGenerationMode(CodeGeneratorModel model)
		{
			Console.WriteLine("\n=== Code Generation Mode ===\n");

			while (true)
			{
				Console.Write("Prompt> ");
				string prompt = Console.ReadLine();

				if (prompt.ToLower() == "exit") break;

				Console.WriteLine("\nGenerating...\n");
				Console.WriteLine(model.Generate(prompt, 200));
				Console.WriteLine("\n" + new string('-', 50) + "\n");
			}

			Console.WriteLine("Goodbye!");
		}

		static string LoadAllCode(string folderPath)
		{
			StringBuilder allCode = new StringBuilder();
			foreach (var file in Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories))
			{
				allCode.Append(File.ReadAllText(file));
				allCode.Append("\n");
			}
			return allCode.ToString();
		}
	}
}
