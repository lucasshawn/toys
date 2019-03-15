There are a couple of ways you can use stripcr.  You can either call it via pipes on CLI:

Usage (using cmd shell pipes):
   dir {somepath} /s/b | stripcr
   NOTE: The input must be a list of files only so be sure you use the /b/s flags

Usage (standard in):
   stripcr {NO ARGS}
Type one or more paths and press ENTER.  When done press ENTER one more time to process the list.
