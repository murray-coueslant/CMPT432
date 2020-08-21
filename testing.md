# Testing the Lexer

To test the lexers for this class we are very lucky, since a whole host of instances of other lexers exist on the course website.
So, in order to test Illumi I looked there first.

I started by grabbing a whole bunch of test cases from different examples in the HOF, and running them through my lexer. If they broke
anything I would try to fix it as and when it came up without any further breakage of unrelated code. Unfortunately this wasn't always the case, and many refactorings had to be performed.

Running through the test cases, I came across some particular problems. Such as my handling of strings and comments, which was imperfect and did not work in a lot of cases. I have now fixed it so much so that it is imperfect, but does work for the majority of test cases. In
the end, testing was a great bug finding tool thanks to the variety of different test cases I had available to me. One thing testing showed me was that there are a lot more errors possible than I had imagined, and the ones which are reportable in my application are just a small
subset of those.
