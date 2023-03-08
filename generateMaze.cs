using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateMaze : MonoBehaviour
{
    public bool braid = true;
    public int size = 40; //The size of one side of the maze. Between 1-40. Large numbers have a small chance to cause stack overflow
    public int braidRate = 3; //The lower the number, the more braids.
    public int stackDetail = 15; //lower numbers force more gaps between cells.
    public int numOfStructures; //The number of structures for puzzles, most likely will stay at 3
    public string[] symbolList;
    public string[][] descriptorList;
    List<int> tracePath = new List<int>(); //global variable that tracks how the maze was written. This will be used to make patrol routes. 
    /*
    the convertToNull function takes in an array of symbols and an array representing the 
    indices to replace in the symbol array. It then returns the symbol array with the according
    indices replaced with "!"
    */
    private string[] convertToNull(string[] tempArray, int[] indexesToReplace)
    {
        for (int i = 0; i < indexesToReplace.Length; i++)
        {
            int pick = indexesToReplace[i];
            tempArray[pick] = "!";
        }
        return tempArray;
    }
    /*
    the convertSymbol function takes a bool array in the format [bool, bool, bool, bool] and
    returns a string that represents a cell in a maze
    */
    private string convertSymbol(bool[] connections) //convert an array of True and Falses 
    {                                                  //into the corresponding char
        string[] tempArray = {"█", "─", "│", "╵", "╷", "╶", "╴", "┐", "┌", "└", "┘",
                          "┬", "┤", "├", "┴", "┼"};
        //                0    1    2    3    4    5   6    7     8    9   10
        //                11   12   13   14   15   
        int numTrue = 0;
        for (int i = 0; i < 4; i++)
        {
            if (connections[i])
            {
                numTrue++;
            }
        }
        // Filter out symbols based on number of connections (specify which ones to take away)
        if (numTrue == 0)
        {
            return "█";
        }
        else if (numTrue == 1)
        {
            int[] indexArray = { 0, 1, 2, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            tempArray = convertToNull(tempArray, indexArray);
        }
        else if (numTrue == 2)
        {
            int[] indexArray = { 0, 3, 4, 5, 6, 11, 12, 13, 14, 15 };
            tempArray = convertToNull(tempArray, indexArray);
        }
        else if (numTrue == 3)
        {
            int[] indexArray = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15 };
            tempArray = convertToNull(tempArray, indexArray);
        }
        else
        {
            return "┼";
        }
        // Filter out symbols based on connection array (specify which ones to take away)
        if (!connections[0]) //up
        {
            int[] indexArray = { 2, 3, 9, 10, 12, 13, 14, 15 };
            tempArray = convertToNull(tempArray, indexArray);
        }
        if (!connections[1]) //down
        {
            int[] indexArray = { 2, 4, 7, 8, 11, 12, 13, 15 };
            tempArray = convertToNull(tempArray, indexArray);
        }
        if (!connections[2]) //left
        {
            int[] indexArray = { 1, 6, 7, 10, 11, 12, 14, 15 };
            tempArray = convertToNull(tempArray, indexArray);
        }
        if (!connections[3]) //right
        {
            int[] indexArray = { 1, 5, 8, 9, 11, 13, 14, 15 };
            tempArray = convertToNull(tempArray, indexArray);
        }

        for (int i = 0; i < tempArray.Length; i++) // Search for the one remaining symbol that is
        {                                          // not "!"
            if (tempArray[i] != "!")
            {
                return tempArray[i];
            }
        }
        return ""; //should never reach here
    }
    /* createArray takes a size and makes a 2D array filled with falses, input must be a perfect square
    */
    private bool[][] createArray(int size)
    {
        bool[][] maze = new bool[size][];
        for (int i = 0; i < size; i++)
        {
            maze[i] = new bool[4] { false, false, false, false };
        }
        return maze;
    }
    //given 4 bools, check if they are all false, return if so
    private bool checkEmpty(bool[] cell)
    {
        for (int i = 0; i < 4; i++)
        {
            if (cell[i])
            {
                return false;
            }
        }
        return true;
    }
    /*
    Figures out which cell to change and whether or not to braid, 
    */
    private int calculateNextCell(int currentCell, bool[][] mazeGrid, int width, int height)
    {
        System.Random random = new System.Random();
        int pick = 0;
        bool one = false;
        bool two = false;
        bool three = false;
        bool four = false;
        while (!one | !two | !three | !four)
        {
            pick = random.Next(0, 4);
            if (pick == 0 && currentCell - width > 0) // check up
            {
                if (checkEmpty(mazeGrid[currentCell - width]))
                {
                    return currentCell - width;
                }
                else
                {
                    one = true;
                }
            }
            else if (pick == 1 && currentCell + width < width * height) // check down
            {
                if (checkEmpty(mazeGrid[currentCell + width]))
                {
                    return currentCell + width;
                }
                else
                {
                    two = true;
                }
            }
            else if (pick == 2 && ((currentCell + width) % width) != 0)
            {
                if (checkEmpty(mazeGrid[currentCell - 1]))
                {
                    return currentCell - 1;
                }
                else
                {
                    three = true;
                }
            }
            else if (pick == 3 && ((currentCell + 1) % width) != 0)
            {
                if (checkEmpty(mazeGrid[currentCell + 1]))
                {
                    return currentCell + 1;
                }
                else
                {
                    four = true;
                }
            }
            if (currentCell - width < 1)
            {
                one = true;
            }
            if (currentCell + width > width * height - 1)
            {
                two = true;
            }
            if (((currentCell + width) % width) == 0)
            {
                three = true;
            }
            if (((currentCell + 1) % width) == 0)
            {
                four = true;
            }
        }
        while (true)
        {
            pick = random.Next(4);
            if (pick == 0 && currentCell - width > 0) // check up
            {
                return currentCell - width;
            }
            else if (pick == 1 && currentCell + width < width * height) // check down
            {
                return currentCell + width;
            }
            else if (pick == 2 && ((currentCell + width) % width) != 0) //check left
            {
                return currentCell - 1;
            }
            else if (pick == 3 && ((currentCell + 1) % width) != 0) // check right
            {
                return currentCell + 1;
            }
        }
    }
    /*
    Decides whether or not to go forwards or go back given standard maze info
    */
    private bool shouldGoBack(int currentCell, int proposedCell, int lastCell, bool[][] maze, int maxSize, int stackSize, int stackDetail)
    {
        if (stackSize > stackDetail)
        {
            return false;
        }
        if (proposedCell > 0 && proposedCell < maxSize)
        {
            if (checkEmpty(maze[proposedCell]))
            {
                return false;
            }
            return true;
        }
        return true;
    }
    /*
    Given a maze, a proposed cell, the current cell, and the last cell, decided whether or not the proposed cell should be braided
    */
    private bool shouldBraid(int currentCell, int proposedCell, int lastCell, bool[][] maze, int maxSize, int braidRatio)
    {
        System.Random random = new System.Random();
        if (proposedCell < maxSize - 1 && proposedCell > 0)
        {
            if (currentCell != lastCell && proposedCell != lastCell)
            {
                if (checkEmpty(maze[proposedCell]) == false)
                {
                    if (random.Next(braidRatio) == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
            return false;
        }
        return false;
    }
    // print the maze
    public void printProgress(bool[][] grid, int width, int height)
    {
        string line = "";
        for (int n = 0; n < height; n++)
        {
            for (int i = 0; i < width; i++)
            {
                line += convertSymbol(grid[i + (n * width)]);
            }
            System.Console.WriteLine(line);
            line = "";
        }
    }
    /*
    Given a blank grid, the width, and the height, generate random cells until the grid is full
    */
    private bool[][] createMaze(bool[][] grid, int width, int height, int stackDetail, int braidRatio = 2, bool braid = true)
    {
        int currentCell = 0;
        int lastCell = 0;
        int visited = 0;
        Stack<int> stack = new Stack<int>();
        stack.Push(currentCell);
        while (visited < height * width - 1)
        {
            int proposedCell = calculateNextCell(currentCell, grid, width, height);
            if (shouldBraid(currentCell, proposedCell, lastCell, grid, width * height, braidRatio) && braid)
            {
                if (proposedCell == currentCell - width) // up
                {
                    grid[currentCell][0] = true;
                    grid[proposedCell][1] = true;
                }
                else if (proposedCell == currentCell + width) // down
                {
                    grid[currentCell][1] = true;
                    grid[proposedCell][0] = true;
                }
                else if (proposedCell == currentCell - 1) // left
                {
                    grid[currentCell][2] = true;
                    grid[proposedCell][3] = true;
                }
                else // right
                {
                    grid[currentCell][3] = true;
                    grid[proposedCell][2] = true;
                }
                stack.Pop();
                currentCell = stack.Peek();
                stack.Pop();
                lastCell = stack.Peek();
                stack.Push(currentCell);
            }
            else if (shouldGoBack(currentCell, proposedCell, lastCell, grid, width * height, stack.Count, stackDetail)) // pop stack and go back a cell
            {
                stack.Pop();
                currentCell = stack.Peek();
                stack.Pop();
                lastCell = stack.Peek();
                stack.Push(currentCell);
            }
            else // go to new, empty cell
            {
                if (proposedCell == currentCell - width) // up
                {
                    grid[currentCell][0] = true;
                    grid[proposedCell][1] = true;
                    lastCell = stack.Peek();
                    stack.Push(proposedCell);
                    currentCell = stack.Peek();
                    visited++;
                    tracePath.Add(currentCell);
                }
                else if (proposedCell == currentCell + width) // down
                {
                    grid[currentCell][1] = true;
                    grid[proposedCell][0] = true;
                    lastCell = stack.Peek();
                    stack.Push(proposedCell);
                    currentCell = stack.Peek();
                    visited++;
                    tracePath.Add(currentCell);
                }
                else if (proposedCell == currentCell - 1) // left
                {
                    grid[currentCell][2] = true;
                    grid[proposedCell][3] = true;
                    lastCell = stack.Peek();
                    stack.Push(proposedCell);
                    currentCell = stack.Peek();
                    visited++;
                    tracePath.Add(currentCell);
                }
                else // right
                {
                    grid[currentCell][3] = true;
                    grid[proposedCell][2] = true;
                    lastCell = stack.Peek();
                    stack.Push(proposedCell);
                    currentCell = stack.Peek();
                    visited++;
                    tracePath.Add(currentCell);
                }
            }
            //printProgress(grid, width, height);
        }
        return grid;
    }

    /*
    Takes a symbol and returns an array from strings with values for type of block and rotation   
    █ ─ │ ╵ ╷ ╶ ╴ ┐ ┌ └ ┘ ┬ ┤ ├ ┴ ┼ reference symbols 
    */
    private string[] convertSymbolToDescriptor(string symbol)
    {
        string valid = "█─│╵╷╶╴┐┌└┘┬┤├┴┼UDLR↓↑→←<>˅^";
        string zero = " ╵│└┴┼˅D↓";
        string ninety = "┌├─<L←╶";
        string one_eighty = "╷┐┬^U↑";
        string dead_end = "╴╶╷╵";
        string straight = "─│";
        string l_turn = "└┐┌┘";
        string three_way = "├┴┤┬";
        string start = "UDLR";
        string end = "↓↑→←";
        string structure = "<>˅^";
        string[] returnValue = { "empty", "empty" };
        if (!valid.Contains(symbol))     // <- check if generation has blemish, return error and retry flag if so
        {
            returnValue[0] = "failed, symbol not in symbol table";
            returnValue[1] = "retry";
            return returnValue;
        }
        if (symbol == "█")
        {
            returnValue[0] = "EMPTY";
            returnValue[1] = "0";
            return returnValue;
        }
        if (zero.Contains(symbol)) //determine if rotation is neutral
        {
            returnValue[1] = "0";
        }
        else if (ninety.Contains(symbol)) //determine if rotation is 90 degrees
        {
            returnValue[1] = "90";
        }
        else if (one_eighty.Contains(symbol)) //determine if rotation is 180 degrees
        {
            returnValue[1] = "180";
        }
        else //determine if rotation is 270 degrees
        {
            returnValue[1] = "270";
        }
        if (dead_end.Contains(symbol))
        {
            returnValue[0] = "DEAD_END";
        }
        else if (straight.Contains(symbol))
        {
            returnValue[0] = "STRAIGHT";
        }
        else if (l_turn.Contains(symbol))
        {
            returnValue[0] = "L_TURN";
        }
        else if (three_way.Contains(symbol))
        {
            returnValue[0] = "THREE_WAY";
        }
        else if (start.Contains(symbol))
        {
            returnValue[0] = "START";
        }
        else if (end.Contains(symbol))
        {
            returnValue[0] = "END";
        }
        else if (structure.Contains(symbol))
        {
            returnValue[0] = "STRUCTURE";
        }
        else
        {
            returnValue[0] = "CROSS_SECT";
        }
        return returnValue;
    }
    // given a width and a height, create and print a maze
    public string[] startCreating(int sideSize, int ratio = 2, bool braid = true, int stackDetail = 3000)
    {
        if (sideSize > 50)
        {
            sideSize = 50;
        }
        string[] symbolList = new string[sideSize * sideSize];
        bool computing = true;
        while (computing)
        {
            symbolList = new string[sideSize * sideSize];
            bool[][] grid = createArray(sideSize * sideSize);
            grid = createMaze(grid, sideSize, sideSize, stackDetail, ratio, braid);
            string line = "";
            for (int n = 0; n < sideSize; n++)
            {
                for (int i = 0; i < sideSize; i++)
                {
                    line += convertSymbol(grid[i + (n * sideSize)]);
                    symbolList[i + (n * sideSize)] = convertSymbol(grid[i + (n * sideSize)]);
                }
                //Debug.Log(line);
                line = "";
            }
            if (symbolList.Length == sideSize * sideSize)
            {
                computing = false;
            }
        }
        return symbolList;
    }

    /*
    Given a list of symbols and a size, if there is a dead end, convert it to a building, if there are less than 5 dead ends, rebuild the maze
    */
    private string[] generateCaverns(string[] symbolList, int size, int braidRate, bool braiding, int stackDetail, int structures)
    {
        Console.WriteLine("generating caverns");
        int deadCount = 0;
        bool keepGoing = true;
        //Verify enough exits
        while (keepGoing)
        {
            deadCount = 0;
            for (int i = 0; i < (size * size) - 1; i++)
            {
                if (symbolList[i] == "╵" | symbolList[i] == "╷" | symbolList[i] == "╴" | symbolList[i] == "╶")
                {
                    deadCount++;
                }
            }
            if (deadCount < structures + 2)
            {
                symbolList = this.startCreating(size, braidRate, braiding, stackDetail);
            }
            else
            {
                keepGoing = false;
            }
        }

        //Generate Stuctures
        int numGenerated = 1;
        int numEncountered = 0;
        int skip = deadCount / structures;
        for (int i = 0; i < (size * size) - 1; i++)
        {
            if (numGenerated < structures + 1)
            {
                if (symbolList[i] == "╵")
                {
                    if ((numEncountered == 0) || (numEncountered % skip == 0))
                    {
                        symbolList[i] = "˅";
                        numGenerated++;
                    }
                    numEncountered++;
                }
                else if (symbolList[i] == "╷")
                {
                    if ((numEncountered == 0) || (numEncountered % skip == 0))
                    {
                        symbolList[i] = "^";
                        numGenerated++;
                    }
                    numEncountered++;
                }
                else if (symbolList[i] == "╴")
                {
                    if ((numEncountered == 0) || (numEncountered % skip == 0))
                    {
                        symbolList[i] = ">";
                        numGenerated++;
                    }
                    numEncountered++;
                }
                else if (symbolList[i] == "╶")
                {
                    if ((numEncountered == 0) || (numEncountered % skip == 0))
                    {
                        symbolList[i] = "<";
                        numGenerated++;
                    }
                    numEncountered++;
                }
            }
        }
        Console.WriteLine("printing map");
        string line = "";
        for (int i = 1; i < (size * size) + 1; i++)
        {
            if ((i % size == 0) && i > 0)
            {
                //Console.WriteLine("this should print 50 times");
                line += symbolList[i - 1];
                Console.WriteLine(line);
                line = "";
            }
            else
            {
                //Console.WriteLine("this should print a lot");
                line += symbolList[i - 1];
            }
        }
        return symbolList;
    }

    /*
  Takes the symbol list and the size and converts the first and last deadEnd to an entrance or exit
  */
    private string[] convertExitEntrance(string[] symbolList, int size)
    {
        //calculate first and last ends
        int countEnds = 0;
        int first = 0;
        bool firstB = false;
        for (int i = 0; i < (size * size) - 1; i++)
        {
            if (symbolList[i] == "╵" | symbolList[i] == "╷" | symbolList[i] == "╴" | symbolList[i] == "╶")
            {
                countEnds++;
                if (!firstB)
                {
                    first += i;
                    firstB = true;
                }
            }
        }

        int tempCount = 0;
        int lastPlace = 0;
        for (int n = 0; n < (size * size) - 1; n++)
        {
            if (symbolList[n] == "╵" | symbolList[n] == "╷" | symbolList[n] == "╴" | symbolList[n] == "╶")
            {
                tempCount++;
                if (tempCount == countEnds)
                {
                    lastPlace = n;
                }
            }
        }
        //determine orientation
        bool entrance = true;
        for (int i = 0; i < 2; i++)
        {
            int choose = first;
            if (i == 1)
            {
                choose = lastPlace;
                entrance = false;
            }
            // match char
            if (symbolList[choose] == "╵")
            {
                if (entrance)
                {
                    symbolList[choose] = "D"; //entrance
                }
                else
                {
                    symbolList[choose] = "↓"; //exit
                }
            }
            else if (symbolList[choose] == "╷")
            {
                if (entrance)
                {
                    symbolList[choose] = "U"; //entrance
                }
                else
                {
                    symbolList[choose] = "↑"; //exit
                }
            }
            else if (symbolList[choose] == "╴")
            {
                if (entrance)
                {
                    symbolList[choose] = "R"; //entrance
                }
                else
                {
                    symbolList[choose] = "→"; //exit
                }
            }
            else if (symbolList[choose] == "╶")
            {
                if (entrance)
                {
                    symbolList[choose] = "L"; //entrance
                }
                else
                {
                    symbolList[choose] = "←"; //eixt
                }
            }
        }
        Console.WriteLine("printing map w/ entrance and exit");
        string line = "";
        for (int i = 1; i < (size * size) + 1; i++)
        {
            if ((i % size == 0) && i > 0)
            {
                //Console.WriteLine("this should print 50 times");
                line += symbolList[i - 1];
                Console.WriteLine(line);
                line = "";
            }
            else
            {
                //Console.WriteLine("this should print a lot");
                line += symbolList[i - 1];
            }
        }
        return symbolList;
    }

    /*
    Given a list of symbols and the size of the list, creates a list of descriptors describing in words, each cell of the maze
    */
    private string[][] gridToDescriptor(string[] symbolList, int size, bool write = false)
    {
        string[][] returnValue = new string[size][];
        for (int i = 0; i < size; i++)
        {
            returnValue[i] = convertSymbolToDescriptor(symbolList[i]);
            if (write)
            {
                System.Console.Write(returnValue[i][0]);
                System.Console.Write(" ");
                System.Console.Write(returnValue[i][1]);
                System.Console.WriteLine(" ");
            }
        }
        return returnValue;
    }
    //Basically a main
    private void startGeneration()
    {
        if (size < 5) // under 5 and the map never generates
        {
            size = 5;
        }
        else if (size > 25) // over 25 and the nav mesh agent starts bugging due to mass calculations of where it can go, but 625 blocks is more than enough
        {
            size = 25;
        }
        symbolList = startCreating(size, braidRate, braid, stackDetail); //throw in a false to turn off/minimize braiding
        symbolList = generateCaverns(symbolList, size, braidRate, braid, stackDetail, numOfStructures);
        symbolList = convertExitEntrance(symbolList, size); // must do exit/entrance after generateCaverns
        descriptorList = gridToDescriptor(symbolList, size * size, false); //throw in a true to see the written description
    }
    // start generating a maze and return a list of maze parts to use for generation
    public string[][] getMazeDescription()
    {
        startGeneration();
        return descriptorList;
    }
    // get the size of the maze
    public int getSize()
    {
        return size;
    }
}
