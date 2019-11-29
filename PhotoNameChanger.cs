using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PhotoNameChangerFinal
{
    class PhotoNameChanger
    {
        //Delete all files in the destination folder
        public static void DeleteRenamedFiles(string destinationPath)
        {
            DirectoryInfo dir = new DirectoryInfo(destinationPath);
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

        }

        //Returns all file name in directories and sub dierectories without extension
        public static string[] GetFileNames(string[] filePaths, string[] result)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                string entry = Path.GetFileNameWithoutExtension(filePaths[i]);
                Console.WriteLine("File Name {0}: {1}", i, entry);

                result[i] = entry.ToString();
            }

            return result;
        }

        //Delete records from CSWebPhotosTable
        public static void DeleteCSWebPhotoTable(SqlConnectionStringBuilder builder)
        {
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();

                string deleteQuery = "DELETE FROM [CSWebPhotos]";
                try
                {
                    using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }
                }
                catch (SystemException ex)
                {
                    Console.WriteLine(string.Format("An error occurred: {0}", ex.Message));
                }
            }
        }

        // Insert data in CSWebPhotosTable
        public static void PopulateWebPhotoName(SqlConnectionStringBuilder builder, string[] filePaths, string[] result)
        {
            string pattern = @"^.*?(?=_)";
            //string[] fileNames = GetFileNames(filePaths, result);

            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {

                conn.Open();

                string insertQuery = "INSERT INTO [CSWEbPhotos] ([PhotoName], [PhotoNameWithoutExt]) VALUES (@p, @c)";
                Console.WriteLine("Populating CSWebPhotos Table...");
                SqlCommand insertCommand;

                Match pCode;

                for (int i = 0; i < result.Length; i++)
                {
                    insertCommand = new SqlCommand(insertQuery, conn);
                    insertCommand.Parameters.Add(new SqlParameter("p", result[i]));
                    pCode = Regex.Match(result[i], pattern);
                    if (!pCode.Success)
                    {
                        insertCommand.Parameters.Add(new SqlParameter("c", result[i]));
                    }
                    else
                    {
                        insertCommand.Parameters.Add(new SqlParameter("c", pCode.ToString()));
                    }

                    insertCommand.ExecuteNonQuery();

                }

            }
        }

        //Get all items and their photo names
        public static void CheckItemPhotos(SqlConnectionStringBuilder builder)
        {
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                //string joinQuery = "SELECT [Item_GID], [Item_Code], [PhotoName]  FROM [dbo].[CSWebItemsTable] LEFT JOIN [CSWebPhotos] ON [dbo].[CSWebItemsTable].[Item_Code] = [CSWebPhotos].[PhotoNameWithoutExt] ORDER BY [PhotoName]"; //WHERE [PhotoName] IS NOT NULL";
                string joinQuery = "SELECT  [idExternal], [PhotoName] , [Code]  FROM [dbo].[ecmProducts] LEFT JOIN [CSWebPhotos] ON [dbo].[ecmProducts].[Code] = [CSWebPhotos].[PhotoNameWithoutExt] ORDER BY [PhotoName]"; //WHERE [PhotoName] IS NOT NULL";


                SqlCommand joinCommand = new SqlCommand(joinQuery, conn);

                string reader1 = "";
                string reader2 = "";

                SqlDataAdapter custAdapter = new SqlDataAdapter(joinQuery, conn);

                DataSet photos = new DataSet();

                custAdapter.Fill(photos, "CSWebItemPhoto");
                using (StreamWriter writer = new StreamWriter(@"" + ConfigurationManager.AppSettings.Get("dataLogFilePath") + ""))
                {
                    foreach (DataRow pRow in photos.Tables["CSWebItemPhoto"].Rows)
                    {
                        if (pRow["PhotoName"] == DBNull.Value)
                        {
                            pRow["PhotoName"] = "NULL";
                        }
                        Console.WriteLine(pRow["idExternal"].ToString() + " " + pRow["Code"].ToString() + " " + pRow["PhotoName"].ToString());
                        writer.WriteLine(pRow["idExternal"].ToString() + " " + pRow["Code"].ToString() + " " + pRow["PhotoName"].ToString());
                    }
                }
            }
        }

        //Rename files and copy new file to destination folder
        public static void RenameFiles(SqlConnectionStringBuilder builder, string[] result, string[] newNames, Match match, string destinationPath, string[] filePaths)
        {
            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {

                //string joinQuery = "SELECT [Item_GID], [PhotoName] FROM [dbo].[CSWebItemsTable] LEFT JOIN [CSWebPhotos] ON [dbo].[CSWebItemsTable].[Item_Code] = [CSWebPhotos].[PhotoNameWithoutExt] WHERE [PhotoName] IS NOT NULL ORDER BY [PhotoName] ";
                string joinQuery = "SELECT [idExternal], [PhotoName], [Code] FROM [dbo].[ecmProducts] LEFT JOIN [CSWebPhotos] ON [dbo].[ecmProducts].[Code] = [CSWebPhotos].[PhotoNameWithoutExt] WHERE [PhotoName] IS NOT NULL ORDER BY [PhotoName] ";


                SqlCommand joinCommand = new SqlCommand(joinQuery, conn);
                conn.Open();

                SqlDataReader reader = joinCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    string pattern = "[a-z]*[0-9]*-*[a-z]*[0-9]*_";
                    string pat = @"[^_]*";
                    string pattern1 = @"^.*?(?=_)";
                    string name = "";

                    string sourceFullPath;

                    string destinationFullPath;

                    int i = 0;

                    Regex r = new Regex(pat);

                    while (reader.Read())
                    {


                        Console.WriteLine("Reader " + i + " : " + reader[0].ToString());

                        match = Regex.Match(reader[1].ToString(), pattern1);

                        if (match.Success)
                        {
                            int j = 0;
                            foreach (string res in result)
                            {
                                if (reader[1].ToString() == res)
                                {
                                    sourceFullPath = filePaths[j];

                                    name = Regex.Replace(res, pattern, reader[0].ToString() + "_");

                                    Console.WriteLine("Result: {0} Source Path: {1}", res, sourceFullPath);
                                    newNames[i] = name + ".jpg";

                                    destinationFullPath = destinationPath + newNames[i];


                                    try
                                    {
                                        File.Copy(sourceFullPath, destinationFullPath);
                                        Console.WriteLine("{0} was moved to {1}.", sourceFullPath, destinationFullPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!Directory.Exists(Path.GetDirectoryName(sourceFullPath)))
                                        {
                                            Console.WriteLine("filePath does not exist: " + sourceFullPath);
                                        }

                                        if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                                        {

                                            DirectoryInfo di = Directory.CreateDirectory(destinationPath);
                                            Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(destinationPath));
                                            //Console.WriteLine("newFilePath does not exist: " + fp);
                                        }
                                    }
                                }
                                j++;
                            }

                        }
                        else
                        {
                            int n = 0;
                            foreach (string res in result)
                            {
                                if (reader[1].ToString() == res)
                                {
                                    sourceFullPath = filePaths[n];

                                    name = reader[0].ToString();
                                    //Console.WriteLine("Result {0}: new name: {1}, result name: {2}, name: {3}", i, reader[0].ToString(), res, name);
                                    newNames[i] = name + ".jpg";

                                    destinationFullPath = destinationPath + newNames[i];


                                    try
                                    {
                                        File.Copy(sourceFullPath, destinationFullPath);
                                        Console.WriteLine("{0} was moved to {1}.", sourceFullPath, destinationFullPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!Directory.Exists(Path.GetDirectoryName(sourceFullPath)))
                                        {
                                            Console.WriteLine("filePath does not exist: " + sourceFullPath);
                                        }

                                        if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                                        {

                                            DirectoryInfo di = Directory.CreateDirectory(destinationPath);
                                            Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(destinationPath));
                                            //Console.WriteLine("newFilePath does not exist: " + fp);
                                        }
                                    }
                                }
                                n++;
                            }
                        }

                        i++;
                    }



                    reader.Close();
                }

            }
        }
    }
}
