using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

class FileReader
{
    private StreamReader _reader;
    public int Current { get; private set; }
    private bool _hasNext;

    public FileReader(string filePath)
    {
        _reader = new StreamReader(filePath);
        _hasNext = MoveNext();
    }

    public bool HasNext() => _hasNext;

    public bool MoveNext()
    {
        string line = _reader.ReadLine();
        if (line != null)
        {
            Current = int.Parse(line);
            return true;
        }
        else
        {
            _hasNext = false;
            return false;
        }
    }
    public void Close()
    {
        _reader.Close();
    }
}
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        string filepath = "randomNumbers.txt";
        string temporaryFolder = "B";

        CreateFile(filepath, 100);
        Console.WriteLine("файл создан");

        Stopwatch sw = new Stopwatch();
        sw.Start();


        DivideNumbersIntoFiles(filepath, 10, temporaryFolder);

        SortFileContents(temporaryFolder);

        MergeFiles(temporaryFolder, "output.txt");

        //ProcessFiles(temporaryFolder, filepath);

        sw.Stop();

        Console.WriteLine("time: {0} seconds", sw.Elapsed.TotalSeconds);
    }

    static void ProcessFiles(string folderPath, string finalResultPath)
    {
        string[] fileName = Directory.GetFiles(folderPath);

        int seria = 1;

        int multipy = fileName.Length;

        while (fileName.Length != 1)
        {
            fileName = Directory.GetFiles(folderPath);
            List<StreamReader> readers = new List<StreamReader>();

            for (int i = 0; i < fileName.Length; i++)
            {
                readers.Add(new StreamReader(fileName[i]));
            }

            List<(int?, int)> values = new List<(int?, int)>();
            for (int i = 0; i < fileName.Length; i++)
            {
                values.Add((null, 0));
            }

            int index = 0;

            while (readers.Count != 0)
            {
                if (index == fileName.Length)
                    index = 0;

                StreamWriter writer = new StreamWriter(Path.Combine(folderPath, $"C{index + 1}.txt"), append:true);
                
                for (int i = 0; i < values.Count; i++)
                {
                    if (int.TryParse(readers[i].ReadLine(), out int value))
                    {
                        values[i] = (value, 1);
                    }
                    else
                    {
                        readers[i].Close();
                        readers.RemoveAt(i);
                        values.RemoveAt(i);
                    }
                }
                EndedSeria(readers, writer, values, seria);
                writer.Close();
                index++;

            }

            seria *= multipy;

            for (int i = 0; i < readers.Count; i++)
            {
                readers[i].Close();
            }

            for (int i = 0; i < fileName.Length; i++)
            {
                File.Delete(fileName[i]);
            }

            string[] filesToRename = Directory.GetFiles(folderPath, "C*");

            for (int i = 0; i < filesToRename.Length; i++)
            {
                string newFileName = "B" + Path.GetFileName(filesToRename[i]).Substring(1);
                string newFilePath = Path.Combine(folderPath, newFileName);

                File.Move(filesToRename[i], newFilePath);
            }
        }
    }

    public static bool EndedSeria(List<StreamReader> readers, StreamWriter writer, List<(int?, int)> values, int seria) 
    {
        try
        {
            if (readers.Count == 0)
                return true;

            while (values.Count(x => x.Item1 == null) != values.Count)
            {
                int? min = values.Min(x => x.Item1);
                int index = values.FindIndex(x => x.Item1 == min);
                values[index] = (null, values[index].Item2 + 1);
                writer.WriteLine(min.ToString());

                if (values[index].Item2 <= seria)
                {
                    if (int.TryParse(readers[index].ReadLine(), out int value))
                    {
                        values[index] = (value, values[index].Item2);
                    }
                    else
                    {
                        readers[index].Close();
                        readers.RemoveAt(index);
                        values.RemoveAt(index);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return false;
    }

    public static void SortFileContents(string filepath)
    {
        string[] files = Directory.GetFiles(filepath);

        foreach (string file in files)
        {
            List<int> values = new List<int>();
            using (StreamReader reader = new StreamReader(file))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    values.Add(int.Parse(line));
                }

                values.Sort();
            }
            WriteAllLines(file, values);
            
        }
    }

    public static void WriteAllLines(string file, List<int> values) 
    {
        using (StreamWriter writer = new StreamWriter(file))
        {
            foreach (var value in values)
            {
                writer.WriteLine(value);
            }
        }
    }

    public static void CreateFile(string filePath, int size)
    {
        long fileSize = (1024 * 1024) * size;

        Random random = new Random();

        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(fs))
        {
            long currentSize = 0;

            while (currentSize < fileSize)
            {
                int randomNumber = random.Next(10001);
                string numberString = randomNumber.ToString() + Environment.NewLine;

                int byteCount = System.Text.Encoding.UTF8.GetByteCount(numberString);

                if (currentSize + byteCount > fileSize)
                {
                    break;
                }

                writer.Write(numberString);
                currentSize += byteCount;
            }
        }

        Console.WriteLine("Файл создан: " + filePath);
    }

    public static void DivideNumbersIntoFiles(string inputFilePath, int countOfFiles, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        StreamWriter[] writers = new StreamWriter[countOfFiles];

        for (int i = 0; i < countOfFiles; i++) 
        {
            string fileName = Path.Combine(outputDirectory, $"B{i + 1}.txt");
            writers[i] = new StreamWriter(fileName);
        }

        using (StreamReader reader = new StreamReader(inputFilePath)) 
        {
            int index = 0;
            string line;

            while ((line = reader.ReadLine()) != null) 
            {
                writers[index++].WriteLine(line);

                if (index == countOfFiles)
                    index = 0;
            }
        }

        for (int i = 0; i < countOfFiles; i++)
        {
            writers[i].Close();
            writers[i].Dispose();
        }

    }

    public static void MergeFiles(string directory, string outputFilePath)
    {
        string[] files = Directory.GetFiles(directory);
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            PriorityQueue<FileReader, int> pq = new PriorityQueue<FileReader, int>();

            foreach (var file in files)
            {
                var reader = new FileReader(file);
                if (reader.HasNext())
                {
                    pq.Enqueue(reader, reader.Current);
                }
            }

            while (pq.Count > 0)
            {
                var minReader = pq.Dequeue();
                writer.WriteLine(minReader.Current);

                if (minReader.MoveNext())
                {
                    pq.Enqueue(minReader, minReader.Current);
                }
                else
                {
                    minReader.Close();
                }
            }
        }

        foreach (var file in files) 
        {
            File.Delete(file);
        }
    }
}