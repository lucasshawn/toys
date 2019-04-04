using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Base64
{
    class Program
    {
        static int Main(string[] args)
        {
            string srcFile = null;
            string dstFile = null;
            Action Action = Action.None;

        //    Directory.SetCurrentDirectory
            foreach (string s in args)
            {
                if (s.StartsWith("-") || s.StartsWith("/"))
                {
                    string sAction = s.Substring(1).ToUpper();
                    if (sAction.StartsWith("H"))
                    {
                        ShowHelp();
                        return 0;
                    }
                    if (Action == Program.Action.None)
                    {
                        if (sAction.StartsWith("E"))
                            Action = Action.Encode;
                        else if (sAction.StartsWith("D"))
                            Action = Action.Decode;
                        else
                            return ExitWithError(ErrorCode.invalidSwitch, s);
                    }
                    else
                        return ExitWithError(ErrorCode.tooManySwitches, s);

                }
                else  //must be file
                {
                    string aFile = s;
                    if (aFile.StartsWith("'"))  aFile = aFile.Substring(1);
                    if (aFile.StartsWith("\"")) aFile = aFile.Substring(1);
                    if (aFile.EndsWith("'")) aFile = aFile.Substring(0, aFile.Length - 1);
                    if (aFile.EndsWith("\"")) aFile = aFile.Substring(0, aFile.Length - 1);

                    if (srcFile == null)  //that's the first file
                    {
                        if (!File.Exists(aFile))
                            return ExitWithError(ErrorCode.srcFileDoesntExist, s);
                        else
                            srcFile = aFile;
                    }
                    else if (dstFile == null)
                    {
                        dstFile = aFile;
                    }
                    else
                        return ExitWithError(ErrorCode.tooManyParameters, s);

                }
            }
            if (Action == Action.None)
                return ExitWithError(ErrorCode.noActionDefined, "");

            if (srcFile == null)
                return ExitWithError(ErrorCode.noSrcFileProvided, "");

            //Do stuff here
            switch (Action)
            {
                case Program.Action.Encode:
                    try
                    {
                        Base64FileEncoder BE = new Base64FileEncoder();
                        BE.Encode(srcFile, dstFile);
                        if (dstFile!=null)
                            Console.WriteLine(string.Format("File '{0}' was successfully Byte64 encoded and saved to '{1}'", srcFile, dstFile));
                    }
                    catch (Exception ex)
                    {
                        return ExitWithError(ErrorCode.errorWhileEncoding, ex.Message);
                    }
                    break;
                case Program.Action.Decode:
                    try
                    {
                        Base64FileEncoder BE = new Base64FileEncoder();
                        BE.Decode(srcFile, dstFile);
                        if (dstFile != null)
                            Console.WriteLine(string.Format("File '{0}' was successfully decoded and saved to '{1}'", srcFile, dstFile));
                    }
                    catch (Exception ex)
                    {
                        return ExitWithError(ErrorCode.errorWhileDecoding, ex.Message);
                    }
                    break;
            }

            return 1;


        }

        static int ExitWithError(ErrorCode errorCode, string Arg)
        {
            switch (errorCode)
            {
                case ErrorCode.srcFileDoesntExist:
                    Console.WriteLine(string.Format("srcFile: '{0}' doesn't exist", Arg));
                    break;
                case ErrorCode.invalidSwitch:
                    Console.Write(string.Format("Invalid switch:{0}", Arg));
                    break;
                case ErrorCode.tooManySwitches:
                    Console.WriteLine("Only one switch allowed /e or /d or /h");
                    break;
                case ErrorCode.tooManyParameters:
                    Console.WriteLine("Too many parameters");
                    break;
                case ErrorCode.noSrcFileProvided:
                    Console.WriteLine("No srcFile provided");
                    break;
                case ErrorCode.noActionDefined:
                    Console.WriteLine("No action defined; expected one of the following: /e or /d or /h");
                    break;


            }
            Console.WriteLine();
            ShowHelp();
            return 0;

        }
        private static void ShowHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("Base64 by ProXoft L.L.C (http://www.proxoft.com)");
            Console.WriteLine("");
 
            Console.WriteLine("Valid command line syntax:");
            Console.WriteLine("To Encode:");
            Console.WriteLine("Base64[.exe] /e[ncode] sourceFile [destinationFile]");
            Console.WriteLine("To Decode:");

            Console.WriteLine("Base64[.exe] /d[ecode] sourceFile [destinationFile]");
            Console.WriteLine("For Help:");

            Console.WriteLine("Base64[.exe] /h");
            Console.WriteLine();
            Console.WriteLine("Note:");
            Console.WriteLine("if optional parameter [destinationFile] is omitted, program's output will be redirected to the console window.");

        }

        enum ErrorCode
        {
            None,
            noActionDefined,
            srcFileDoesntExist,
            invalidSwitch,
            tooManySwitches,
            tooManyParameters,
            noSrcFileProvided,
            errorWhileEncoding,
            errorWhileDecoding

        }
        enum Action
        {
            None,
            Encode,
            Decode
        }
    }
}
