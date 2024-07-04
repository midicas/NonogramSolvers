This project implements 5 versions of nonogram solvers.

The Nonogram class is the internal representation of a nonogram.
Nonograms are retrieved from .csv file. Enclosed are some example nonograms.

The solver class implements 5 different solvers.
1) BacktrackByIndex
2) BacktrackByLine
Two versions of LineOverlap:
3) Memory optimized
4) Time optimized

5) Overlap of the blocks.

The nonogramPrinter class implements some useful debugging methods.
The Tester class implements a simple test environment to compare all solving methods.
Running the main Program allows you to either solve a single puzzle using one of the solver methods or test the solvers using multiple puzzles.
