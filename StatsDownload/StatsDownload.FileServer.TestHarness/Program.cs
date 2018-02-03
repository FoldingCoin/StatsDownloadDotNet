﻿namespace StatsDownload.FileServer.TestHarness
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                DependencyRegistration.Register();
                Console.WriteLine("Press {enter} to close server.");
                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"An exception was thrown. Details:{Environment.NewLine}{Environment.NewLine}{exception}{Environment.NewLine}");
                Console.WriteLine("Press {enter} to close server.");
                Console.ReadLine();
            }
        }
    }
}