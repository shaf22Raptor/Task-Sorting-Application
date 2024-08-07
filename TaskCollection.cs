using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectManager
{
    internal class TaskCollection
    {
        private int count;
        private List<Task> tasks;
        private string sourceFile;
        public TaskCollection()
        {
            // Upon initialisation, it will immediately try to load data from a save file.
            LoadData(Program.Input());
        }
        public int Number { get { return count; } }
        public string SourceFile { set { sourceFile = value; } }
        public List<Task> Tasks { get { return tasks; } }

        /// <summary>
        /// Returns a task that is being searched for.
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public Task Search(string taskID, string errorMessage)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                // Checks each task in collection to see if its ID matches supplied ID.
                if (tasks[i].GetID == taskID)
                {
                    return tasks[i];
                }
            }
            // Returns error message if task is not found. g
            Console.WriteLine($"{errorMessage}");
            return null;
        }
        /// <summary>
        /// Returns a task that is being searched for when an error message (for when task is not found) is not needed.
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public Task Search(string taskID)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                // Checks each task in collection to see if its ID matches supplied ID.
                if (tasks[i].GetID == taskID)
                {
                    return tasks[i];
                }
            }
            return null;
        }
        /// <summary>
        /// Acknowledge whether save file has been found or not.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        string ConfirmFileName(string fileName)
        {
            bool fileExists = false;
            while (!fileExists)
            {
                // if file exists, then acknowledge existence. 
                if (File.Exists($"{fileName}.txt"))
                {
                    Console.WriteLine("File does exist");
                    fileExists = true;
                }
                else
                {
                    // If file is not found, then user must supply the name of a file that does exist.
                    Console.WriteLine("The file does not exist.");
                    Console.WriteLine("Please enter an existing file name:");
                    fileName = Program.Input();
                    continue;
                }
            }
            return fileName;
        }

        /// <summary>
        /// Reads data from save file and reconstructs task collection so it may be worked with. 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        List<Task> ReadData(string fileName)
        {
            List<Task> tasks = new List<Task>();
            using (StreamReader reader = new StreamReader($"{fileName}.txt"))
            {
                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine();
                    // If file is empty, then return. 
                    if (currentLine.Trim() == "")
                    {
                        return tasks;
                    }
                    tasks.Add(InterpretData(currentLine));
                }
            }
            return tasks;
        }

        /// <summary>
        /// Tasks are rebuilt in program after being read in from save file. 
        /// </summary>
        /// <param name="currentLine"></param>
        /// <returns></returns>
        Task InterpretData(string currentLine)
        {
            string ID;
            int executionTime;
            string[] properties = currentLine.Split(',');
            List<string> dependentTasks = new List<string>();
            ID = properties[0];
            int.TryParse(properties[1], out executionTime);
            for (int i = 2; i < properties.Length; i++)
            {
                dependentTasks.Add(properties[i].Trim());
            }
            Task loadTask = new Task(ID, executionTime, dependentTasks);
            return loadTask;
        }
        /// <summary>
        /// Main method used to load save file and bring contents into the program
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<Task> LoadData(string fileName)
        {
            sourceFile = fileName;
            fileName = ConfirmFileName(fileName);
            SourceFile = fileName;
            tasks = ReadData(fileName);
            count = tasks.Count();
            return tasks;
        }

        /// <summary>
        /// Change execution time of a task.
        /// </summary>
        public void EditExecutionTime()
        {
            Console.WriteLine("Please enter the task you would like to edit");
            string task = Program.Input();
            // Try to find the task that will be edited.
            Task taskToEdit = Search(task, "The task could not be found!");
            if (taskToEdit == null)
            {
                return;
            }
            Console.WriteLine("Please enter the new execution time");
            int.TryParse(Program.Input(), out int newExecutionTime);
            taskToEdit.edit(newExecutionTime);
            Console.WriteLine("Execution time succesfully edited!");
        }

        /// <summary>
        /// Remove a supplied task from the program.
        /// </summary>
        public void Remove()
        {
            Console.WriteLine("Please enter the task you would like to delete");
            string target = Program.Input().Trim();
            // Search for task that is to be removed. 
            Task taskToRemove = Search(target, "The task could not be found!");
            if (taskToRemove == null)
            {
                return;
            }
            // Acknowledge if it has been removed. 
            Console.WriteLine($"{taskToRemove.GetID} has successfully been removed");
            tasks.Remove(taskToRemove);
            // If this task was depended on by another task, then it will be removed from the list of dependencies from those respective tasks. 
            foreach (Task task in tasks)
            {
                if (task.SeeDependencies.Contains(taskToRemove.GetID)) {
                    task.SeeDependencies.Remove(taskToRemove.GetID);
                }
            }
        }

        /// <summary>
        /// Save file. All tasks added to the program will be added to the save file. It is in the format: [name], [executionTime], [dependentask], ...
        /// Or if there are no dependent tasks: [name], [executionTime]
        /// </summary>
        public void SaveMainFile()
        {
            RemoveUnmadeTasks();
            using (StreamWriter writer = new StreamWriter($"{sourceFile}.txt", false))
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    // Write job details. Each line of file is reserved for one task. 
                    writer.WriteLine(tasks[i].ToString());
                }
            }
        }
        /// <summary>
        /// This method is used to remove tasks that are referenced as being dependencies of other classes, but they themselves have not been added. 
        /// This is executed during the SaveMainFile, Sequence, and EarliestTimes method. 
        /// </summary>
        void RemoveUnmadeTasks()
        {
            // Scan each task in collection
            foreach (Task task in Tasks)
            {
                // Create list that will hold tasks that are marked as dependent but don't actually exist
                List<string> falseDependencies = new List<string>();
                foreach (string taskID in task.SeeDependencies) // Compare each task ID in the list of dependencies to the ID of every task in the collection
                {
                    bool exists = false;
                    foreach (Task task2 in Tasks)
                    {
                        if (task2.GetID != taskID)
                        {
                            continue;
                        }
                        else
                        {
                            // Mark task as exists and break from loop.
                            exists = true;
                            break;
                        }
                    }
                    if (exists == false)
                    {
                        // if task does not exist, then add to the list of dependencies. 
                        falseDependencies.Add(taskID);
                    }
                }
                foreach (string ID in falseDependencies)
                {
                    // Go through list of task dependencies, and then remove them from the list of dependencies held by the task object.
                    task.SeeDependencies.Remove(ID);
                }
            }
        }

        /// <summary>
        /// Allows user to add new task. Details such as name, execution time and list of tasks that it will be dependent on are added here. 
        /// </summary>
        public void AddTask()
        {
            // Enter name of task
            string taskID = EnterTaskID();
            // Enter execution time of task
            int executionTime = EnterExecutionTime();
            // Enter the list of tasks that this new task will be dependent on. 
            List<string> dependentTasks = EnterDependentTasks();
            Task newTask = new Task(taskID, executionTime, dependentTasks);
            // Add the task to the current collection. 
            tasks.Add(newTask);
            count++;
            Console.WriteLine($"{newTask.GetID} has been succssfully added");
        }
        /// <summary>
        /// Makes user enter a name for the task that is unique.
        /// </summary>
        /// <returns></returns>
        string EnterTaskID()
        {
            Console.WriteLine("Please enter the title of the task:");
            string taskID = Program.Input();
            taskID = Validate(taskID, "Please enter a valid name for the task");
            while (Search(taskID) != null)
            {
                // If name is not unique, user will be prompted to add a new one.
                Console.WriteLine("Please a task name that is unique");
                taskID = Program.Input();
            }
            return taskID;
        }
        /// <summary>
        /// Enter execution time of task. 
        /// </summary>
        /// <returns></returns>
        int EnterExecutionTime()
        {
            // Prompts user to enter the execution time. 
            Console.WriteLine("Please enter execution time for task. Execution time must be a positive integer (0 or higher) to be considered valid.");
            string executionTime = Program.Input();
            int intexecutionTime;
            // Execution time must be an integer. Checks validity of user input
            while (string.IsNullOrWhiteSpace(executionTime) || int.TryParse(executionTime, out intexecutionTime) == false || intexecutionTime < 0)
            {
                // Forces user to enter a valid input, which must be an integer. 
                Console.WriteLine("Please enter valid execution time for task");
                executionTime = Program.Input();
            }
            return intexecutionTime;
        }

        /// <summary>
        /// Prompts user to enter the list of tasks this new task is dependent on. The list does not need to contain tasks that have already been added. 
        /// They can be added later. But they need to be added if the user wants to use the sequence or earliest time functions.
        /// </summary>
        /// <returns></returns>
        List<string> EnterDependentTasks()
        {
            List<string> dependentTasks;
            Console.WriteLine(@"    
    Please enter IDs of dependent tasks.
    Leave blank if there are no dependent tasks.
    Place a comma between each ID. 
    For multiple tasks, ""T1,T2,T3"" is acceptable. 
    The tasks does not need to exist already, 
    but please add them before using the sequence and 
    earliest times functions.
    ");
            // If user does not enter any data, then no dependent tasks will be added. 
            string input = Program.Input().Trim();
            if (input.ToLower() == (""))
            {
                return new List<string>();
            }
            Validate(input, "Please enter a valid list of dependent tasks");
            if (input.Contains(','))
            {
                dependentTasks = new List<string>();
                string[] splitInput = input.Split(',');
                for (int i = 0; i < splitInput.Length; i++)
                {
                    // If input for any given dependent task has an invalid name. Currently, a blank input is considered to be invalid. 
                    if (splitInput[i].Trim() == "")
                    {
                        // This nested method is used to determine the string that would describe the position of a task that has been added but is invalid. 
                        // For example, if the first dependent task added by the user has an invalid name, the program will say: 
                        // The 1st dependent task you added has an invalid name, please enter a valid name. 
                        string Nth()
                        {
                            if (i == 0)
                            {
                                return "1st";
                            }
                            else if (i == 1)
                            {
                                return "2nd";
                            }
                            else if (i == 2)
                            {
                                return "3rd";
                            }
                            else
                            {
                                return $"{i + 1}th";
                            }
                        }
                        string position = Nth();
                        Console.WriteLine($"The {position} dependent task you added has an invalid name, please enter a valid name");
                        // Retrieve valid input from user. 
                        input = Validate(Program.Input().Trim(), "Please enter a valid name");
                        // Add to list of dependent tasks. 
                        dependentTasks.Add(input.Trim());
                    }
                    else
                    {
                        // Add to list of dependent tasks if the name is valid.
                        dependentTasks.Add(splitInput[i].Trim());
                    }
                }
            }
            else
            {
                dependentTasks = new List<string>();
                // If only one task is entered
                dependentTasks.Add(input);
            }
            return dependentTasks;
        }
        /// <summary>
        /// Used to see if a given input is valid. User is forced to re enter a valid input. Error message is situational depending on which method call is from. 
        /// Hence, error message is also supplied to this method and printed accordingly. 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        string Validate(string input, string errorMessage)
        {
            // While input is still invalid. 
            while (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine(errorMessage);
                input = Program.Input();
            }
            return input;
        }
       
        /// <summary>
        /// Used to show the sequence the tasks will be executed in, from first to last. 
        /// </summary>
        public void Sequence() 
        {
            // Dependent tasks that do not exist will be removed. 
            RemoveUnmadeTasks();
            Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
            // Add tasks and their depeendencies to a dictionary to be processed using a topological sort algorithm.
            foreach (Task task in Tasks)
            {
                keyValuePairs.Add(task.GetID, task.SeeDependencies.ToList());
            }
            // Call method to find order of execution for algorithm. 
            var sequence = FindTaskSequence(keyValuePairs);
            // Save data to a new file called sequence.txt. 
            using (StreamWriter writer = new StreamWriter("Sequence.txt"))
            {
                // write the task sequence to the file
                writer.WriteLine(string.Join(", ", sequence));
            }
            // Output order to user
            Console.WriteLine(string.Join(", ", sequence));
        }
        // TopologicalSort algorithm used to find the sequence
        List<string> FindTaskSequence(Dictionary<string, List<string>> graph)
        {
            // list to store the sorted task sequence
            List<string> sequence = new List<string>();

            // vertex is removed in each iteration until empty. 
            while (graph.Count > 0)
            {
                // variable to store the vertex to remove in each iteration
                string independentNode =null;
                foreach (var node in graph)
                {
                    // check if the vertex has 0 dependencies
                    if (node.Value.Count == 0)
                    {
                        // vertex that needs to be removed is stored. 
                        independentNode = node.Key;
                        break;
                    }
                }
                // if there is no independent vertex, there must be a circular dependency. 
                if (independentNode == null)
                {
                    // no vertex with no dependencies found
                    Console.WriteLine("The graph contains a cycle, and therefore cannot be sorted topologically.");
                    break;
                }
                // iterate over the graph
                foreach (var vertex in graph)
                {
                    // remove the independent vertex from the list of dependencies 
                    if (vertex.Value.Contains(independentNode))
                    {
                        vertex.Value.Remove(independentNode);
                    }
                }
                // add the vertex with no dependencies to the task sequence.
                sequence.Add(independentNode);
                // remove the vertex from the graph since it has already been sorted. 
                graph.Remove(independentNode);
            }
            // return the sorted sequence. 
            return sequence;
        }
        /// <summary>
        /// Method used to find the time needed for task to begin executing. 
        /// </summary>
        public void EarliestTimes() 
        {
            // Remove tasks that are in lists of dependencies but do not exist. 
            RemoveUnmadeTasks();
            // Create a dictionary to store tasks and their respective lists of dependencies. 
            Dictionary<string, List<string>> keyValuePairs = new Dictionary<string, List<string>>();
            foreach (Task task in Tasks)
            {
                keyValuePairs.Add(task.GetID, task.SeeDependencies.ToList());
            }
            // Find the earliest time a task can be executed. 
            var taskSequence = FindEarliestTimes(keyValuePairs);

            using (StreamWriter writer = new StreamWriter("EarliestTimes.txt"))
            {
                // write the task sequence to the file
                foreach(var task in taskSequence)
                {
                    string earliestTime = string.Join(", ", task);
                    string output = earliestTime.Substring(1,earliestTime.Length-2);
                    writer.WriteLine(output);
                }
                
            }
            // Write output of sequence
            Console.WriteLine(string.Join(", ", taskSequence));
        }
        /// <summary>
        /// Used to find the earliest time a task can be executed. Dependent tasks will always start after the tasks they are dependent on are finished. 
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        Dictionary<string, int> FindEarliestTimes(Dictionary<string, List<string>> graph)
        {
            // Dictionary to store the taskIDs and the earliest times they start. 
            Dictionary<string, int> earliestTimes = new Dictionary<string, int>();
            foreach (Task task in Tasks)
            {
                // Set each earliest time to 0 initially. 
                earliestTimes[task.GetID]=0;
            }
            // Process each vertex until non are left. Vertices are removed after they are processed. 
            while (graph.Count > 0)
            {
                string independentVertex = null;
                foreach (var vertex in graph)
                {
                    // check if the vertex has 0 dependencies
                    if (vertex.Value.Count == 0)
                    {
                        // vertex that needs to be removed is stored. 
                        independentVertex = vertex.Key;
                        break;
                    }
                }
                // if there is no independent vertex, there must be a circular dependency. 
                if (independentVertex == null)
                {
                    // no vertex with no dependencies found
                    Console.WriteLine("The graph contains a cycle, or a dependent task does not exist. In the latter case, please add that task to continue. Topological sort is not possible.");
                    break;
                }
                else
                {
                    // See which task has the vertex as a dependency. 
                    foreach (Task task in Tasks)
                    {
                        if (task.SeeDependencies.Contains(independentVertex))
                        {
                            // task that is dependent on this vertex will have its dependency time increased based on the execution time of the task being processed. 
                            int dependencyTime = earliestTimes[independentVertex] + Tasks.First(job => job.GetID == independentVertex).TaskExecutionTime;

                            if (dependencyTime > earliestTimes[task.GetID])
                            {
                                earliestTimes[task.GetID] = dependencyTime;
                            }
                        }
                    }
                    foreach (var vertex in graph)
                    {
                        // remove the independent vertex from the list of vertices. 
                        if (vertex.Value.Contains(independentVertex))
                        {
                            vertex.Value.Remove(independentVertex);
                        }
                    }
                }
                // remove the independent vertex from the graph
                graph.Remove(independentVertex);
            }
            return earliestTimes;
        }
    }
}



