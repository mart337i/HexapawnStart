using System;

namespace NewHexapawnTest
{

    // Move, defined at bottom
    class Program
    {
        static char emptyCell = '_';
        private static Dictionary<int, Board> _lostBoards = new Dictionary<int, Board>();
        static void Main(string[] args)
        {
            do
            {
                // size of board 3x3 is default
                int boardSize = 3;
                var board = new Board(boardSize);

                // current player either 'P' or 'C'
                char player = 'P';
                Move move;

                while (!board.IsFinished(player))
                {
                    board.Print();
                    do
                    {
                        move = GetInputForPlayer(player,board);
                    } while (board.IsValidMove(player, move) == false);

                    board.MakeMove(move);
                    board.moves.Add(move);

                    // switch player
                    player = (player == 'P' ? 'C' : 'P');
                }

                if (board.Winner != player)
                {
                    _lostBoards.Add(board.version, board);
                }

                board.Print();
                Console.WriteLine("Player {0} wins!", board.Winner);
                WriteColorLine(ConsoleColor.Yellow, String.Format("Player {0} wins!", board.Winner));
                Console.WriteLine("Enter to continue, Q to exit");
            } while (Console.ReadLine() != "Q");
        }

        // metode til at prettyfy output
        static void WriteColorLine(ConsoleColor color, string text)
        {
            ConsoleColor oldColor = System.Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }

        static Move GetInputForPlayer(char player, Board board)
        {
            string fromPosition;
            string toPosition;

            Console.WriteLine(player == 'P' ? "Player 1 'P's Turn" : "Player 2 'C's Turn");

            if (player == 'C')
            {
                return AiMove(board);
            }
            else
            {
                // player moves
                Console.Write("Enter the position of the pawn you want to move (e.g., B1): ");
                fromPosition = Console.ReadLine().Trim().ToUpper();
                Console.Write("Enter the target position (e.g., B2): ");
                toPosition = Console.ReadLine().Trim().ToUpper();

                return textToMove(fromPosition, toPosition);
            }

            Move textToMove(string fromPosition, string toPosition)
            {
                Move move = new Move(0, 0, 0, 0);
                move.FromRow = int.Parse(fromPosition[1].ToString()) - 1;
                move.FromCol = fromPosition[0] - 'A';
                move.ToRow = int.Parse(toPosition[1].ToString()) - 1;
                move.ToCol = toPosition[0] - 'A';

                return move;
            }
        }

        // IMPLEMENT THIS!!!
        private static Move AiMove(Board board)
        {
            List<Move> validMoves = board.GetValidMoves('C');
            

            Random random = new Random();
            Move move = validMoves[random.Next(validMoves.Count)];
            return move;
        }

        class Board
        {
            public int version;
            private char[,] _cells;   // char [x,y]
            private int _boardSize;
            char? _winner = null;
            public List<Move> moves = new List<Move>(); 
            
            public Board(int boardSize)
            {
                version += 1; 
                _boardSize = boardSize;
                _cells = new char[boardSize, boardSize];

                // populate board
                for (int i = 0; i < boardSize; i++)
                {
                    _cells[i, 0] = 'P';
                    for (int j = 1; j < boardSize - 1; j++)
                    {
                        _cells[i, j] = emptyCell;
                    }
                    _cells[i, boardSize - 1] = 'C';
                }

                _winner = null;
            }

            public int BoardSize()
            {
                return _boardSize;
            }

            private char? AtPos(int x, int y)
            {
                // if outside bounds - return null
                if (x < 0 || y < 0 || x > _boardSize - 1 || y > _boardSize - 1) { return null; }

                // otherwise return content
                return _cells[x, y];
            }

            public void MakeMove(Move move)
            {
                _cells[move.ToCol, move.ToRow] = _cells[move.FromCol, move.FromRow];
                _cells[move.FromCol, move.FromRow] = emptyCell;
            }

            public bool IsFinished(char player)
            {
                _winner = CheckWinner(player);
                return _winner != null;
            }

            public char? Winner
            {
                get { return _winner; }
            }

            private char? CheckWinner(char player)
            {
                char otherPlayer = (player == 'P' ? 'C' : 'P');
                // if reached other end WIN!
                for (int i = 0; i < _boardSize; i++)
                {
                    if (AtPos(i, _boardSize - 1) == 'P') { return 'P'; }
                    if (AtPos(i, 0) == 'C') { return 'C'; }
                }
                if (GetValidMoves(player).Count() == 0) { return otherPlayer; }
                else if (GetValidMoves(otherPlayer).Count() == 0) { return player; }

                return null;
            }

            // find alle mulige træk
            public List<Move> GetValidMoves(char player)
            {
                List<Move> moves = new List<Move>();
                int vdirection = player == 'P' ? 1 : -1; // if P then positive/down, else (C) negative/up
                for (int row = 0; row < _boardSize; row++)
                {
                    for (int col = 0; col < _boardSize; col++)
                    {
                        if (_cells[col, row] == player)
                        {
                            for (int leftRight = -1; leftRight < 2; leftRight++)
                            {
                                Move tempMove = new Move(col, row, col + leftRight, row + vdirection);
                                if (IsValidMove(player, tempMove))
                                {
                                    moves.Add(tempMove);
                                }
                            }
                        }
                    }
                }
                return moves;
            }

            public bool IsValidMove(char player, Move move)
            {
                int direction = (player == 'P' ? 1 : -1);

                // moving forward?
                if (move.ToRow - move.FromRow == direction)
                {
                    bool CorrentFrom = AtPos(move.FromCol, move.FromRow) == player;
                    // not player at FromPosition
                    if (!CorrentFrom) { return false; }

                    int RightLeftStraight = Math.Abs(move.ToCol - move.FromCol);
                    // moving forward?
                    if (RightLeftStraight == 0)
                    {
                        bool CorrentTo = AtPos(move.ToCol, move.ToRow) == emptyCell;
                        return CorrentTo;
                    }
                    else if (RightLeftStraight == 1)
                    {
                        bool CorrentTo = AtPos(move.ToCol, move.ToRow) == (player == 'P' ? 'C' : 'P');
                        return CorrentTo;
                    }
                    else { return false; }
                }
                // Invalid move.
                return false;
            }

            public void Print()
            {
                // understøtter kun op til N=6
                Console.WriteLine("   A  B  C  D  E  F".Substring(0, _boardSize * 3 + 1));
                for (int row = 0; row < _boardSize; row++)
                {
                    Console.Write(row + 1 + " ");
                    for (int col = 0; col < _boardSize; col++)
                    {
                        Console.Write($" {_cells[col, row]} ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }

    internal class Move
    {
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }

        public Move(int fromCol, int fromRow, int toCol, int toRow)
        {
            FromRow = fromRow;
            FromCol = fromCol;
            ToRow = toRow;
            ToCol = toCol;
        }

        public override string ToString()
        {
            string ColumnLabels = "ABCDEF";
            return String.Format("{0}{1}->{2}{3}", ColumnLabels[FromCol], FromRow + 1, ColumnLabels[ToCol], ToRow + 1);
        }

    }

}
