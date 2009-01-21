﻿using System;
using System.Collections.Generic;
using System.Text;

// Simple file deletion operations with no user interface.

public class SimpleFileDelete
{
    static void Main()
    {
        // Delete a file by using File class static method
        if (System.IO.File.Exists(@"C:\Users\Public\TestFolder\DeleteTest\test.txt"))
        {
            // Use a try block to catch IOExceptions, to
            // handle the case of the file already being
            // opened by another process.
            try
            {
                System.IO.File.Delete(@"C:\Users\Public\TestFolder\DeleteTest\test.txt");
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

    }
}