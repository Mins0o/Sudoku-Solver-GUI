using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;

namespace Sudoku_Solver_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Solver puz = new Solver();
        //Solver class has a boolean nested array that will be used to check the numbers,
        //and various methods that will help solve and manipulate the sudoku puzzle.
        int[] sudokuNums = new int[81];
        // sudokuNums is an array of integer the main class holds to track and display the solution
        public MainWindow()
        {
            InitializeComponent();
        }



        private void open(object sender, RoutedEventArgs e)
        {
            //this function asks to choose file that has been saved from this program,
            //and it will load the sequence of numbers to the sudoku puzzle.
            //as well as display it.
            string sequence = "";
            OpenFileDialog openfile = new OpenFileDialog();
            if (openfile.ShowDialog() == true && openfile.CheckFileExists)
            {
                sequence = File.ReadAllText(openfile.FileName);

                for (int i = 0; i < 81; i++)
                {
                    sudokuNums[i] = (int)sequence[i] - 48;
                }
                DisplayNums(sudokuNums);
            }
            //still have to work on exceptions.
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            //self explanatory. This function saves the sequence of numbers : sudokuNums
            puz.Save(sudokuNums);
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            //this function will actually input blank to every cells, triggering WriteNum function of
            //Solver to erase the corresponding number from the program
            foreach (UIElement child in table.Children)
            {
                if (child is TextBox)
                {
                    TextBox tb = child as TextBox;
                    tb.Text = "";
                }
            }
        }


        private void Solve(object sender, RoutedEventArgs e)
        {
            //Refer to the function Solve of Solver class
            puz = new Solver(sudokuNums);
            int[] sol = puz.Solve(0);
            DisplayNums(sol);
        }

        private void Hint(object sender, RoutedEventArgs e)
        {
            //This function is intended to give you the sequence the computer followed to
            //solve the puzzle
            puz = new Solver(sudokuNums);
            int[] hint = puz.Solve(1);
            DisplayNums(hint, 0);
        }

        private void UpdateNum(object sender, TextChangedEventArgs e)
        {
            //This function is triggered by text change in the GUI.
            //It will validate the input text and put it in the sudokuNums array.
            //The updated number will be sent to the Solver when Solve or Hint is called.
            TextBox TB = sender as TextBox;
            int row_index_in_table = Grid.GetRow(TB);
            int column_index_in_table = Grid.GetColumn(TB);
            int the_table_index = row_index_in_table * 9 + column_index_in_table;
            if (TB.Text != "" && TB.Text != "0")
            {

                int input = (int)TB.Text[0];
                if (input > 47 && input < 59)
                {
                    sudokuNums[the_table_index] = input - 48;
                }
                else
                    TB.Text = "";

            }
            else
            {
                TB.Text = "";
                sudokuNums[the_table_index] = 0;
            }
        }

        private void DisplayNums(int[] solutions, int mode = 0)
        {
            //I am not sure what I was trying to do with this function
            //but it seems like case0 just scans the input array and
            //display(update) on the grid.
            switch (mode)
            {

                case 0:
                    int i = 0;
                    foreach (UIElement child in table.Children)
                    {
                        if (child is TextBox)
                        {
                            TextBox t = child as TextBox;
                            string temp;
                            temp = solutions[i].ToString();
                            t.Text = temp;
                            i++;
                        }
                    }
                    break;
                case 1:

                    break;
            }
        }
    }

    class Solver
    {

        public bool[,,,,] Puzzle = new bool[3, 3, 3, 3, 10];//[ Zone_x , Zone_y , Index_x , Index_y , Memo(index 0 : Number determined index, index 1~9 : which number is probable or not)]
        public Solver()
            : this(new int[81])
        {
        }

        public Solver(int[] input_puzzle)
        {
            int xa;
            int xi;
            int ya;
            int yi;
            for (byte i = 0; i < 81; i++)//////////////////////////   Initialization   ////////////////////////////////////
            {
                xa = x_a(i);
                xi = x_i(i);
                ya = y_a(i);
                yi = y_i(i);
                for (byte j = 1; j < 10; j++)
                {
                    this.Puzzle[xa, ya, xi, yi, j] = true;//Marking every number as probable.
                }
                this.Puzzle[xa, ya, xi, yi, 0] = false;//The number is undetermined.
            }//////////////////////////////////////////////////////   Initialization   ///////////////////////////////////

            for (int i = 0; i < 81; i++)
            {
                if (input_puzzle[i] != 0)
                {
                    WriteNum(i % 9, i / 9, input_puzzle[i]);
                }
            }
        }
        //these functions produce zones coordinates and index coordinates from full index
        private int x_a(int index)
        {
            return (index % 9) / 3;
        }
        private int x_i(int index)
        {
            return (index) % 3;
        }
        private int y_a(int index)
        {
            return (index / 27);
        }
        private int y_i(int index)
        {
            return (index / 9) % 3;
        }

        
        private bool Isin(int[] list, int theNum)
        {
            //This function checks whether there is theNum in the list or not
            int i = 0;
            while (i < list.Length - 1 && list[i] != theNum)
            {
                i++;
            }
            if (list[i] == theNum)
                return true;
            return false;
        }

        public int[] Solve(int options)
        {
            //this function basically just repeats MarkHiddenDef and Markonly
            //for a set amount of times
            if (options == 0)
            {

                for (byte i = 0; i < 10; i++)
                {
                    MarkHiddenDef();
                    MarkOnly();
                }
                int[] sol = new int[81];
                for (int i = 0; i < 81; i++)
                {
                    sol[i] = ReadNum(i % 9, i / 9);
                }
                return sol;
            }

            //Another intention is to log the cells it solves during the solution
            //and inform the user with the sequence.
            else
            {
                int[] solsequence = new int[81];
                for (int temp = 0; temp < 81; temp++)
                {
                    solsequence[temp] = (ReadNum(temp % 9, temp / 9) == 0) ? 0 : 81;
                }
                int[] seq1;
                int[] seq2;
                int i = 1;
                int j;
                int k;
                int x;
                for (byte itr = 0; itr < 10; itr++)
                {
                    j = 0;
                    k = 0;
                    seq1 = MarkHiddenDef();
                    seq2 = MarkOnly();
                    while (seq1[j] != 0 )
                    {
                        if (solsequence[seq1[j]]==0 && solsequence[seq1[j]] == 81)
                        {
                            solsequence[seq1[j]] = i;
                            i++;
                        }
                        j++;
                    }
                    while (seq2[k] != 0 )
                    {
                        if (solsequence[seq2[k]]== 0 && solsequence[seq1[j]] == 81)
                        {
                            solsequence[seq2[k]] = i;
                            i++;
                        }
                        k++;
                    }
                }
                for (int scan = 0; scan < 81; scan++)
                {
                    x = solsequence[scan];
                    solsequence[scan] = (x == 81) ? 0 : x;
                }
                return solsequence;
            }
        }

        private void WriteNum(int x, int y, int val)
        {
            //좌표 x,y와 값 val을 받아서 해당 칸에 값을 기록하는 함수

            byte numNow = ReadNum(x, y);

            if (numNow != 0)
            {//이미 0의 값이 아니라면 값을 초기화해줍니다.

                Puzzle[x / 3, y / 3, x % 3, y % 3, 0] = false;
                for (byte index = 0; index < 9; index++)
                {
                    if (!Puzzle[x / 3, index / 3, x % 3, index % 3, 0])
                    {
                        Puzzle[x / 3, index / 3, x % 3, index % 3, numNow] = true;
                    }//열
                    if (!Puzzle[index / 3, y / 3, index % 3, y % 3, 0])
                    {
                        Puzzle[index / 3, y / 3, index % 3, y % 3, numNow] = true;
                    }//행
                    if (!Puzzle[x / 3, y / 3, index % 3, index % 3, 0])
                    {
                        Puzzle[x / 3, y / 3, index % 3, index % 3, numNow] = true;
                    }//구역
                    Puzzle[x / 3, y / 3, x % 3, y % 3, index + 1] = true;
                }
            }


            if (val != 0)
            {//그 의외의 유효한 값은

                for (byte index = 0; index < 9; index++)
                { //그 칸이 속한 행/열/구역 에 대해 칸마다 그 수가 불가하다는 것을 표시하고

                    if (!Puzzle[x / 3, index / 3, x % 3, index % 3, 0])
                    {
                        Puzzle[x / 3, index / 3, x % 3, index % 3, val] = false;
                    }//열
                    if (!Puzzle[index / 3, y / 3, index % 3, y % 3, 0])
                    {
                        Puzzle[index / 3, y / 3, index % 3, y % 3, val] = false;
                    }//행
                    if (!Puzzle[x / 3, y / 3, index % 3, index % 3, 0])
                    {
                        Puzzle[x / 3, y / 3, index % 3, index % 3, val] = false;
                    }//구역

                }

                for (byte i = 1; i < 10; i++)
                {
                    if (i == val)
                    {
                        Puzzle[x / 3, y / 3, x % 3, y % 3, i] = true;
                    }
                    else
                    {
                        Puzzle[x / 3, y / 3, x % 3, y % 3, i] = false;
                    }

                }

                Puzzle[x / 3, y / 3, x % 3, y % 3, 0] = true;

            }

            return;
        }



        private byte ReadNum(int x, int y)
        {//x와 y 좌표를 받아서 그에 해당하는 숫자를 반환합니다. 만약 확정수가 없으면 0을 반환합니다.

            if (Puzzle[x / 3, y / 3, x % 3, y % 3, 0])
            {//확실한 숫자 하나가 있어야만 ReadNum을 실행합니다.

                for (byte i = 1; i < 10; i++)
                {

                    if (Puzzle[x / 3, y / 3, x % 3, y % 3, i])
                    {
                        return i;
                    }
                    //확정수가 반환됩니다.

                }

            }

            return 0;
        }



        public void Save(int[] sol)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = "firsttry.txt";
            saveFile.Filter = "Text Fiel | *.txt";
            saveFile.ShowDialog();
            StreamWriter wr = new StreamWriter(saveFile.OpenFile());
            for (int i = 0; i < sol.Length; i++)
            {
                wr.Write(sol[i].ToString());
            }
            wr.WriteLine();
            wr.Dispose();
            wr.Close();
        }


        private int[] MarkHiddenDef()
        {//어떤 칸에 가능한 수가 단 한 개 있는데 확정수로 표시되어있지 않은 경우 확정수로 전환하는 함수

            int[] sequence = new int[81];
            int j = 0;
            int xa;
            int ya;
            int xi;
            int yi;
            for (byte i = 0; i < 81; i++)
            {
                xa = x_a(i);
                xi = x_i(i);
                ya = y_a(i);
                yi = y_i(i);
                if (!Puzzle[xa, ya, xi, yi, 0])
                {

                    byte memory = 0;
                    byte truesum = 0;//가능한 숫자가 한개만 있는지 확인하는데 사용되는 변수

                    for (byte scan = 1; scan < 10; scan++)
                    {//가능한 숫자의 개수를 세는데,

                        if (Puzzle[xa, ya, xi, yi, scan])
                        {
                            truesum++;
                            memory = scan;
                        }//여기서 숫자를 셈니다.(emory로는 어느 자리가 그 한 개의 수였는지 기억합니다.
                        if (truesum == 2)
                        {
                            break;
                        }//2개를 넘어가면 바로 다음으로 넘어갑니다.

                    }

                    //모두 세고 난 후,
                    if (truesum == 1)
                    {
                        WriteNum(i % 9, i / 9, memory);
                        sequence[j] = i;
                        j++;

                    }//그 수를 기록합니다.

                }

            }

            return sequence;
        }

        private int[] MarkOnly()
        {
            int[] sequence = new int[81];
            int j = 0;
            for (byte num = 1; num < 10; num++)
            {

                for (byte index = 0; index < 9; index++)
                {

                    byte aTruesum = 0;
                    byte aMemory = 0;
                    byte rTruesum = 0;
                    byte rMemory = 0;
                    byte cTruesum = 0;
                    byte cMemory = 0;

                    for (byte rpt = 0; rpt < 9; rpt++)
                    {

                        if (Puzzle[index % 3, index / 3, rpt % 3, rpt / 3, num] && aTruesum < 2)
                        {
                            aTruesum++;
                            aMemory = rpt;
                        }
                        if (Puzzle[rpt / 3, index / 3, rpt % 3, index % 3, num] && rTruesum < 2)
                        {
                            rTruesum++;
                            rMemory = rpt;
                        }
                        if (Puzzle[index / 3, rpt / 3, index % 3, rpt % 3, num] && cTruesum < 2)
                        {
                            cTruesum++;
                            cMemory = rpt;
                        }

                    }

                    if (aTruesum == 1)
                    {
                        int xcor = (index % 3 * 3) + (aMemory % 3);
                        int ycor = (index / 3 * 3) + (aMemory / 3);
                        WriteNum(xcor, ycor, num);
                        if (!Isin(sequence, xcor + ycor * 9))
                        {
                            sequence[j] = xcor + ycor * 9;
                            j++;
                        }
                    }
                    else if (rTruesum == 1)
                    {
                        WriteNum(rMemory, index, num);
                        if (!Isin(sequence, rMemory + index * 9))
                        {
                            sequence[j] = rMemory + index * 9;
                            j++;
                        }
                    }
                    else if (cTruesum == 1)
                    {
                        WriteNum(index, cMemory, num);
                        if (!Isin(sequence, index + cMemory * 9))
                        {
                            sequence[j] = index + cMemory * 9;
                            j++;
                        }
                    }

                }

            }

            return sequence;
        }

    }
}

