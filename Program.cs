using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Compression;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0].Length != 0 & args[1].Length != 0 & args[2].Length != 0)
            {
                if (File.Exists(args[1]) & !File.Exists(args[2]))
                {


                    try
                    {
                        string SourceFile = args[1];
                        int nNoofFiles = 1024;
                        Thread[] threads = new Thread[nNoofFiles];
                        FileStream fs = new FileStream(SourceFile, FileMode.Open, FileAccess.Read);
                        FileStream outfile = new FileStream(args[2], FileMode.OpenOrCreate, FileAccess.Write);
                        GZipStream compressstream = new GZipStream(outfile, CompressionMode.Compress);
                        GZipStream decompressstream = new GZipStream(fs, CompressionMode.Decompress);

                        Console.WriteLine("file is being processed...");
                        int SizeofEachFile = (int)Math.Ceiling((double)fs.Length / nNoofFiles);
                        for (int i = 0; i < nNoofFiles; i++)
                        {


                            byte[] buffer = new byte[SizeofEachFile];

                            if (args[0] == "compress")
                            {
                                int bytesRead = 0;
                                if ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                                {




                                    Chunk chunk = new Chunk(buffer, bytesRead, compressstream);
                                    threads[i] = new Thread(() => chunk.Compress())
                                    {

                                        Priority = ThreadPriority.Highest
                                    };
                                    threads[i].Start();


                                }
                            }
                            else if (args[0] == "decompress")
                            {
                                int bytesReaded;
                                if ((bytesReaded = decompressstream.Read(buffer, 0, SizeofEachFile)) > 0)
                                {
                                    Chunk chunk = new Chunk(buffer, bytesReaded, outfile);
                                    threads[i] = new Thread(() => chunk.Decompress())
                                    {

                                        Priority = ThreadPriority.Highest
                                    };
                                    threads[i].Start();
                                }
                            }
                            else
                            {
                                Console.Write("Command doesn't exist");
                                break;
                            }
                            Thread.Sleep(330);
                        }

                        Console.WriteLine("Done");
                        decompressstream.Close();
                        compressstream.Close();
                        outfile.Close();
                        fs.Close();


                    }


                    catch (InvalidDataException)
                    {
                        Console.WriteLine("Error: The file being read contains invalid data.");
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("Error:The file specified was not found.");
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Error: path is a zero-length string, contains only white space, or contains one or more invalid characters");
                    }                
                    catch (IOException)
                    {
                        Console.WriteLine("Error: An I/O error occurred while opening the file.");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Error: path specified a file that is read-only, the path is a directory, or caller does not have the required permissions.");
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Error: You must provide parameters for MyGZIP.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error:File too big");
                    }
                } else { Console.WriteLine("Error:File doesn't exist or output file does");  }
            }
            else { Console.WriteLine("Error:Command is not right"); }
            Console.ReadLine();

        }
    }

    class Chunk
    {
        static object locker = new object();
        Semaphore sem = new Semaphore(2,2);
        

        private byte[] buffer;
        private int bytesRead;
        private GZipStream gZip;
        private FileStream outputfile;

        public Chunk(byte[] buff, int bytRead, GZipStream gZipStream) {
            buffer = buff;
            bytesRead = bytRead;
            gZip = gZipStream;
        }
        public Chunk(byte[] buff, int bytRead, FileStream outf)
        {
            buffer = buff;
            bytesRead = bytRead;
            outputfile = outf;
        }

        public void Compress()
        {

            lock (locker)
            {
                sem.WaitOne();
                gZip.Write(buffer, 0, bytesRead);
                Console.Write("-");
                sem.Release();
            }

        }

        public void Decompress( )
        {
            lock (locker)
            {
                sem.WaitOne();

                outputfile.Write(buffer, 0, bytesRead);
                Console.Write("-");
                sem.Release();
            }

           
        }
    }

}
