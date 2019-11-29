using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoNameChangerFinal
{
    class Menu
    {
        public static void DisplayMenu()
        {
            Console.Title = "PhotoName Changer";
            string title = @"
 ____     __     ______  ________   ________     
/\  _`\  /\ \   /\__  _\/\_____  \ /\_____  \    
\ \ \L\ \\ \ \  \/_/\ \/\/____//'/'\/____//'/'   
 \ \  _ <'\ \ \  __\ \ \     //'/'      //'/'    
  \ \ \L\ \\ \ \L\ \\_\ \__ //'/'___   //'/'___  
   \ \____/ \ \____//\_____\/\_______\ /\_______\
    \/___/   \/___/ \/_____/\/_______/ \/_______/";

            //Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (title.Length / 2)) + "}", title));

            Console.WriteLine(title);
            Console.WriteLine(" ");
            Console.WriteLine("--------------------------------------------------------------------------------------");
            Console.WriteLine("--------------------------------------------------------------------------------------");
            Console.WriteLine(" 1. Start");
            Console.WriteLine(" 2. Exit");

        }
    }
}
