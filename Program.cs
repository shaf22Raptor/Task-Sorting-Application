namespace ProjectManager
{
    /// <summary>
    /// Code in Program class borrowed from CAB201 lecture videos. Originally written by Lawrence Buckingham, adapted for current assignment.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Begins the program with greetings and main menu options
        /// </summary>
        const int addTask = 0, removeTask = 1, editExecutionTime = 2, save = 3, showSequence = 4, earliestTimes = 5, exit = 6;
        
        static void Main(string[] args)
        {
            string[] options = {@"Main Menu
---------",
                    "Add new task",
                    "Remove task", 
                    "Edit execution time",
                    "Save data",
                    "Show sequence of data",
                    "Show earliest times",
                    "Exit"
            };
            Welcome();
            // Task collection is created immediately. Collection methods are stored here.
            TaskCollection taskCollection = new TaskCollection();
            Run(options);
            Exit();

            /// Greetings
            void Welcome()
            {
                Console.WriteLine("Welcome to the Project Task Manager!");
                Console.WriteLine("Please enter name of file to load tasks from:");
            }
            /// Message shown when exiting the program.
            void Exit()
            {
                Console.WriteLine("Thank you for using Task Manager!");
            }

            /// Shows menu options and runs the option chosen by the user.
            void Run(string[] options)
            {
                while (true)
                {
                    // Uses menu, showmenu and getopt methods. 
                    int choice = Menu(options);
                    if (choice == exit) break;
                    Process(choice);
                }
            }

            /// Show the menu and get chosen option from user
            int Menu(string[] options)
            {
                while (true)
                {
                    // Show the menu
                    ShowMenu(options);
                    int option;

                    // Get the option from the user
                    if (GetOpt(out option, 1, options.Length, options.Length - 1))
                    {
                        return option - 1;
                    }
                }
            }

            /// Shows the menu naviagtion instructions
            void ShowMenu(string[] options)
            {
                Console.WriteLine(@$"{options[0]}");

                for (int i = 1; i < options.Length; i++)
                {
                    // Shows each menu option
                    Console.WriteLine($"({i}) {options[i]}");
                }
                // Shows how to select an option
                Console.WriteLine($"Please select select an option between 1 and {options.Length - 1}");
            }

            /// Get the chosen option from user
            bool GetOpt(out int option, int low, int high, int exit)
            {
                string userInput = Input();

                if (userInput == null)
                {
                    option = exit + 1;
                    Console.WriteLine("Please select one of the options.");
                    return false;
                }

                return int.TryParse(userInput, out option)
                    && option >= low
                    && option <= high;
            }

            /// Execute the chosen option using methods in the taskcollection class
            void Process(int choice)
            {
                switch (choice)
                {
                    case addTask: taskCollection.AddTask(); break;
                    case removeTask: taskCollection.Remove(); break;
                    case editExecutionTime: taskCollection.EditExecutionTime(); break;
                    case save: taskCollection.SaveMainFile(); break;
                    case showSequence: taskCollection.Sequence(); break;
                    case earliestTimes: taskCollection.EarliestTimes(); break;
                    default: break;
                }
            }
        }
        /// <summary>
        ///  Used to get input from the user whenever an input is needed.
        /// </summary>
        /// <returns></returns>
        public static string Input()
        {
            Console.Write("> ");
            return Console.ReadLine();     
        }

    }
}
