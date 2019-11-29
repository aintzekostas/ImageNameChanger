using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace PhotoNameChangerFinal
{
    class Program
    {
        public static SqlConnectionStringBuilder DBConnection()
        {

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = ConfigurationManager.AppSettings.Get("DataSource");   // update me
            builder.InitialCatalog = ConfigurationManager.AppSettings.Get("InitialCatalog");
            builder.IntegratedSecurity = false;
            builder.UserID = ConfigurationManager.AppSettings.Get("UserID");               // update me
            builder.Password = ConfigurationManager.AppSettings.Get("Password");     // update me


            return builder;
        }


        static void Main(string[] args)
        {
            string spAttr;
            string npAttr;
            spAttr = ConfigurationManager.AppSettings.Get("source_directory");
            npAttr = ConfigurationManager.AppSettings.Get("newFilePath");

            string[] filePaths = Directory.GetFiles(@"" + spAttr, "*.jpg", SearchOption.AllDirectories);
            int i = 0;
            foreach(string fp in filePaths)
            {
                Console.WriteLine("File {0} Path: {1}", i, fp);
                i++;
            }
            //string sourcePath = @"C:\Users\blizz\Desktop\novalis_images\source_files";
            string newFilePath = npAttr;
            string[] result = new string[filePaths.Length];
            string[] newNames = new string[filePaths.Length];
            Match match = null;

            string dataSource;
            string dbUser;
            string password;
            string database;



            //string[,] oldPathsMap = new string[filePaths.Length, 2];
            //string[,] newPathsMap = new string[filePaths.Length, 2];

            // Read a particular key from the config file            


            if (ConfigurationManager.AppSettings.Get("displayMenu") == "true")
            {
                ConsoleKeyInfo cki;

                do
                {
                    Menu.DisplayMenu();
                    cki = Console.ReadKey(false); // show the key as you read it
                    switch (cki.KeyChar.ToString())
                    {
                        case "1":
                            SqlConnectionStringBuilder builder = DBConnection();
                            string[] filenames = PhotoNameChanger.GetFileNames(filePaths, result);

                            PhotoNameChanger.DeleteCSWebPhotoTable(builder);
                            PhotoNameChanger.DeleteRenamedFiles(newFilePath);
                            PhotoNameChanger.PopulateWebPhotoName(builder, filePaths, filenames);
                            PhotoNameChanger.CheckItemPhotos(builder);
                            PhotoNameChanger.RenameFiles(builder, filenames, newNames, match, newFilePath, filePaths); 
                            break;

                        case "3":
                            Console.WriteLine(spAttr);
                            break;

                    }
                } while (cki.Key != ConsoleKey.D2);

            }
            else
            {
                SqlConnectionStringBuilder builder = DBConnection();
                string[] filenames = PhotoNameChanger.GetFileNames(filePaths, result);

                PhotoNameChanger.DeleteCSWebPhotoTable(builder);
                PhotoNameChanger.DeleteRenamedFiles(newFilePath);
                PhotoNameChanger.PopulateWebPhotoName(builder, filePaths, filenames);
                PhotoNameChanger.CheckItemPhotos(builder);
                PhotoNameChanger.RenameFiles(builder, filenames, newNames, match, newFilePath, filePaths);
            }
        }
    }
}

